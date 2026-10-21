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
        CoreHealth health = other.GetComponentInParent<CoreHealth>();
        if (health != null)
        {
            if (health.IsValidHurtbox(other))
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
                return;
            }
        }

        if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}