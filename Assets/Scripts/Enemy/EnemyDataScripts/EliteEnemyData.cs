using UnityEngine;

[CreateAssetMenu(fileName = "EliteEnemyData", menuName = "Enemy/Elite")]
public class EliteEnemyData : MeleeEnemyData
{
    [Header("Orb Attack")]
    public GameObject orbPrefab;
    public GameObject orbCastVfxPrefab; 
    public float orbCastVfxLifetime;
    [Range(0f, 1f)] public float orbHpPercent = 0.5f;
    public int orbCount = 2;
    public float orbitRadius = 1.2f;
    public float orbitSpeed = 180f;
    public float orbDuration = 3f;
}