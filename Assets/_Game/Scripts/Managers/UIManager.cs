using TMPro;
using UnityEngine;

// Singleton that shows and hides all the UI panels and widgets.
public class UIManager : MonoBehaviour
{
    // The one and only instance of this manager.
    public static UIManager Instance;

    [Header("UI References (assign in Inspector)")]
    // The "Hold & Move" prompt shown at the start.
    [SerializeField] private GameObject holdMovePanel;
    // Panel shown when the player wins.
    [SerializeField] private GameObject winPanel;
    // Panel shown when the player loses.
    [SerializeField] private GameObject losePanel;
    // Root object of the boss health bar (level 3 only).
    [SerializeField] private GameObject bossHealthBarRoot;
    // Label showing the current level number.
    [SerializeField] private TMP_Text levelText;

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

    // Ensures panels start in the right visibility at scene load.
    private void Start()
    {
        // Hold & Move visible, everything else hidden.
        if (holdMovePanel != null) holdMovePanel.SetActive(true);
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        if (bossHealthBarRoot != null) bossHealthBarRoot.SetActive(false);
        // Show which level this is.
        if (levelText != null && LevelManager.Instance != null)
        {
            levelText.text = "LEVEL " + LevelManager.Instance.GetCurrentLevelNumber();
        }
    }

    // Hooked to the win panel's Next Level button.
    public void OnNextLevelButton()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.LoadNextLevel();
    }

    // Hooked to the lose panel's Retry button.
    public void OnRetryButton()
    {
        if (LevelManager.Instance != null) LevelManager.Instance.ReloadLevel();
    }

    // Hides the "Hold & Move" prompt (called on first touch).
    public void HideHoldMove()
    {
        if (holdMovePanel != null) holdMovePanel.SetActive(false);
    }

    // Shows the win panel.
    public void ShowWinPanel()
    {
        if (winPanel != null) winPanel.SetActive(true);
    }

    // Shows the lose panel.
    public void ShowLosePanel()
    {
        Invoke("LostPanel", 2f);
    }

    void LostPanel()
    {
        losePanel.SetActive(true);
    }

    // Reveals the boss health bar (level 3 arena entry).
    public void ShowBossHealthBar()
    {
        if (bossHealthBarRoot != null) bossHealthBarRoot.SetActive(true);
    }
}
