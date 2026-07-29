using System.Collections.Generic;
using UnityEngine;

// Keeps a stationary enemy group packed into an organic disc using the same
// two steps as CrowdFormation: a gentle constant pull toward the group's
// centre for cohesion, then a hard pairwise separation pass that enforces a
// uniform minimum distance -- so enemies never visibly overlap. Enemies are
// also clamped to the platform in world space.
public class EnemyFormation : MonoBehaviour
{
    [Header("Formation Tuning")]
    // Constant speed enemies drift toward the group centre.
    [SerializeField] private float centerPullSpeed = 2.5f;
    // Uniform distance every enemy keeps from its neighbours.
    [SerializeField] private float separationDistance = 0.55f;
    // How many pairwise relaxation passes run per frame. Needs to outrun the
    // centre pull's per-frame compression in a dense disc (see CrowdFormation).
    [SerializeField] private int separationPasses = 5;
    // Caps how far one separation push can move an enemy per second, so a
    // tightly packed group resolves over a few frames instead of exploding.
    [SerializeField] private float maxMoveSpeed = 8f;

    [Header("Platform Bounds")]
    // Enemies' world X is clamped to +/- this so nobody hangs off the
    // platform (the lane floor is 6 wide with wall inner faces at +/-3).
    [SerializeField] private float platformHalfWidth = 2.75f;

    // Steers every enemy toward the centre while keeping neighbours at a
    // uniform distance. Called by EnemyGroup in its Update.
    public void UpdatePositions(List<EnemyCharacter> enemies)
    {
        float dt = Time.deltaTime;
        int count = enemies.Count;
        if (count == 0) return;

        // Step 1: re-centre the blob as a whole -- every enemy shifts by the
        // SAME vector, so this can never deform the shape or fight the
        // separation pass (see CrowdFormation for the full reasoning).
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            Vector3 lp = enemies[i].transform.localPosition;
            lp.y = 0f;
            centroid += lp;
        }
        centroid /= count;
        float offsetMag = centroid.magnitude;
        if (offsetMag > 0.05f)
        {
            Vector3 shift = -centroid / offsetMag
                * Mathf.Min(centerPullSpeed * dt, offsetMag - 0.05f);
            for (int i = 0; i < count; i++)
            {
                Transform t = enemies[i].transform;
                Vector3 p = t.localPosition + shift;
                // Keep everyone flat on the ground plane.
                p.y = 0f;
                t.localPosition = p;
            }
        }

        // Step 2: pull in only the enemies sitting OUTSIDE the radius a
        // packed disc of this group size occupies. A settled group is
        // perfectly still; anyone scattered outside gets pulled back in.
        // Calibrated boundary -- see CrowdFormation for why.
        float packedRadius = Mathf.Max(separationDistance,
            separationDistance * (0.59f * Mathf.Sqrt(count) - 0.36f));
        for (int i = 0; i < count; i++)
        {
            Transform t = enemies[i].transform;
            Vector3 pos = t.localPosition;
            Vector3 toCenter = -pos;
            toCenter.y = 0f;
            float distToCenter = toCenter.magnitude;
            if (distToCenter <= packedRadius) continue;

            // Walk to the disc boundary, never past it (no overshoot churn).
            float step = Mathf.Min(centerPullSpeed * dt, distToCenter - packedRadius);
            pos += toCenter / distToCenter * step;
            pos.y = 0f;
            t.localPosition = pos;
        }

        // Step 2: hard separation. Any pair closer than separationDistance is
        // pushed apart symmetrically until the gap is exact.
        float maxStep = maxMoveSpeed * dt;
        for (int pass = 0; pass < separationPasses; pass++)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Transform a = enemies[i].transform;
                for (int j = i + 1; j < enemies.Count; j++)
                {
                    Transform b = enemies[j].transform;
                    Vector3 offset = a.localPosition - b.localPosition;
                    offset.y = 0f;
                    float dist = offset.magnitude;
                    if (dist >= separationDistance) continue;

                    // Perfectly coincident enemies have no push direction;
                    // derive a stable one from the pair's indices.
                    Vector3 dir = dist > 0.0001f
                        ? offset / dist
                        : new Vector3(Mathf.Cos(i * 2.4f + j), 0f, Mathf.Sin(i * 2.4f + j));
                    float push = Mathf.Min((separationDistance - dist) * 0.5f, maxStep);
                    a.localPosition += dir * push;
                    b.localPosition -= dir * push;
                }
            }
        }

        // Step 3: clamp everyone to the platform in world space.
        for (int i = 0; i < enemies.Count; i++)
        {
            Transform t = enemies[i].transform;
            Vector3 world = t.position;
            world.x = Mathf.Clamp(world.x, -platformHalfWidth, platformHalfWidth);
            t.position = world;
        }
    }
}
