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

    public void Init(Vector2 startPos, Vector2 targetPos, float arcHeight, int dmg, float speedMultiplier = 1f)
    {
        damage = dmg;

        float baseGravity = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
        float deltaY = targetPos.y - startPos.y;

        float timeUp = Mathf.Sqrt(2f * arcHeight / baseGravity);
        float heightFromPeakToTarget = Mathf.Max(arcHeight - deltaY, 0.01f);
        float timeDown = Mathf.Sqrt(2f * heightFromPeakToTarget / baseGravity);

        float totalTime = timeUp + timeDown;
        float vy = baseGravity * timeUp;
        float vx = (targetPos.x - startPos.x) / totalTime;

        rb.gravityScale *= speedMultiplier * speedMultiplier;
        rb.linearVelocity = new Vector2(vx, vy) * speedMultiplier;

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}