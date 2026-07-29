using UnityEngine;

// Represents a single red enemy character in an enemy group.
// Deaths are instant: the enemy disappears the moment it is killed.
public class EnemyCharacter : MonoBehaviour
{
    [Header("References")]
    // Animator driving Idle / Running states.
    [SerializeField] private Animator animator;

    private const string RunParam = "IsRunning";

    // Tells the enemy to start or stop the run animation (e.g. charging into battle).
    public void SetRunning(bool isRunning)
    {
        if (animator != null) animator.SetBool(RunParam, isRunning);
    }

    // Removes this enemy from the scene immediately.
    public void Die()
    {
        Destroy(gameObject);
    }
}
