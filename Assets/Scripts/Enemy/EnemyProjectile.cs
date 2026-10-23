using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 dir, float speed, int dmg)
    {
        damage = dmg;
        rb.linearVelocity = dir.normalized * speed;   
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.CanBeDamagedBy(other))
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}