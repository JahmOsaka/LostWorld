using UnityEngine;
using System.Collections.Generic;

public class EliteEnemy : Enemy
{
    [Header("Orb Attack")]
    public Transform orbitCenter;
    private List<GameObject> activeOrbs = new List<GameObject>();

    public EliteTeleportState TeleportState { get; private set; }
    [HideInInspector] public float nextTeleportTime;

    protected override void Awake()
    {
        base.Awake();
        TeleportState = new EliteTeleportState(this, stateMachine, enemyData, "teleport");
    }

    public bool CanTeleportToPlayer()
    {
        var elite = enemyData as EliteEnemyData;
        if (elite == null || Time.time < nextTeleportTime) return false;

        float dist = DistanceToPlayer();
        return dist >= elite.teleportMinRange && dist <= elite.teleportMaxRange;
    }

    public void NotifyComboCheck()
    {
        if (stateMachine.CurrentState != attackState) return;
        attackState.OnComboCheck();
    }

    public void NotifySpawnOrbVFX()
    {
        var elite = enemyData as EliteEnemyData;
        if (elite == null) return;

        for (int i = 0; i < elite.orbCount; i++)
        {
            float startAngle = (360f / elite.orbCount) * i;
            float rad = startAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad) * elite.orbitRadius.x, Mathf.Sin(rad) * elite.orbitRadius.y);
            Vector3 edgeSpawnPos = orbitCenter.position + (Vector3)offset;

            if (vfxList != null && vfxList.Length > 2 && vfxList[2].prefab != null)
            {
                GameObject vfx = Instantiate(vfxList[2].prefab, edgeSpawnPos, Quaternion.identity);
                SpriteRenderer vfxSprite = vfx.GetComponentInChildren<SpriteRenderer>();
                if (vfxSprite != null) vfxSprite.flipX = this.spriteRenderer.flipX;
            }
        }
    }

    public void NotifySpawnOrbs()
    {
        var elite = enemyData as EliteEnemyData;
        if (elite == null || elite.orbPrefab == null) return;

        ClearActiveOrbs();

        for (int i = 0; i < elite.orbCount; i++)
        {
            float startAngle = (360f / elite.orbCount) * i;
            float rad = startAngle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(rad) * elite.orbitRadius.x, Mathf.Sin(rad) * elite.orbitRadius.y);
            Vector3 edgeSpawnPos = orbitCenter.position + (Vector3)offset;

            GameObject orbObj = Instantiate(elite.orbPrefab, edgeSpawnPos, Quaternion.identity);
            orbObj.GetComponent<OrbitingDamageOrb>()
                .Init(orbitCenter, elite.orbitRadius, elite.orbitSpeed, startAngle, elite.orbDuration, elite.orbDamage);
            activeOrbs.Add(orbObj);
        }
    }
    public void SetActiveOrbsCenter(Transform newCenter)
    {
        activeOrbs.RemoveAll(orb => orb == null);
        foreach (var orb in activeOrbs)
        {
            var damageOrb = orb.GetComponent<OrbitingDamageOrb>();
            if (damageOrb != null) damageOrb.SetCenter(newCenter);
        }
    }

    public void ClearActiveOrbs()
    {
        foreach (var orb in activeOrbs)
        {
            if (orb != null) Destroy(orb);
        }
        activeOrbs.Clear();
    }

    public void SetActiveOrbsVisible(bool active)
    {
        activeOrbs.RemoveAll(orb => orb == null);
        foreach (var orb in activeOrbs)
        {
            orb.SetActive(active);
        }
    }

    public bool HasActiveOrbs()
    {
        activeOrbs.RemoveAll(orb => orb == null);
        return activeOrbs.Count > 0;
    }

    public override void ResetEnemyState()
    {
        base.ResetEnemyState();
        ClearActiveOrbs();
        nextTeleportTime = 0f;
    }

    protected override void DrawRangeGizmos()
    {
        base.DrawRangeGizmos();

        var melee = enemyData as MeleeEnemyData;
        if (melee == null) return;

        BoxCollider2D col = physicsCollider != null ? physicsCollider : GetComponent<BoxCollider2D>();
        Vector3 center = col != null ? (Vector3)col.bounds.center : transform.position;

        DrawRange(center, melee.attackRange, new Color(1f, 0.2f, 0.2f), "Attack");
        DrawRange(center, melee.attackRange * melee.attackExitRange, new Color(1f, 0.2f, 0.2f, 0.35f), "Attack exit");

        var elite = enemyData as EliteEnemyData;
        if (elite != null)
        {
            DrawRange(center, elite.teleportMinRange, new Color(0.6f, 0.2f, 1f, 0.6f), "TP Min");
            DrawRange(center, elite.teleportMaxRange, new Color(0.6f, 0.2f, 1f, 1f), "TP Max");
        }
    }

}