using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class EnemyArcProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 startPos, Vector2 targetPos, float launchAngle, int dmg, float speed)
    {
        damage = dmg;
        float dx = targetPos.x - startPos.x;
        float deltaY = targetPos.y - startPos.y;

        float T = Mathf.Max(Mathf.Abs(dx), 0.5f) / Mathf.Max(speed, 0.01f);   
        float vx = dx / T;
        float vy = Mathf.Abs(vx) * Mathf.Tan(launchAngle * Mathf.Deg2Rad);   

      
        float g = 2f * (vy * T - deltaY) / (T * T);
        g = Mathf.Max(g, 1f);

        rb.gravityScale = g / Mathf.Abs(Physics2D.gravity.y);
        rb.linearVelocity = new Vector2(vx, vy);

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