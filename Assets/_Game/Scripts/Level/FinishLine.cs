using System.Collections;
using UnityEngine;

// Detects the crowd crossing the checkered line. On levels 1 & 2 the crowd
// root (and the camera, which is its child) stops at the line while the
// characters themselves keep running ahead, and the level-complete panel
// appears shortly after. On level 3 it activates the boss fight instead.
public class FinishLine : MonoBehaviour
{
    [Header("End-of-level Settings")]
    // Boss controller for level 3 (leave empty on levels 1 & 2).
    [SerializeField] private BossController boss;
    // How fast the characters keep running past the line.
    [SerializeField] private float runOffSpeed = 6f;
    // Delay before the level-complete panel appears.
    [SerializeField] private float winPanelDelay = 2f;

    // Ensures the finish logic runs only once.
    private bool finished;

    // Fires when the crowd root crosses the finish line.
    private void OnTriggerEnter(Collider other)
    {
        // Only the crowd root triggers the finish, and only once.
        if (finished) return;
        if (!other.CompareTag("Player")) return;
        finished = true;

        // Level 3: hand over to the boss fight.
        if (boss != null)
        {
            boss.BeginFight();
            return;
        }

        // Levels 1 & 2: freeze the crowd root so the camera holds at the
        // line, and suspend the disc steering so it doesn't drag the
        // characters back toward the stopped root while they run off.
        PlayerController playerController = CrowdManager.Instance.GetComponent<PlayerController>();
        if (playerController != null) playerController.SetPaused(true);
        CrowdManager.Instance.SetInBattle(true);
        // The count stops mattering past the line -- hide the bubble.
        CrowdManager.Instance.HideCountBubble();
        // Celebration flair the moment the line is crossed.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Play(AudioManager.SfxType.WinFireworks);
        }
        StartCoroutine(RunOffRoutine());
    }

    // Keeps the characters running forward past the camera; shows the
    // level-complete panel after the delay while they jog on behind it.
    private IEnumerator RunOffRoutine()
    {
        float elapsed = 0f;
        bool won = false;
        while (true)
        {
            foreach (PlayerCharacter pc in CrowdManager.Instance.GetCharacters())
            {
                pc.transform.position +=
                    Vector3.forward * runOffSpeed * Time.deltaTime;
            }
            elapsed += Time.deltaTime;
            if (!won && elapsed >= winPanelDelay)
            {
                won = true;
                GameManager.Instance.Win();
            }
            yield return null;
        }
    }
}
