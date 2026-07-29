using UnityEngine;

// Reads touch / mouse drag input, auto-runs the crowd root forward,
// and steers it left / right within the lane.
public class PlayerController : MonoBehaviour
{
    [Header("Movement Tuning")]
    // Forward speed once the game is playing.
    [SerializeField] private float forwardSpeed = 6f;
    // How fast dragging moves the crowd sideways.
    [SerializeField] private float steerSpeed = 12f;
    // Half the lane width; clamps left/right position.
    [SerializeField] private float laneHalfWidth = 3f;

    // Screen X of the finger/mouse on the previous frame.
    private float lastPointerX;
    // Whether we currently have a finger/mouse held down.
    private bool isDragging;
    // Whether movement and input are frozen (e.g. during an enemy battle).
    private bool isPaused;

    // Freezes or resumes forward movement and steering. The camera is a
    // child of this transform, so it stops right along with it.
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }

    // Reads input each frame and moves the crowd root.
private void Update()
    {
        // Only respond while idle (to start) or actively playing.
        GameManager.GameState state = GameManager.Instance.GetState();
        if (state == GameManager.GameState.Win
            || state == GameManager.GameState.Lose) return;

        // Frozen during an enemy battle -- no steering, no forward movement.
        if (isPaused) return;

        HandlePointer(state);

        // Auto-run forward only after play has started.
        if (state == GameManager.GameState.Playing)
        {
            transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime,
                Space.World);
        }
    }

    // Handles press / drag / release for both touch and mouse.
    private void HandlePointer(GameManager.GameState state)
    {
        // Press began this frame.
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastPointerX = Input.mousePosition.x;
            // First touch starts the game.
            if (state == GameManager.GameState.Idle)
            {
                GameManager.Instance.StartPlaying();
                SetCrowdRunning(true);
            }
        }
        // Press released this frame.
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        // While held, steer based on horizontal finger movement.
        if (isDragging)
        {
            float deltaX = Input.mousePosition.x - lastPointerX;
            lastPointerX = Input.mousePosition.x;

            // Convert screen delta into world sideways movement.
            float move = (deltaX / Screen.width) * steerSpeed;
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x + move, -laneHalfWidth, laneHalfWidth);
            transform.position = pos;
        }
    }

    // Tells every crowd character to play the run animation.
    private void SetCrowdRunning(bool running)
    {
        // Ask each active character to switch animation state.
        foreach (PlayerCharacter pc in CrowdManager.Instance.GetCharacters())
        {
            pc.SetRunning(running);
        }
    }
}
