using UnityEngine;

[CreateAssetMenu(fileName = "NewPerishSkill", menuName = "Data/Skills/Perish Skill (Blue)")]
public class PerishSkillData : SkillData
{
    public float destroyRadius = 2f;
    public LayerMask obstacleLayer;

    public override void ExecuteSkill(GameObject user)
    {
        Collider2D[] hitObstacles = Physics2D.OverlapCircleAll(user.transform.position, destroyRadius, obstacleLayer);

        bool hitSomething = false;
        foreach (Collider2D obstacle in hitObstacles)
        {
            if (obstacle.CompareTag("BlueGate"))
            {
                Destroy(obstacle.gameObject);
                hitSomething = true;
            }
        }

        if (hitSomething && vfxPrefab != null)
        {
            Instantiate(vfxPrefab, user.transform.position, Quaternion.identity);
        }
    }
}