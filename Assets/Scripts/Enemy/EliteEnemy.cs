using UnityEngine;
using System.Collections.Generic;

public class EliteEnemy : Enemy
{
    [Header("Orb Attack")]
    public Transform orbitCenter;
    private List<GameObject> activeOrbs = new List<GameObject>();

    public void NotifyComboCheck()
    {
        if (stateMachine.CurrentState != attackState) return;
        attackState.OnComboCheck();
    }

    public void NotifySpawnOrbs()
    {
        var elite = enemyData as EliteEnemyData;
        if (elite == null || elite.orbPrefab == null) return;

        ClearActiveOrbs();

        if (elite.orbCastVfxPrefab != null)
        {
            GameObject vfx = Instantiate(elite.orbCastVfxPrefab, transform.position, Quaternion.identity, transform);
            if (elite.orbCastVfxLifetime > 0f) Destroy(vfx, elite.orbCastVfxLifetime);
        }

        for (int i = 0; i < elite.orbCount; i++)
        {
            GameObject orbObj = Instantiate(elite.orbPrefab, orbitCenter.position, Quaternion.identity);
            float startAngle = (360f / elite.orbCount) * i;
            orbObj.GetComponent<OrbitingDamageOrb>()
                .Init(orbitCenter, elite.orbitRadius, elite.orbitSpeed, startAngle, elite.orbDuration, elite.damage);
            activeOrbs.Add(orbObj);
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

    public bool HasActiveOrbs()
    {
        activeOrbs.RemoveAll(orb => orb == null);
        return activeOrbs.Count > 0;
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
    }
}