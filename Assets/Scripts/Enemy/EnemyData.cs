using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stationary Enemy")]
    public bool isStationary = false;

    [Header("Retreat")]
    public bool retreatsAfterAttack = false;
    public float retreatDistance = 2f;
    public float retreatSpeed = 3f;

    [Header("Detection Override")]
    public bool canDetectPlayer = false;

    [Header("Patrol")]
    public float patrolSpeed = 1.5f;
    public float idleTimeAtPoint = 1.5f;

    [Header("Detection")]
    public float chaseRange = 5f;
    public float attackRange = 1.5f;

    [Header("Movement")]
    [Range(1f, 20f)]
    public float moveSpeed = 2f;

    [Header("Ranged Attack")]
    public bool isRangedEnemy = false;  
    public float throwRange = 6f;        
    public float throwCooldown = 2f;     
    public float projectileSpeed = 8f;

    [Header("Ranged - Arc Projectile")]
    public bool useArcProjectile = false;
    public float arcHeight = 2f;
    public float arcSpeedMultiplier = 1f;

    [Header("Melee - Combo Attack")]
    public int damage = 1;
    public float attackCooldown = 1.5f;
    public float comboInterval = 0.1f;

    [Header("Orb Attack (Warden)")]
    public bool hasOrbAttack = false;
    public int orbCount = 2;
    public float orbitRadius = 1.2f;
    public float orbitSpeed = 180f;
    public float orbDuration = 3f;
}