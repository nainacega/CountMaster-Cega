using UnityEngine;

// Central sound-effect player. Uses a single AudioSource and PlayOneShot
// only, so effects overlap freely and nothing ever interrupts anything.
// Each effect type owns a set of clip variations -- one is picked at random
// every play -- and its own volume.
public class AudioManager : MonoBehaviour
{
    // Identifies which sound effect to play. Mirrors the clip sets in
    // Assets/_Ripped/AudioClip. Extend as new sounds are added.
    public enum SfxType
    {
        // DEAD_PLAYER_01..12 -- a battle exchange where both fighters die.
        DeadPlayer,
        // DEAD_PLAYER_LAST -- reserved for the final crowd member's death.
        DeadPlayerLast,
        // Enemy_Crowd_Win -- the enemy crowd wins the battle.
        EnemyCrowdWin,
        // Gate Trigger -- the crowd consumes a gate.
        GateTrigger,
        // RUN_01..08 -- footsteps, repeated on an interval while running.
        Run,
        // Win_Fireworks -- reserved for win celebration flair.
        WinFireworks,
        // Win_Siren -- played when the level is won.
        WinSiren,
    }

    // One entry per effect type: its clip variations and volume.
    [System.Serializable]
    public class SfxEntry
    {
        // Which effect this entry defines.
        public SfxType type;
        // Per-effect volume.
        [Range(0f, 1f)] public float volume = 1f;
        // One of these is picked at random each time the effect plays.
        public AudioClip[] clips;
    }

    // The one and only instance of this manager.
    public static AudioManager Instance;

    [Header("References")]
    // The single source every effect plays through.
    [SerializeField] private AudioSource sfxSource;

    [Header("Sound Effects")]
    // One entry per SfxType (set up in the Inspector).
    [SerializeField] private SfxEntry[] effects;

    [Header("Run Loop")]
    // Seconds between footstep plays while the crowd is running.
    [SerializeField] private float runClipInterval = 0.35f;

    // Time accumulated toward the next footstep play.
    private float runTimer;

    // Sets up the singleton reference.
    private void Awake()
    {
        // Enforce a single instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Fall back to a source on this object if none was wired.
        if (sfxSource == null) sfxSource = GetComponent<AudioSource>();
    }

    // Repeats the Run footsteps on the interval while the crowd is in its
    // running state (the game is in Playing -- from the first touch until
    // win/lose, which covers auto-run, battles, and the finish run-off).
    private void Update()
    {
        if (GameManager.Instance == null
            || GameManager.Instance.GetState() != GameManager.GameState.Playing)
        {
            runTimer = 0f;
            return;
        }

        runTimer += Time.deltaTime;
        if (runTimer >= runClipInterval)
        {
            runTimer -= runClipInterval;
            Play(SfxType.Run);
        }
    }

    // Plays a random variation of the given effect through the shared source.
    public void Play(SfxType type)
    {
        if (sfxSource == null) return;

        foreach (SfxEntry entry in effects)
        {
            if (entry.type != type) continue;
            if (entry.clips == null || entry.clips.Length == 0) return;

            AudioClip clip = entry.clips[Random.Range(0, entry.clips.Length)];
            if (clip != null) sfxSource.PlayOneShot(clip, entry.volume);
            return;
        }
    }
}
