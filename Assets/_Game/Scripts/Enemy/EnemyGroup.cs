using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A stationary red crowd. When the player crowd enters the trigger radius, a
// battle begins: fighters on both sides charge at their nearest living
// opponent (keeping spacing from their own allies so the crowds meet spread
// out instead of as two stacked columns), and whenever a player and an enemy
// close within killRange the pair trades kills 1-for-1. When one side is
// wiped out the battle ends and the surviving crowd regroups into its disc.
public class EnemyGroup : MonoBehaviour
{
    [Header("Battle Settings")]
    // How fast fighters on both sides run at each other.
    [SerializeField] private float chargeSpeed = 3.5f;
    // A player and an enemy this close kill each other (1-for-1 trade).
    [SerializeField] private float killRange = 0.6f;
    // Spacing fighters keep from allies while charging.
    [SerializeField] private float allySeparation = 0.45f;
    // Formation helper that packs the enemies into an organic disc.
    [SerializeField] private EnemyFormation formation;
    // World-space count bubble floating above this group (optional).
    [SerializeField] private CountBubbleUI countBubble;

    // All enemy characters that belong to this group.
    private readonly List<EnemyCharacter> enemies =
        new List<EnemyCharacter>();

    // Players already claimed by a kill this frame, so two enemies can't
    // both trade with the same player.
    private readonly HashSet<PlayerCharacter> claimedThisFrame =
        new HashSet<PlayerCharacter>();

    // Ensures the battle only starts once.
    private bool battleStarted;

    // Collects all child EnemyCharacter components at start.
    private void Start()
    {
        // Gather every enemy nested under this group.
        GetComponentsInChildren(true, enemies);
        // Show the group's size above it.
        if (countBubble != null) countBubble.SetCount(enemies.Count);
    }

    // Keeps the enemy disc formation updated until the battle takes over.
    private void Update()
    {
        if (!battleStarted && formation != null)
        {
            formation.UpdatePositions(enemies);
        }
    }

    // Starts the battle when the player crowd enters the radius.
    private void OnTriggerEnter(Collider other)
    {
        // Only the crowd root (tagged "Player") should start the battle.
        if (battleStarted) return;
        if (!other.CompareTag("Player")) return;

        battleStarted = true;
        StartCoroutine(BattleRoutine());
    }

    // Drives the whole fight frame by frame until one side is wiped out.
    private IEnumerator BattleRoutine()
    {
        // Freeze the crowd's forward movement (and the camera, which follows
        // it) for the whole fight so the outcome plays out before anything
        // advances. Also suspend the disc steering on both sides -- the
        // battle drives every character's position until it is over.
        PlayerController playerController = CrowdManager.Instance.GetComponent<PlayerController>();
        if (playerController != null) playerController.SetPaused(true);
        CrowdManager.Instance.SetInBattle(true);

        // Enemies charge into the fight.
        foreach (EnemyCharacter enemy in enemies)
        {
            if (enemy != null) enemy.SetRunning(true);
        }

        // Keep fighting while both sides still have members.
        while (enemies.Count > 0 && CrowdManager.Instance.GetCount() > 0)
        {
            List<PlayerCharacter> players = CrowdManager.Instance.GetCharacters();

            // Everyone runs at their nearest living opponent.
            foreach (PlayerCharacter p in players)
            {
                MoveToward(p.transform, NearestPosition(p.transform.position, enemies));
            }
            foreach (EnemyCharacter e in enemies)
            {
                MoveToward(e.transform, NearestPosition(e.transform.position, players));
            }

            // Keep allies from stacking on each other while they charge.
            SeparateAllies(players);
            SeparateAllies(enemies);

            // Resolve kills: each enemy within killRange of an unclaimed
            // player trades with it 1-for-1.
            claimedThisFrame.Clear();
            bool anyKillThisFrame = false;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                EnemyCharacter enemy = enemies[i];
                if (enemy == null) { enemies.RemoveAt(i); continue; }

                PlayerCharacter victim = null;
                float best = killRange;
                foreach (PlayerCharacter p in players)
                {
                    if (p == null || claimedThisFrame.Contains(p)) continue;
                    float d = FlatDistance(p.transform.position, enemy.transform.position);
                    if (d <= best)
                    {
                        best = d;
                        victim = p;
                    }
                }

                if (victim != null)
                {
                    claimedThisFrame.Add(victim);
                    anyKillThisFrame = true;
                    enemies.RemoveAt(i);
                    // Leave a splat under each fallen fighter (before Die()
                    // moves them out of the world).
                    if (SplatPool.Instance != null)
                    {
                        SplatPool.Instance.SpawnEnemySplat(enemy.transform.position);
                        SplatPool.Instance.SpawnPlayerSplat(victim.transform.position);
                    }
                    enemy.Die();
                    // Die() unregisters the player from CrowdManager itself.
                    victim.Die();
                }
            }

            // Keep the group's count bubble in sync as enemies fall.
            if (countBubble != null) countBubble.SetCount(enemies.Count);

            // One death sound per frame of trades, not one per pair --
            // several pairs often trade in the same frame and stacking
            // PlayOneShots would just clip.
            if (anyKillThisFrame && AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(AudioManager.SfxType.DeadPlayer);
            }

            yield return null;
        }

        // Hide the bubble once the group is wiped out.
        if (countBubble != null && enemies.Count == 0)
        {
            countBubble.gameObject.SetActive(false);
        }

        // If the enemies won, the survivors stop charging and stand idle.
        if (CrowdManager.Instance.GetCount() == 0)
        {
            foreach (EnemyCharacter enemy in enemies)
            {
                if (enemy != null) enemy.SetRunning(false);
            }
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Play(AudioManager.SfxType.EnemyCrowdWin);
            }
        }

        // Hand control back: the disc steering resumes, which pulls the
        // survivors back into a circle around the crowd root on its own.
        CrowdManager.Instance.SetInBattle(false);

        // Resume movement only if the crowd survived -- a wipe already
        // triggers Lose() via CrowdManager, which freezes input on its own.
        if (CrowdManager.Instance.GetCount() > 0 && playerController != null)
        {
            playerController.SetPaused(false);
        }
    }

    // Moves one fighter toward a target point at chargeSpeed, staying flat.
    private void MoveToward(Transform mover, Vector3 target)
    {
        target.y = mover.position.y;
        mover.position = Vector3.MoveTowards(
            mover.position, target, chargeSpeed * Time.deltaTime);
    }

    // World position of the nearest living member of the opposing side, or
    // the mover's own position when the other side is already empty.
    private Vector3 NearestPosition<T>(Vector3 from, List<T> side) where T : Component
    {
        Vector3 nearest = from;
        float best = float.MaxValue;
        foreach (T member in side)
        {
            if (member == null) continue;
            float d = FlatDistance(from, member.transform.position);
            if (d < best)
            {
                best = d;
                nearest = member.transform.position;
            }
        }
        return nearest;
    }

    // Pushes apart any two allies closer than allySeparation, so a charging
    // side stays spread out rather than collapsing into a single file.
    private void SeparateAllies<T>(List<T> side) where T : Component
    {
        for (int i = 0; i < side.Count; i++)
        {
            if (side[i] == null) continue;
            Transform a = side[i].transform;
            for (int j = i + 1; j < side.Count; j++)
            {
                if (side[j] == null) continue;
                Transform b = side[j].transform;
                Vector3 offset = a.position - b.position;
                offset.y = 0f;
                float dist = offset.magnitude;
                if (dist >= allySeparation) continue;

                Vector3 dir = dist > 0.0001f
                    ? offset / dist
                    : new Vector3(Mathf.Cos(i * 2.4f + j), 0f, Mathf.Sin(i * 2.4f + j));
                float push = (allySeparation - dist) * 0.5f;
                a.position += dir * push;
                b.position -= dir * push;
            }
        }
    }

    // Distance between two points ignoring height.
    private float FlatDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}
