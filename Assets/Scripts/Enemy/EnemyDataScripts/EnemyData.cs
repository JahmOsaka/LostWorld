using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Enemy/Base")]
public class EnemyData : ScriptableObject
{
    [Header("Stats")]
    public int maxHealth = 30;
    public int damage = 1;

    [Header("Movement")]
    [Range(1f, 20f)]
    public float moveSpeed = 2f;
    public float patrolSpeed = 1.5f;
    public float idleTimeAtPoint = 1.5f;

    [Header("Detection")]
    public bool canDetectPlayer = false;
    public float chaseRange = 5f;

    [Header("Retreat")]
    public bool retreatsAfterAttack = false;
    public float retreatDistance = 2f;
    public float retreatSpeed = 3f;
    public float retreatTriggerRange = 2f;
    public float retreatPauseTime = 0.4f;
    [Range(0f, 1f)] public float retreatChance = 1f;

    public bool ShouldRetreat(float distanceToPlayer)
    {
        return retreatsAfterAttack
            && distanceToPlayer <= retreatTriggerRange
            && (retreatChance >= 1f || Random.value < retreatChance);
    }
}