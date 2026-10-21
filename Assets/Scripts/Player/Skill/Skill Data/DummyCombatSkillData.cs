using UnityEngine;

[CreateAssetMenu(fileName = "NewDummyCombatSkill", menuName = "Data/Skills/Dummy Combat Skill")]
public class DummyCombatSkillData : SkillData
{
    [Header("Dummy Skill Settings")]
    public float attackRadius = 2.5f;
    public int skillDamage = 30;
    public LayerMask enemyLayer;

    public override void ExecuteSkill(GameObject user)
    {
        Debug.Log($"[Combat Skill Executed] {skillName}! Dealt {skillDamage} damage around player.");

        if (vfxPrefab != null)
        {
            Instantiate(vfxPrefab, user.transform.position, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, attackRadius, enemyLayer);
        foreach (var hit in hits)
        {
            CoreHealth enemyHealth = hit.GetComponent<CoreHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(skillDamage);
            }
        }
    }
}