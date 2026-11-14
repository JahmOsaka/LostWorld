using UnityEngine;

public class EnemyAnimationTriggers : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void SpawnProjectile()
    {
        if (enemy is RangedEnemy ranged) ranged.SpawnProjectile();
    }

    private void NotifyAttackAnimationEnd()
    {
        enemy.NotifyAttackAnimationEnd();
    }

    private void NotifyComboCheck()
    {
        if (enemy is EliteEnemy elite) elite.NotifyComboCheck();
    }

    private void NotifyThrowAnimationEnd()
    {
        if (enemy is RangedEnemy ranged) ranged.NotifyThrowAnimationEnd();
    }

    private void EnableAttackHitbox(int index)
    {
        enemy.EnableAttackHitbox(index);
    }

    private void DisableAttackHitbox(int index)
    {
        enemy.DisableAttackHitbox(index);
    }
}