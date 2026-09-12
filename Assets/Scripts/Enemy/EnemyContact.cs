using UnityEngine;

public class EnemyContact : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CoreHealth playerHealth = collision.GetComponentInParent<CoreHealth>();

        if(playerHealth != null)
        {
            playerHealth.TakeDamage(enemy.enemyData.damage);
        }
    }
}

