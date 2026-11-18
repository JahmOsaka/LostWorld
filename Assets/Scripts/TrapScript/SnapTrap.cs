using UnityEngine;

public class SnapTrap : TrapBase
{
    public int damage = 2;

    protected override void OnSnap()
    {
        anim.SetTrigger("snap");

        if (playerCollider != null)
        {
            IDamageable damageable = playerCollider.GetComponentInParent<IDamageable>();
            if (damageable != null && damageable.CanBeDamagedBy(playerCollider))
            {
                damageable.TakeDamage(damage);
            }
        }
    }

    protected override void OnReset()
    {
        anim.SetTrigger("reset");
    }
}