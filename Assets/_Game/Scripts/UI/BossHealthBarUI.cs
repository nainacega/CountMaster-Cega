using UnityEngine;
using UnityEngine.UI;

// Level 3 only. Shows boss HP as a horizontal fill bar.
public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    // The Image set to Filled / Horizontal used as the red bar.
    [SerializeField] private Image fillImage;

    // Sets the fill amount (0 = empty, 1 = full). Called by BossController.
    public void SetFill(float normalized)
    {
        // Clamp and apply the fill.
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}
