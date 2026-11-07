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
        float dy = targetPos.y - startPos.y;
        float distanceX = Mathf.Abs(dx);

        float fixedGravityScale = 2f;
        rb.gravityScale = fixedGravityScale;
        float g = Mathf.Abs(Physics2D.gravity.y) * fixedGravityScale;

        float angleRad = launchAngle * Mathf.Deg2Rad;
        float tanAngle = Mathf.Tan(angleRad);

        float denominator = (distanceX * tanAngle) - dy;

        float vx = 0f;
        float vy = 0f;

        if (denominator > 0)
        {
            float vxSq = (0.5f * g * distanceX * distanceX) / denominator;
            vx = Mathf.Sqrt(vxSq);
            vy = vx * tanAngle;
        }
        else
        {
            vx = speed;
            vy = 0;
        }

        vx *= Mathf.Sign(dx);

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