using UnityEngine;

[CreateAssetMenu(fileName = "NewPerishSkill", menuName = "Data/Skills/Perish Skill (Blue)")]
public class PerishSkillData : SkillData
{
    public float destroyRadius = 2f;
    public LayerMask obstacleLayer;

    public override void ExecuteSkill(GameObject user)
    {
        if (user == null) return;
        ExecuteSkill(user.transform);
    }

    public void ExecuteSkill(Transform playerTransform)
    {
        if (playerTransform == null) return;

        Debug.Log("Executing Perish Skill: Trying to clear obstacles.");

        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, playerTransform.position, Quaternion.identity);
        }

        Collider2D[] obstacles = Physics2D.OverlapCircleAll(playerTransform.position, destroyRadius, obstacleLayer);

        foreach (Collider2D obstacle in obstacles)
        {
            PerishBlueGate gate = obstacle.GetComponentInParent<PerishBlueGate>();
            if (gate != null && gate.AbsorbByPerish(playerTransform))
            {
                Debug.Log($"Cleared Obstacle (Perish): {obstacle.name}");
            }
        }
    }
}