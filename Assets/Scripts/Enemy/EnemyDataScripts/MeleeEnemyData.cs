using UnityEngine;

[CreateAssetMenu(fileName = "MeleeEnemyData", menuName = "Enemy/Melee")]
public class MeleeEnemyData : EnemyData
{
    [Header("Melee Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.5f;
    public float comboInterval = 0.1f;
    public float attackCancelRange = 2.5f;
}