using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SkillPickup : MonoBehaviour
{
    public SkillData skillToGive;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerSkillController skillController = collision.GetComponentInParent<PlayerSkillController>();
            if (skillController != null)
            {
                skillController.EquipSkill(skillToGive);
                Destroy(gameObject);
            }
        }
    }
}