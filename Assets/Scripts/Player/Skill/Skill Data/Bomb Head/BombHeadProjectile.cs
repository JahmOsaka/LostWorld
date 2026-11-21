using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombHeadProjectile : MonoBehaviour, IDamageable
{
    [Header("Procedural Fuse Visual")]
    [Tooltip("SpriteRenderer used to flash warning color before detonation.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Warning tint color during fuse pulse.")]
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.2f, 1f);
    [Tooltip("Maximum scale punch multiplier during the fuse pulse.")]
    [SerializeField] private float pulseScaleAmount = 0.22f;

    private Rigidbody2D rb;
    private BombHeadSkillData data;
    private Color originalColor = Color.white;
    private Vector3 baseScale;

    private float elapsed;
    private bool hasExploded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void Initialize(BombHeadSkillData skillData, int facingDirection)
    {
        data = skillData;
        rb.linearVelocity = new Vector2(data.throwVelocity.x * facingDirection, data.throwVelocity.y);
    }

    private void Update()
    {
        if (hasExploded || data == null) return;

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / data.fuseDuration);

        float frequency = Mathf.Lerp(6f, 28f, progress * progress);
        float wave = (Mathf.Sin(elapsed * frequency) + 1f) * 0.5f;

        float scaleMult = 1f + wave * pulseScaleAmount * (0.5f + progress);
        transform.localScale = baseScale * scaleMult;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(originalColor, warningColor, wave);
        }

        if (elapsed >= data.fuseDuration)
        {
            Explode();
        }
    }

    public bool CanBeDamagedBy(Collider2D hitCollider) => !hasExploded;

    public void TakeDamage(int amount)
    {
        Explode();
    }

    public void Die()
    {
        Explode();
    }

    public void Explode()
    {
        if (hasExploded || data == null) return;
        hasExploded = true;

        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        if (GameFeelManager.Instance != null && Camera.main != null)
        {
            GameFeelManager.Instance.TriggerShake(data.shakeDuration, data.shakeMagnitude, Camera.main.transform);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, data.explosionRadius, data.enemyLayer);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        foreach (Collider2D col in hits)
        {
            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null || ReferenceEquals(damageable, this) || damagedTargets.Contains(damageable))
                continue;

            if (damageable.CanBeDamagedBy(col))
            {
                damagedTargets.Add(damageable);
                damageable.TakeDamage(data.explosionDamage);

                HitKnockback knockback = col.GetComponentInParent<HitKnockback>();
                if (knockback != null)
                {
                    int pushDir = (col.transform.position.x >= transform.position.x) ? 1 : -1;
                    knockback.ApplyKnockback(pushDir, data.knockbackForce);
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        float radius = (data != null) ? data.explosionRadius : 2.8f;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}