using UnityEngine;

// Represents a single blue character inside the player crowd.
// Deaths are instant: the character disappears and returns to the pool the
// moment it is killed (no lingering death animation).
public class PlayerCharacter : MonoBehaviour
{
    [Header("References")]
    // Animator that drives Idle / Run states.
    [SerializeField] private Animator animator;

    // Cached name of the running bool parameter on the Animator.
    private const string RunParam = "IsRunning";

    // Tells the character to start or stop the run animation.
    public void SetRunning(bool isRunning)
    {
        // Guard against a missing animator reference.
        if (animator != null) animator.SetBool(RunParam, isRunning);
    }

    // Kills this character: removes it from the crowd and recycles it
    // immediately.
    public void Die()
    {
        // Remove from the crowd list immediately so counts stay correct.
        CrowdManager.Instance.UnregisterCharacter(this);
        // Reset running state for the next time it is reused.
        SetRunning(false);
        ObjectPool.Instance.Return(this);
    }
}
