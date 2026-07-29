using UnityEngine;
using UnityEngine.UI;
using TMPro;

// A world-space bubble that follows the crowd root and shows the count.
public class CountBubbleUI : MonoBehaviour
{
    [Header("References")]
    // The crowd root to follow.
    [SerializeField] private Transform crowdRoot;
    // Text component showing the number (TextMeshPro).
    [SerializeField] private TMP_Text countText;
    // Height above the crowd root to float the bubble.
    [SerializeField] private float heightOffset = 2.2f;

    // Follows the crowd root each frame and faces the camera.
    private void LateUpdate()
    {
        // Keep the bubble above the crowd root.
        if (crowdRoot != null)
        {
            transform.position = crowdRoot.position + Vector3.up * heightOffset;
        }
        // Billboard the bubble toward the camera.
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    // Updates the displayed count. Called by CrowdManager.
    public void SetCount(int count)
    {
        if (countText != null) countText.text = count.ToString();
    }
}
