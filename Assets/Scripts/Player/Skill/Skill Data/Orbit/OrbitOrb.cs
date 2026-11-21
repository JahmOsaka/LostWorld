using System.Collections.Generic;
using UnityEngine;

public class OrbitOrb : MonoBehaviour
{
    private Transform owner;
    private OrbitSkillData data;
    private readonly Dictionary<IDamageable, float> nextHitTimes = new Dictionary<IDamageable, float>();

    public void Initialize(Transform skillOwner, OrbitSkillData skillData)
    {
        owner = skillOwner;
        data = skillData;
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

        HitKnockback knockback = other.GetComponentInParent<HitKnockback>();
        if (knockback != null && owner != null)
        {
            int pushDir = (other.transform.position.x >= owner.position.x) ? 1 : -1;
            knockback.ApplyKnockback(pushDir, data.knockbackForce);
        }

        if (data.vfxPrefab != null)
        {
            GameObject fx = Instantiate(data.vfxPrefab, other.bounds.center, Quaternion.identity);
            Destroy(fx, 0.6f);
        }
    }
}