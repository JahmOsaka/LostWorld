using UnityEngine;

public class TeleportOrbDamage : MonoBehaviour
{
    private int damage;
    private bool hasHit;

    public void Init(int dmg)
    {
        damage = dmg;
        hasHit = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit || damage <= 0 || !other.CompareTag("Player")) return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null && damageable.CanBeDamagedBy(other))
        {
            damageable.TakeDamage(damage);
            hasHit = true;
        }
    }
}