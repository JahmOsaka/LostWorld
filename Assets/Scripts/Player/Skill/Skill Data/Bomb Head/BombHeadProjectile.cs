using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BombHeadProjectile : MonoBehaviour, IDamageable
{
    [Header("Procedural Fuse Visual")]
    [Tooltip("SpriteRenderer used to flash warning color before detonation.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Warning tint color during fuse pulse.")]
    [SerializeField] private Color warningColor = new Color(1f, 0.35f, 0.15f, 1f);
    [Tooltip("Maximum scale punch multiplier during the fuse pulse.")]
    [SerializeField] private float pulseScaleAmount = 0.28f;

    [Header("Telegraph Ring Settings")]
    [Tooltip("Optional material for the telegraph rings (falls back to Sprites/Default if empty).")]
    [SerializeField] private Material ringMaterial;
    [Tooltip("Base color of the outer explosion radius telegraph ring.")]
    [SerializeField] private Color outerRingColor = new Color(1f, 0.35f, 0.15f, 0.35f);
    [Tooltip("Color of the expanding inner timer ring.")]
    [SerializeField] private Color innerRingColor = new Color(1f, 0.75f, 0.25f, 0.75f);
    [Tooltip("Line width of the telegraph rings.")]
    [SerializeField] private float ringWidth = 0.05f;
    [Tooltip("Number of segments used to draw smooth circles.")]
    [SerializeField] private int ringSegments = 40;
    [Tooltip("Sorting order for the telegraph rings.")]
    [SerializeField] private int ringSortingOrder = 10;

    private Rigidbody2D rb;
    private BombHeadSkillData data;
    private Color originalColor = Color.white;
    private Vector3 baseScale;

    private LineRenderer outerRing;
    private LineRenderer innerRing;

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

        SetupTelegraphRings();
        UpdateTelegraphRings(0f);
    }

    private void Update()
    {
        if (hasExploded || data == null) return;

        elapsed += Time.deltaTime;
        float progress = Mathf.Clamp01(elapsed / data.fuseDuration);

        // Accelerate pulse speed as detonation approaches
        float frequency = Mathf.Lerp(6f, 30f, progress * progress);
        float wave = (Mathf.Sin(elapsed * frequency) + 1f) * 0.5f;

        float scaleMult = 1f + wave * pulseScaleAmount * (0.5f + progress);
        transform.localScale = baseScale * scaleMult;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(originalColor, warningColor, wave);
        }

        UpdateTelegraphRings(progress);

        if (elapsed >= data.fuseDuration)
        {
            Explode();
        }
    }

    public bool CanBeDamagedBy(Collider2D hitCollider) => !hasExploded;

    public void TakeDamage(int amount) => Explode();

    public void Die() => Explode();

    public void Explode()
    {
        if (hasExploded || data == null) return;
        hasExploded = true;

        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, transform.position, Quaternion.identity);
            BombExplosionVFX explosionVfx = fx.GetComponent<BombExplosionVFX>();
            if (explosionVfx != null)
            {
                explosionVfx.Initialize(data.explosionRadius);
            }
            Destroy(fx, 1.5f);
        }

        if (GameFeelManager.Instance != null)
        {
            if (data.hitstopDuration > 0f)
            {
                GameFeelManager.Instance.TriggerHitstop(data.hitstopDuration);
            }

            if (Camera.main != null)
            {
                GameFeelManager.Instance.TriggerShake(data.shakeDuration, data.shakeMagnitude, Camera.main.transform);
            }
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

                HitFlash hitFlash = col.GetComponentInParent<HitFlash>();
                if (hitFlash != null)
                {
                    hitFlash.TriggerFlash(data.enemyFlashDuration);
                }

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

    private void SetupTelegraphRings()
    {
        Material mat = ringMaterial != null ? ringMaterial : new Material(Shader.Find("Sprites/Default"));
        outerRing = CreateRing("OuterTelegraphRing", mat, ringWidth);
        innerRing = CreateRing("InnerTelegraphRing", mat, ringWidth * 0.85f);
    }

    private LineRenderer CreateRing(string ringName, Material mat, float width)
    {
        GameObject ringObj = new GameObject(ringName);
        ringObj.transform.SetParent(transform, false);

        LineRenderer lr = ringObj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = ringSegments;
        lr.startWidth = width;
        lr.endWidth = width;
        lr.material = mat;
        lr.sortingOrder = ringSortingOrder;
        return lr;
    }

    private void UpdateTelegraphRings(float progress)
    {
        if (outerRing == null || innerRing == null || data == null) return;

        Color outerCol = outerRingColor;
        outerCol.a = Mathf.Lerp(outerRingColor.a, 0.9f, progress * progress);
        outerRing.startColor = outerCol;
        outerRing.endColor = outerCol;
        DrawCircle(outerRing, transform.position, data.explosionRadius);

        float innerRadius = Mathf.Lerp(0.15f, data.explosionRadius, progress);
        Color innerCol = Color.Lerp(innerRingColor, warningColor, progress);
        innerRing.startColor = innerCol;
        innerRing.endColor = innerCol;
        DrawCircle(innerRing, transform.position, innerRadius);
    }

    private void DrawCircle(LineRenderer lr, Vector3 center, float radius)
    {
        float step = (Mathf.PI * 2f) / ringSegments;
        for (int i = 0; i < ringSegments; i++)
        {
            float angle = i * step;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            lr.SetPosition(i, pos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        float radius = (data != null) ? data.explosionRadius : 2.8f;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}