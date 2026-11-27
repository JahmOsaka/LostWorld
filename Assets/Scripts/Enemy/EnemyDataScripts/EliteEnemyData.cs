using UnityEngine;

[CreateAssetMenu(fileName = "EliteEnemyData", menuName = "Enemy/Elite")]
public class EliteEnemyData : MeleeEnemyData
{
    [Header("Orbit Orb Attack")]
    public GameObject orbPrefab;
    public int orbDamage = 1;
    [Range(0f, 1f)] public float orbHpPercent = 0.5f;
    public int orbCount = 2;
    public Vector2 orbitRadius = new Vector2(1.75f, 1.15f);
    public float orbitSpeed = 180f;
    public float orbDuration = 3f;

    [Header("Teleport")]
    public int teleportOrbDamage = 1;
    public float teleportMinRange = 3.5f;
    public float teleportMaxRange = 8f; 
    public float teleportCooldown = 4f;
    public float frontOffset = 1.6f;    
    public float vanishDuration = 0.2f;   
    public float orbTravelSpeed = 18f;     
    public float reappearDelay = 0.2f;      
    public GameObject teleportOrbPrefab;   
    public GameObject teleportVfxPrefab;
    public GameObject teleportVanishVfxPrefab;
    public GameObject teleportReappearVfxPrefab;
}