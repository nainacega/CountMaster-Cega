using System.Collections.Generic;
using UnityEngine;

// Singleton that owns the list of active player characters and performs
// all count operations (add / subtract / multiply / divide).
public class CrowdManager : MonoBehaviour
{
    // The one and only instance of this manager.
    public static CrowdManager Instance;

    [Header("References")]
    // Formation helper that arranges characters into a disc.
    [SerializeField] private CrowdFormation formation;
    // World-space count bubble UI following the crowd.
    [SerializeField] private CountBubbleUI countBubble;

    [Header("Start Settings")]
    // How many characters the crowd begins with.
    [SerializeField] private int startCount = 1;
    // Safety cap so the pool and formation never explode.
    [SerializeField] private int maxCount = 300;

    // The list of currently active characters.
    private readonly List<PlayerCharacter> characters =
        new List<PlayerCharacter>();

    // True while an enemy battle is steering the characters; the disc
    // formation is suspended so it doesn't fight the battle movement.
    private bool inBattle;

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

    // Spawns the starting characters once the pool is ready.
    private void Start()
    {
        // Add the initial crowd members.
        AddCharacters(startCount);
    }

    // Keeps the disc formation updated every frame, except while an enemy
    // battle is driving the characters itself.
    private void Update()
    {
        if (!inBattle) formation.UpdatePositions(characters);
    }

    // Called by EnemyGroup when a battle starts/ends. While true, the disc
    // formation is suspended; when it flips back to false the steering pulls
    // the survivors back into a circle on its own.
    public void SetInBattle(bool value)
    {
        inBattle = value;
    }

    // Returns the current number of active characters.
    public int GetCount()
    {
        return characters.Count;
    }

    // Adds a number of characters from the pool to the crowd.
public void AddCharacters(int amount)
    {
        // Add one at a time, respecting the max cap.
        for (int i = 0; i < amount; i++)
        {
            if (characters.Count >= maxCount) break;
            PlayerCharacter pc = ObjectPool.Instance.Get();
            // Parent under the crowd root so it moves with the group.
            pc.transform.SetParent(transform, false);
            // Tiny random offset so brand-new characters never sit exactly on
            // top of another -- CrowdFormation's separation push has no
            // direction to resolve a perfect overlap.
            Vector2 jitter = Random.insideUnitCircle * 0.05f;
            pc.transform.localPosition = new Vector3(jitter.x, 0f, jitter.y);
            pc.SetRunning(GameManager.Instance.GetState()
                == GameManager.GameState.Playing);
            characters.Add(pc);
        }
        RefreshAfterChange();
    }

    // Removes a number of characters by killing them.
public void RemoveCharacters(int amount)
    {
        // Kill from the end of the list.
        for (int i = 0; i < amount; i++)
        {
            if (characters.Count == 0) break;
            PlayerCharacter pc = characters[characters.Count - 1];
            // Die() calls UnregisterCharacter which updates the list.
            pc.Die();
        }
        // Note: Die() already refreshes via UnregisterCharacter.
    }

    // Multiplies the crowd size by a factor (used by x gates).
    public void MultiplyCrowd(float factor)
    {
        int current = characters.Count;
        int target = Mathf.FloorToInt(current * factor);
        int toAdd = target - current;
        // Only positive growth is expected from multiply gates.
        if (toAdd > 0) AddCharacters(toAdd);
    }

    // Divides the crowd size by a divisor (used by divide gates).
    public void DivideCrowd(float divisor)
    {
        // Ignore invalid divisors.
        if (divisor <= 0f) return;
        int current = characters.Count;
        int target = Mathf.FloorToInt(current / divisor);
        int toRemove = current - target;
        if (toRemove > 0) RemoveCharacters(toRemove);
    }

    // Called by PlayerCharacter.Die() to remove itself from the list.
public void UnregisterCharacter(PlayerCharacter pc)
    {
        // Remove and refresh the layout / count bubble.
        if (characters.Remove(pc))
        {
            RefreshAfterChange();
            // Losing the whole crowd is a loss.
            if (characters.Count == 0)
            {
                GameManager.Instance.Lose();
            }
        }
    }

// Removes a character from tracking without treating it as a death.
    // Used when a character graduates to a non-crowd sequence (e.g. the staircase
    // walk) so CrowdFormation stops repositioning it, without triggering a false Lose.
public void RemoveFromTracking(PlayerCharacter pc)
    {
        characters.Remove(pc);
    }


    // Hides the floating count bubble (called when the finish line is
    // crossed and the number is no longer meaningful).
    public void HideCountBubble()
    {
        if (countBubble != null) countBubble.gameObject.SetActive(false);
    }

    // Returns a copy of the current character list (for stairs / boss).
    public List<PlayerCharacter> GetCharacters()
    {
        return new List<PlayerCharacter>(characters);
    }

    // Recomputes the disc slots and updates the count bubble.
private void RefreshAfterChange()
    {
        if (countBubble != null) countBubble.SetCount(characters.Count);
    }
}
