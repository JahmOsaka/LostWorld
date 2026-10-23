using UnityEngine;

[CreateAssetMenu(fileName = "EliteEnemyData", menuName = "Enemy/Elite")]
public class EliteEnemyData : MeleeEnemyData
{
    [Header("Orb Attack")]
    public GameObject orbPrefab;
    public int orbCount = 2;
    public float orbitRadius = 1.2f;
    public float orbitSpeed = 180f;
    public float orbDuration = 3f;
}