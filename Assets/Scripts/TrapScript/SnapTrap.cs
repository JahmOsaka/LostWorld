using UnityEngine;

public class SnapTrap : TrapBase
{
    public int damage = 2;

    protected override void OnSnap()
    {
        anim.SetTrigger("snap");
        playerCollider?.GetComponent<IDamageable>()?.TakeDamage(damage);
    }

    protected override void OnReset()
    {
        anim.SetTrigger("reset");
    }

}