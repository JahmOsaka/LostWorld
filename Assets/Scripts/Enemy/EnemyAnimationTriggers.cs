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
        enemy.SpawnProjectile();
    }
    
    private void NotifyAttackHit()
    {
        enemy.NotifyAttackHit();
    }

    private void NotifyAttackAnimationEnd()
    {
        enemy.NotifyAttackAnimationEnd();
    }

    private void NotifyThrowAnimationEnd()
    {
        enemy.NotifyThrowAnimationEnd();
    }
}