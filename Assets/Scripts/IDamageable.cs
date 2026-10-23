using UnityEngine;

public interface IDamageable
{
    bool CanBeDamagedBy(Collider2D hitCollider);
    void TakeDamage(int amount);
    void Die();
}