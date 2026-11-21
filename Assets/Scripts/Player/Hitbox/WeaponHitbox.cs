using System.Collections.Generic;
using UnityEngine;

public class WeaponHitbox : MonoBehaviour
{
    [Header("Hit VFX")]
    [SerializeField] private GameObject hitSparkPrefab;
    [SerializeField] private float sparkLifetime = 0.35f;

    [Header("Micro Knockback")]
    [SerializeField] private float lightKnockbackForce = 3.5f;
    [SerializeField] private float heavyKnockbackForce = 8f;

    [Header("Micro Screen Shake (Hit 1-2 Multiplier)")]
    [SerializeField] private float lightShakeDurationMult = 0.45f;
    [SerializeField] private float lightShakeMagnitudeMult = 0.35f;

    private PlayerData playerData;
    private Collider2D myCollider;
    private ContactFilter2D filter;
    private readonly List<IDamageable> alreadyHitTargets = new List<IDamageable>();
    private readonly List<Collider2D> hitResults = new List<Collider2D>();

    private Player player;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        filter = ContactFilter2D.noFilter;

        player = GetComponentInParent<Player>();
        if (player != null)
        {
            playerData = player.playerData;
        }

        if (myCollider != null) myCollider.enabled = false;
    }

    private void OnEnable()
    {
        alreadyHitTargets.Clear();
        if (myCollider != null) myCollider.enabled = true;
    }

    private void OnDisable()
    {
        if (myCollider != null) myCollider.enabled = false;
    }

    private void FixedUpdate()
    {
        if (myCollider == null || !myCollider.enabled || playerData == null) return;

        myCollider.Overlap(filter, hitResults);
        bool hitAnyThisFrame = false;

        int currentCombo = (player != null) ? player.CombatData.ComboCounter : 1;
        float hitstopTime = GetHitstopTime(currentCombo);

        foreach (Collider2D col in hitResults)
        {
            if (col == null) continue;

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable == null || alreadyHitTargets.Contains(damageable)) continue;

            if (damageable.CanBeDamagedBy(col))
            {
                damageable.TakeDamage(playerData.attackDamage);
                alreadyHitTargets.Add(damageable);
                hitAnyThisFrame = true;

                ApplyTargetHitFeel(col, currentCombo, hitstopTime);
                SpawnHitSpark(col, currentCombo);
            }
        }

        if (hitAnyThisFrame)
        {
            TriggerHitstopAndShake(currentCombo, hitstopTime);
        }
    }

    private float GetHitstopTime(int currentCombo)
    {
        if (currentCombo <= 1) return playerData.hitstopHit1;
        if (currentCombo == 2) return playerData.hitstopHit2;
        return playerData.hitstopHit3;
    }

    private void ApplyTargetHitFeel(Collider2D targetCollider, int currentCombo, float hitstopTime)
    {
        bool isFinisher = currentCombo >= 3;

        HitFlash hitFlash = targetCollider.GetComponentInParent<HitFlash>();
        if (hitFlash != null)
        {
            float flashTime = hitstopTime + (isFinisher ? 0.04f : 0.02f);
            hitFlash.TriggerFlash(flashTime);
        }

        HitKnockback knockback = targetCollider.GetComponentInParent<HitKnockback>();
        if (knockback != null)
        {
            int dir = (player != null) ? player.FacingDirection : 1;
            float force = isFinisher ? heavyKnockbackForce : lightKnockbackForce;
            knockback.ApplyKnockback(dir, force);
        }
    }

    private void TriggerHitstopAndShake(int currentCombo, float hitstopTime)
    {
        if (GameFeelManager.Instance == null || playerData == null) return;

        if (hitstopTime > 0f)
        {
            GameFeelManager.Instance.TriggerHitstop(hitstopTime);
        }

        if (Camera.main != null)
        {
            bool isFinisher = currentCombo >= 3;
            float shakeDuration = isFinisher
                ? playerData.screenShakeDuration
                : playerData.screenShakeDuration * lightShakeDurationMult;
            float shakeMagnitude = isFinisher
                ? playerData.screenShakeMagnitude
                : playerData.screenShakeMagnitude * lightShakeMagnitudeMult;

            GameFeelManager.Instance.TriggerShake(shakeDuration, shakeMagnitude, Camera.main.transform);
        }
    }

    private void SpawnHitSpark(Collider2D targetCollider, int currentCombo)
    {
        if (hitSparkPrefab == null) return;

        Vector2 hitPoint = targetCollider.bounds.center;
        float yRot = (player != null && player.FacingDirection < 0) ? 180f : 0f;
        float randomZ = Random.Range(-15f, 15f);

        GameObject spark = Instantiate(hitSparkPrefab, hitPoint, Quaternion.Euler(0f, yRot, randomZ));

        if (currentCombo >= 3)
        {
            spark.transform.localScale *= 1.45f;
        }

        Destroy(spark, sparkLifetime);
    }
}