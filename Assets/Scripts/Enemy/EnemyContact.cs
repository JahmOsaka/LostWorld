using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    [SerializeField] private float damageInterval = 1f;

    private Enemy enemy;
    private float nextDamageTime;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (Time.time < nextDamageTime) return;

        IDamageable damageable = collision.GetComponentInParent<IDamageable>();

        if (damageable != null && damageable.CanBeDamagedBy(collision))
        {
            damageable.TakeDamage(enemy.enemyData.damage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}