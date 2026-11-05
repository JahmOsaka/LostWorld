using UnityEngine;

[CreateAssetMenu(fileName = "RangedEnemyData", menuName = "Enemy/Ranged")]
public class RangedEnemyData : EnemyData
{
    [Header("Ranged Attack")]
    public bool isStationary = false;
    public float throwRange = 6f;
    public float throwCooldown = 2f;
    public float projectileSpeed = 8f;
    public GameObject projectilePrefab;

    [Header("Arc Projectile")]
    public bool useArcProjectile = false;
    public float arcLaunchAngle = 60f;
}