using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Object pool for the death-splat decals left under fallen fighters during
// crowd battles. Splats never despawn during a level (no decay), so the
// pool's job is only to avoid Instantiate spikes mid-battle -- it pre-warms
// a batch and grows on demand.
public class SplatPool : MonoBehaviour
{
    // The one and only instance of this pool.
    public static SplatPool Instance;

    [Header("Pool Settings")]
    // The splat prefab (a flat sprite lying on the ground).
    [SerializeField] private SpriteRenderer splatPrefab;
    // How many splats to create up front.
    [SerializeField] private int prewarmCount = 100;

    [Header("Splat Colors")]
    // Player splats: random tint between these two blues.
    [SerializeField] private Color playerLight = new Color(0.35f, 0.60f, 1.00f);
    [SerializeField] private Color playerDark = new Color(0.10f, 0.20f, 0.70f);
    // Enemy splats: random tint between these two reds.
    [SerializeField] private Color enemyLight = new Color(1.00f, 0.45f, 0.40f);
    [SerializeField] private Color enemyDark = new Color(0.70f, 0.10f, 0.10f);

    [Header("Spawn Animation")]
    // Final uniform scale a splat pops out to.
    [SerializeField] private float targetScale = 0.2f;
    // Seconds the 0 -> targetScale ease-out pop takes.
    [SerializeField] private float scaleDuration = 0.25f;

    // The list of currently inactive (available) splats.
    private readonly Queue<SpriteRenderer> available =
        new Queue<SpriteRenderer>();

    // Counts spawned splats so overlapping ones can stack at slightly
    // different heights instead of z-fighting.
    private int spawnCounter;

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
    }

    // Creates the initial batch of pooled splats.
    private void Start()
    {
        for (int i = 0; i < prewarmCount; i++)
        {
            SpriteRenderer s = Instantiate(splatPrefab, transform);
            s.gameObject.SetActive(false);
            available.Enqueue(s);
        }
    }

    // Places a blue splat under a fallen player character.
    public void SpawnPlayerSplat(Vector3 position)
    {
        Spawn(position, playerLight, playerDark);
    }

    // Places a red splat under a fallen enemy.
    public void SpawnEnemySplat(Vector3 position)
    {
        Spawn(position, enemyLight, enemyDark);
    }

    // Takes a splat from the pool and stamps it on the ground.
    private void Spawn(Vector3 position, Color light, Color dark)
    {
        SpriteRenderer s;
        // Grow the pool on demand if we ran dry.
        if (available.Count > 0)
        {
            s = available.Dequeue();
        }
        else
        {
            s = Instantiate(splatPrefab, transform);
        }

        // Sit flat just above the floor; each splat gets a hair more height
        // than the last so overlapping splats never z-fight.
        spawnCounter++;
        float y = 0.02f + (spawnCounter % 64) * 0.0005f;
        s.transform.position = new Vector3(position.x, y, position.z);
        // Random spin around the up axis so identical sprites read as varied
        // (the prefab's base rotation lays the sprite flat).
        s.transform.rotation = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
        // Random tint between the light and dark shade.
        s.color = Color.Lerp(light, dark, Random.value);
        // Pop in from nothing to full size.
        s.transform.localScale = Vector3.zero;
        s.gameObject.SetActive(true);
        StartCoroutine(ScaleIn(s.transform));
    }

    // Grows a splat from zero to targetScale with an ease-out curve
    // (fast start, soft landing).
    private IEnumerator ScaleIn(Transform splat)
    {
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            float eased = 1f - (1f - t) * (1f - t) * (1f - t);
            splat.localScale = Vector3.one * (targetScale * eased);
            yield return null;
        }
        splat.localScale = Vector3.one * targetScale;
    }
}
