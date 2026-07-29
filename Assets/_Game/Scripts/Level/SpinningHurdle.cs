using UnityEngine;

// A rotating purple spike cylinder. Any player character that touches it dies.
public class SpinningHurdle : MonoBehaviour
{
    [Header("Rotation")]
    // Degrees per second around the Y axis.
    [SerializeField] private float rotateSpeed = 120f;

    // Spins the hurdle every frame.
    private void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }

    // Kills any individual player character that collides with the spikes.
    private void OnTriggerEnter(Collider other)
    {
        // Only react to individual crowd characters.
        PlayerCharacter pc = other.GetComponent<PlayerCharacter>();
        if (pc != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(AudioManager.SfxType.DeadPlayer);
            }
            SplatPool.Instance.SpawnPlayerSplat(pc.transform.position);
            pc.Die();
        }
    }
}
