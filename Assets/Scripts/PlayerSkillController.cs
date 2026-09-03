using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Current Absorbed Skill")]
    [SerializeField] private SkillData currentSkill;

    private float cooldownTimer;

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void EquipSkill(SkillData newSkill)
    {
        currentSkill = newSkill;
        cooldownTimer = 0f;
        Debug.Log($"Absorbed new skill: {currentSkill.skillName}");
    }

    public void UseSkill()
    {
        if (currentSkill != null && cooldownTimer <= 0)
        {
            currentSkill.ExecuteSkill(gameObject);
            cooldownTimer = currentSkill.cooldownTime;
        }
        else if (currentSkill == null)
        {
            Debug.Log("No skill equipped");

        }
    }
}
