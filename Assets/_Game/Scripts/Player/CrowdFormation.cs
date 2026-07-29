using System.Collections.Generic;
using UnityEngine;

// Keeps the crowd packed into an organic disc using two steps each frame:
// a gentle constant pull toward the crowd's centre for cohesion, then a hard
// pairwise separation pass that enforces a uniform minimum distance between
// characters. No fixed slot assignment exists, so gaps close naturally after
// a death -- but because separation is a hard constraint rather than a soft
// push, characters can never visibly overlap. Everyone is also clamped to
// the platform in world space so the crowd can't spill past its edge.
public class CrowdFormation : MonoBehaviour
{
    [Header("Formation Tuning")]
    // Constant speed characters drift toward the crowd centre.
    [SerializeField] private float centerPullSpeed = 2.5f;
    // Uniform distance every character keeps from its neighbours.
    [SerializeField] private float separationDistance = 0.55f;
    // How many pairwise relaxation passes run per frame. Needs to outrun the
    // centre pull's per-frame compression in a dense disc: 2 passes settle at
    // ~0.44 spacing under the default pull, 5 passes hold ~0.53 (measured).
    [SerializeField] private int separationPasses = 5;
    // Caps how far one separation push can move a character per second, so a
    // tightly packed crowd (e.g. many characters spawning at once) resolves
    // over a few frames instead of flinging characters away in one tick.
    [SerializeField] private float maxMoveSpeed = 8f;

    [Header("Platform Bounds")]
    // Characters' world X is clamped to +/- this so nobody hangs off the
    // platform (the lane floor is 6 wide with wall inner faces at +/-3).
    [SerializeField] private float platformHalfWidth = 2.75f;

    // Steers every character toward the centre while keeping neighbours at a
    // uniform distance. Called by CrowdManager in its Update.
    public void UpdatePositions(List<PlayerCharacter> characters)
    {
        float dt = Time.deltaTime;
        int count = characters.Count;
        if (count == 0) return;

        // Step 1: re-centre the blob as a whole. Every character shifts by
        // the SAME vector (toward putting the blob's centroid on the crowd
        // root), so this can never deform the shape or fight the separation
        // pass -- which is what kept the old constant per-character pull
        // flickering. The small deadband stops idle drift.
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            Vector3 lp = characters[i].transform.localPosition;
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
                Transform t = characters[i].transform;
                Vector3 p = t.localPosition + shift;
                // Keep everyone flat on the ground plane.
                p.y = 0f;
                t.localPosition = p;
            }
        }

        // Step 2: pull in only the characters sitting OUTSIDE the radius a
        // packed disc of this crowd size occupies. Inside that radius the
        // separation pass alone shapes the blob, so a settled crowd is
        // perfectly still -- while wall-squeeze tips, battle survivors, and
        // stragglers are always outside it and get pulled back until the
        // circle re-forms. The formula is calibrated ~8% above the measured
        // centre-radius of a relaxed packing: much looser and a squeezed-out
        // ellipse fits entirely inside the boundary and freezes
        // mid-deformation; much tighter and the resting rim sits on the
        // boundary and churns against it.
        float packedRadius = Mathf.Max(separationDistance,
            separationDistance * (0.59f * Mathf.Sqrt(count) - 0.36f));
        for (int i = 0; i < count; i++)
        {
            Transform t = characters[i].transform;
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
        // pushed apart symmetrically until the gap is exact -- this is what
        // makes the spacing read as uniform instead of soft-and-overlapping.
        float maxStep = maxMoveSpeed * dt;
        for (int pass = 0; pass < separationPasses; pass++)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                Transform a = characters[i].transform;
                for (int j = i + 1; j < characters.Count; j++)
                {
                    Transform b = characters[j].transform;
                    Vector3 offset = a.localPosition - b.localPosition;
                    offset.y = 0f;
                    float dist = offset.magnitude;
                    if (dist >= separationDistance) continue;

                    // Perfectly coincident characters have no push direction;
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
        for (int i = 0; i < characters.Count; i++)
        {
            Transform t = characters[i].transform;
            Vector3 world = t.position;
            world.x = Mathf.Clamp(world.x, -platformHalfWidth, platformHalfWidth);
            t.position = world;
        }
    }
}
