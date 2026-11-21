using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PeaProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private PeaShootingSkillData data;
    private int direction = 1;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(PeaShootingSkillData skillData, int facingDirection)
    {
        data = skillData;
        direction = facingDirection;

        rb.gravityScale = data.gravityScale;
        rb.linearVelocity = new Vector2(data.launchVelocity.x * direction, data.launchVelocity.y);

        Destroy(gameObject, data.lifetime);
    }

    private void Update()
    {
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || data == null) return;

        int layerBit = 1 << other.gameObject.layer;

        if ((data.enemyLayer.value & layerBit) != 0)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.CanBeDamagedBy(other))
            {
                hasHit = true;
                damageable.TakeDamage(data.damage);

                HitKnockback knockback = other.GetComponentInParent<HitKnockback>();
                if (knockback != null)
                {
                    knockback.ApplyKnockback(direction, data.knockbackForce);
                }

                SpawnImpactVFX();
                Destroy(gameObject);
            }
            return;
        }

        if ((data.groundLayer.value & layerBit) != 0)
        {
            hasHit = true;
            SpawnImpactVFX();
            Destroy(gameObject);
        }
    }

    private void SpawnImpactVFX()
    {
        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }
    }
}