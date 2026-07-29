using System.Collections.Generic;
using UnityEngine;

// Simple object pool for PlayerCharacter instances.
// Pre-warms a set of characters at scene start so we never
// call Instantiate/Destroy during gameplay (better for mobile).
public class ObjectPool : MonoBehaviour
{
    // The one and only instance of this pool.
    public static ObjectPool Instance;

    [Header("Pool Settings")]
    // The PlayerCharacter prefab to pool.
    [SerializeField] private PlayerCharacter characterPrefab;
    // How many characters to create up front.
    [SerializeField] private int prewarmCount = 300;

    // The list of currently inactive (available) characters.
    private readonly Queue<PlayerCharacter> available =
        new Queue<PlayerCharacter>();

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

    // Creates the initial batch of pooled characters.
    private void Start()
    {
        // Build the requested number of inactive characters.
        for (int i = 0; i < prewarmCount; i++)
        {
            PlayerCharacter pc = Instantiate(characterPrefab, transform);
            pc.gameObject.SetActive(false);
            available.Enqueue(pc);
        }
    }

    // Returns an available character, expanding the pool if empty.
    public PlayerCharacter Get()
    {
        PlayerCharacter pc;
        // Grow the pool on demand if we ran dry.
        if (available.Count > 0)
        {
            pc = available.Dequeue();
        }
        else
        {
            pc = Instantiate(characterPrefab, transform);
        }
        pc.gameObject.SetActive(true);
        return pc;
    }

    // Returns a character back to the pool for reuse.
    public void Return(PlayerCharacter pc)
    {
        // Deactivate and store it for the next Get().
        pc.gameObject.SetActive(false);
        pc.transform.SetParent(transform);
        available.Enqueue(pc);
    }
}
