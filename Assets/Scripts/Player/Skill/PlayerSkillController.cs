using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Current Absorbed Skill")]
    [SerializeField] private SkillData currentSkill;

    [Header("UI Reference")]
    public Image skillIconUI;

    [Header("Input")]
    public PlayerInputHandler inputHandler;

    private float cooldownTimer;

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (inputHandler != null && inputHandler.SkillInput)
        {
            UseSkill();
            inputHandler.UseSkillInput();
        }
    }

    public void EquipSkill(SkillData newSkill)
    {
        currentSkill = newSkill;
        cooldownTimer = 0f;
        UpdateUI();
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

    private void UpdateUI()
    {
        if (skillIconUI != null)
        {
            if (currentSkill != null && currentSkill.skillIcon != null)
            {
                skillIconUI.sprite = currentSkill.skillIcon;
                skillIconUI.enabled = true;
            }
            else
            {
                skillIconUI.enabled = false;
            }
        }
    }
}