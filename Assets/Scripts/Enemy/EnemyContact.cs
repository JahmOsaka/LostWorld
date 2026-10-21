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

        CoreHealth playerHealth = collision.GetComponentInParent<CoreHealth>();

        if (playerHealth != null)
        {
            if (!playerHealth.IsValidHurtbox(collision)) return;

            playerHealth.TakeDamage(enemy.enemyData.damage);
            nextDamageTime = Time.time + damageInterval;
        }
    }
}