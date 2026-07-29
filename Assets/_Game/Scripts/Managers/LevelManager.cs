using UnityEngine;
using UnityEngine.SceneManagement;

// Singleton that handles loading, reloading and advancing between levels.
public class LevelManager : MonoBehaviour
{
    // The one and only instance of this manager.
    public static LevelManager Instance;

    // Scene names in build order. Set these in the Inspector.
    [SerializeField] private string[] levelSceneNames =
        { "Level1", "Level2" };

    // Index of the level currently loaded (0-based).
    [SerializeField] private int currentLevelIndex = 0;

    // Sets up the singleton and keeps it alive across scene loads.
    private void Awake()
    {
        // Enforce a single persistent instance.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Keep the tracked index correct even when a level is opened
        // directly in the editor instead of reached through LoadNextLevel.
        SyncIndexToActiveScene();
    }

    // Matches currentLevelIndex to whichever level scene is actually open.
    private void SyncIndexToActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        for (int i = 0; i < levelSceneNames.Length; i++)
        {
            if (levelSceneNames[i] == sceneName)
            {
                currentLevelIndex = i;
                return;
            }
        }
    }

    // 1-based level number for UI display.
    public int GetCurrentLevelNumber()
    {
        return currentLevelIndex + 1;
    }

    // Loads the next level, or loops back to the first after the last one.
    public void LoadNextLevel()
    {
        currentLevelIndex++;
        // Wrap around if we passed the final level.
        if (currentLevelIndex >= levelSceneNames.Length)
        {
            currentLevelIndex = 0;
        }
        SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
    }

    // Reloads the current level (used after a loss).
    public void ReloadLevel()
    {
        SceneManager.LoadScene(levelSceneNames[currentLevelIndex]);
    }

    // Loads a specific level by its index (used by a menu, if any).
    public void LoadLevel(int index)
    {
        // Ignore out-of-range requests.
        if (index < 0 || index >= levelSceneNames.Length) return;
        currentLevelIndex = index;
        SceneManager.LoadScene(levelSceneNames[index]);
    }
}
