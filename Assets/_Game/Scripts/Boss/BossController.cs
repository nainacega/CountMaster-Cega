using System.Collections;
using UnityEngine;

// Level 3 only. A large enemy with a health bar. Each crowd character that
// reaches it deals one damage and is consumed. Win when boss HP hits 0;
// lose if the crowd empties first.
public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    // Total boss health (number of hits it can take).
    [SerializeField] private int maxHealth = 70;
    // Seconds between each crowd character striking the boss.
    [SerializeField] private float hitInterval = 0.05f;
    // Health bar UI to update each hit.
    [SerializeField] private BossHealthBarUI healthBar;

    // Current boss health.
    private int currentHealth;
    // Ensures the fight only begins once.
    private bool fightStarted;

    // Called by FinishLine when the crowd enters the arena.
    public void BeginFight()
    {
        // Guard against multiple starts.
        if (fightStarted) return;
        fightStarted = true;

        currentHealth = maxHealth;
        UIManager.Instance.ShowBossHealthBar();
        if (healthBar != null) healthBar.SetFill(1f);

        StartCoroutine(FightRoutine());
    }

    // Consumes one crowd member per tick, damaging the boss each time.
    private IEnumerator FightRoutine()
    {
        // Fight until the boss dies or the crowd is wiped out.
        while (currentHealth > 0 && CrowdManager.Instance.GetCount() > 0)
        {
            // One character strikes and is consumed.
            CrowdManager.Instance.RemoveCharacters(1);
            currentHealth--;

            // Update the health bar fill amount.
            if (healthBar != null)
            {
                healthBar.SetFill((float)currentHealth / maxHealth);
            }

            yield return new WaitForSeconds(hitInterval);
        }

        // Decide the outcome.
        if (currentHealth <= 0)
        {
            GameManager.Instance.Win();
        }
        else
        {
            GameManager.Instance.Lose();
        }
    }
}
