using System.Collections.Generic;
using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    [Header("Hit Punch Feel")]
    [Tooltip("Scale multiplier applied briefly to the orb when it strikes an enemy.")]
    [SerializeField] private float hitPunchScale = 1.45f;
    [Tooltip("Duration in seconds for the orb scale to return to normal after a hit.")]
    [SerializeField] private float hitPunchDuration = 0.12f;

    private Transform owner;
    private OrbitSkillData data;
    private readonly Dictionary<IDamageable, float> nextHitTimes = new Dictionary<IDamageable, float>();

    private Vector3 baseScale;
    private float lifecycleScale = 1f;
    private float punchTimer;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    public void Initialize(Transform skillOwner, OrbitSkillData skillData)
    {
        owner = skillOwner;
        data = skillData;
    }

    public void SetLifecycleScale(float scale)
    {
        lifecycleScale = scale;
        ApplyCombinedScale();
    }

    private void Update()
    {
        if (punchTimer > 0f)
        {
            punchTimer = Mathf.Max(0f, punchTimer - Time.deltaTime);
            ApplyCombinedScale();
        }
    }

    private void ApplyCombinedScale()
    {
        float punchMult = 1f;
        if (punchTimer > 0f && hitPunchDuration > 0f)
        {
            float t = punchTimer / hitPunchDuration;
            punchMult = Mathf.Lerp(1f, hitPunchScale, t);
        }

        transform.localScale = baseScale * (lifecycleScale * punchMult);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (data == null) return;

        int layerBit = 1 << other.gameObject.layer;
        if ((data.enemyLayer.value & layerBit) == 0) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null || !damageable.CanBeDamagedBy(other)) return;

        if (nextHitTimes.TryGetValue(damageable, out float nextAllowedTime) && Time.time < nextAllowedTime)
            return;

        nextHitTimes[damageable] = Time.time + data.hitCooldownPerEnemy;
        damageable.TakeDamage(data.damage);
        punchTimer = hitPunchDuration;

        HitFlash hitFlash = other.GetComponentInParent<HitFlash>();
        if (hitFlash != null)
        {
            hitFlash.TriggerFlash(data.enemyFlashDuration);
        }

        HitKnockback knockback = other.GetComponentInParent<HitKnockback>();
        if (knockback != null && owner != null)
        {
            int pushDir = (other.transform.position.x >= owner.position.x) ? 1 : -1;
            knockback.ApplyKnockback(pushDir, data.knockbackForce);
        }

        if (GameFeelManager.Instance != null && Camera.main != null && data.hitShakeDuration > 0f)
        {
            GameFeelManager.Instance.TriggerShake(data.hitShakeDuration, data.hitShakeMagnitude, Camera.main.transform);
        }

        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, other.bounds.center, Quaternion.identity);
            Destroy(fx, 0.6f);
        }
    }
}