using UnityEngine;

// Singleton that owns the overall game state for a single level.
// States: Idle (waiting for first touch), Playing, Win, Lose.
public class GameManager : MonoBehaviour
{
    // The one and only instance of this manager.
    public static GameManager Instance;

    public enum GameState { Idle, Playing, Win, Lose }

    // Current state, visible in the Inspector for debugging.
    [SerializeField] private GameState state = GameState.Idle;

    // Sets up the singleton reference. Runs before Start.
    private void Awake()
    {
        // If another instance already exists, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Returns the current game state.
    public GameState GetState()
    {
        return state;
    }

    // Called by PlayerController on the very first touch to begin play.
    public void StartPlaying()
    {
        // Only move from Idle into Playing once.
        if (state == GameState.Idle)
        {
            state = GameState.Playing;
            UIManager.Instance.HideHoldMove();
        }
    }

    // Called when the player wins (finished stairs or killed the boss).
    public void Win()
    {
        // Guard so we only trigger the win flow once.
        if (state == GameState.Win || state == GameState.Lose) return;
        state = GameState.Win;
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(AudioManager.SfxType.WinSiren);
        }
        UIManager.Instance.ShowWinPanel();
    }

    // Called when the player loses (crowd wiped out).
    public void Lose()
    {
        // Guard so we only trigger the lose flow once.
        if (state == GameState.Win || state == GameState.Lose) return;
        state = GameState.Lose;
        UIManager.Instance.ShowLosePanel();
    }
}
