using UnityEngine;

[CreateAssetMenu(fileName = "EliteEnemyData", menuName = "Enemy/Elite")]
public class EliteEnemyData : MeleeEnemyData
{
    [Header("Orb Attack")]
    public GameObject orbPrefab;
    [Range(0f, 1f)] public float orbHpPercent = 0.5f;
    public int orbCount = 2;
    public Vector2 orbitRadius = new Vector2(1.75f, 1.15f);
    public float orbitSpeed = 180f;
    public float orbDuration = 3f;
}