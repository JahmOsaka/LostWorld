using UnityEngine;

[CreateAssetMenu(fileName = "NewPerishSkill", menuName = "Data/Skills/Perish Skill (Blue)")]
public class PerishSkillData : SkillData
{
    public float destroyRadius = 2f;
    public LayerMask obstacleLayer;

    public void ExecuteSkill(Transform playerTransform)
    {
        Debug.Log("Executing Perish Skill: Trying to clear obstacles.");
        Collider2D[] obstacles = Physics2D.OverlapCircleAll(playerTransform.position, destroyRadius, obstacleLayer);

        foreach (Collider2D obstacle in obstacles)
        {
            ShortcutGate gate = obstacle.GetComponentInParent<ShortcutGate>();

            if (gate != null)
            {
                gate.Interact();
                Debug.Log($"Cleared Obstacle (Perish): {obstacle.name}");
            }
        }
    }
}