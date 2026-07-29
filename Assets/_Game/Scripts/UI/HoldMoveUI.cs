using UnityEngine;

// Shows the "Hold & Move" prompt and hides itself on first touch.
// UIManager also hides it via HideHoldMove(); this script is a simple
// self-contained fallback you can attach directly to the prompt object.
// While the prompt is visible, the tutorial hand slides left and right
// as a drag hint.
public class HoldMoveUI : MonoBehaviour
{
    [Header("Hand Animation")]
    // The tutorial hand icon that slides left and right.
    [SerializeField] private RectTransform hand;
    // How far the hand travels to each side, in canvas pixels.
    [SerializeField] private float swingDistance = 150f;
    // Full left-right-left cycles per second.
    [SerializeField] private float swingSpeed = 0.6f;

    // The hand's resting anchored X, captured at startup.
    private float handBaseX;

    // Remembers where the hand rests so the swing centres on it.
    private void Awake()
    {
        if (hand != null) handBaseX = hand.anchoredPosition.x;
    }

    // Animates the hand, and hides the prompt once play begins.
    private void Update()
    {
        // Once play has begun, hide this prompt and stop updating.
        if (GameManager.Instance.GetState() != GameManager.GameState.Idle)
        {
            gameObject.SetActive(false);
            return;
        }

        // Slide the hand smoothly left and right as a drag hint.
        if (hand != null)
        {
            Vector2 p = hand.anchoredPosition;
            p.x = handBaseX
                + Mathf.Sin(Time.time * swingSpeed * Mathf.PI * 2f) * swingDistance;
            hand.anchoredPosition = p;
        }
    }
}
