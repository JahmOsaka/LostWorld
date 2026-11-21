using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PeaProjectile : MonoBehaviour
{
    [Header("Procedural Flight Visual")]
    [Tooltip("Optional child transform to stretch along velocity (defaults to this transform).")]
    [SerializeField] private Transform visualTransform;
    [Tooltip("Optional particle system emitting droplets during flight (detached on impact so particles finish naturally).")]
    [SerializeField] private ParticleSystem flightParticles;
    [Tooltip("How much the projectile stretches horizontally based on speed.")]
    [SerializeField] private float velocityStretchFactor = 0.035f;
    [Tooltip("Maximum horizontal stretch multiplier.")]
    [SerializeField] private float maxStretchMultiplier = 1.45f;

    private Rigidbody2D rb;
    private PeaShootingSkillData data;
    private Vector3 baseScale;
    private int direction = 1;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (visualTransform == null)
        {
            visualTransform = transform;
        }
        baseScale = visualTransform.localScale;
    }

    public void Initialize(PeaShootingSkillData skillData, int facingDirection)
    {
        data = skillData;
        direction = facingDirection;

        rb.gravityScale = data.gravityScale;
        rb.linearVelocity = new Vector2(data.launchVelocity.x * direction, data.launchVelocity.y);

        UpdateFlightVisual();
        Destroy(gameObject, data.lifetime);
    }

    private void Update()
    {
        if (hasHit) return;
        UpdateFlightVisual();
    }

    private void UpdateFlightVisual()
    {
        if (rb == null || visualTransform == null) return;

        Vector2 vel = rb.linearVelocity;
        float speed = vel.magnitude;

        if (speed > 0.1f)
        {
            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        float stretchX = Mathf.Clamp(1f + speed * velocityStretchFactor, 1f, maxStretchMultiplier);
        float squashY = 1f / Mathf.Sqrt(stretchX);

        visualTransform.localScale = new Vector3(
            baseScale.x * stretchX,
            baseScale.y * squashY,
            baseScale.z
        );
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

                HitFlash hitFlash = other.GetComponentInParent<HitFlash>();
                if (hitFlash != null)
                {
                    hitFlash.TriggerFlash(data.enemyFlashDuration);
                }

                HitKnockback knockback = other.GetComponentInParent<HitKnockback>();
                if (knockback != null)
                {
                    knockback.ApplyKnockback(direction, data.knockbackForce);
                }

                if (GameFeelManager.Instance != null && Camera.main != null && data.hitShakeDuration > 0f)
                {
                    GameFeelManager.Instance.TriggerShake(data.hitShakeDuration, data.hitShakeMagnitude, Camera.main.transform);
                }

                FinishAndDestroy();
            }
            return;
        }

        if ((data.groundLayer.value & layerBit) != 0)
        {
            hasHit = true;
            FinishAndDestroy();
        }
    }

    private void FinishAndDestroy()
    {
        if (flightParticles != null)
        {
            flightParticles.transform.SetParent(null);
            flightParticles.Stop();
            Destroy(flightParticles.gameObject, 1f);
        }

        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }

        Destroy(gameObject);
    }
}