using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Input Reference")]
    public PlayerInputHandler inputHandler;
    [Header("Stats Reference")]
    public PlayerStats playerStats;

    [Header("Key Abilities (Progression)")]
    [Tooltip("Unlocked state for Perish ability (destroys Blue Gates & absorbs low HP enemies).")]
    public bool hasPerish = false;
    public SkillData perishSkillData;

    [Header("Combat Skills Inventory & Wheel")]
    [Tooltip("All combat skills unlocked by the player so far.")]
    public List<SkillData> unlockedCombatSkills = new List<SkillData>();

    [Tooltip("4 slots corresponding to the Hotkey Wheel (0: Up, 1: Right, 2: Down, 3: Left).")]
    public SkillData[] equippedWheelSkills = new SkillData[4];
    public int activeWheelIndex = 0;

    [Header("HUD UI Reference")]
    public Image activeSkillIconUI;

    private float cooldownTimer;

    public SkillData CurrentCombatSkill => equippedWheelSkills[activeWheelIndex];

    private void Awake()
    {
        if (playerStats == null) playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        UpdateHUD();
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (inputHandler == null) return;

        if (inputHandler.SkillInput)
        {
            inputHandler.UseSkillInput();
            UseActiveCombatSkill();
        }

        if (hasPerish && perishSkillData != null && IsNearBlueGate())
        {
            if (inputHandler.InteractInput)
            {
                inputHandler.UseInteractInput();
                perishSkillData.ExecuteSkill(gameObject);
            }
        }
    }

    private bool IsNearBlueGate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("BlueGate")) return true;
        }
        return false;
    }

    public void UseActiveCombatSkill()
    {
        SkillData skill = CurrentCombatSkill;
        if (skill != null && cooldownTimer <= 0)
        {
            if (playerStats != null)
            {
                if (playerStats.currentEnergy >= skill.mpCost)
                {
                    playerStats.ConsumeEnergy(skill.mpCost);

                    skill.ExecuteSkill(gameObject);
                    cooldownTimer = skill.cooldownTime;
                }
                else
                {
                    Debug.Log("Not enough Energy (MP) to use this skill!");
                }
            }
            else
            {
                skill.ExecuteSkill(gameObject);
                cooldownTimer = skill.cooldownTime;
            }
        }
        else if (skill == null)
        {
            Debug.Log("No Combat Skill equipped in the current wheel slot!");
        }
    }

    public void UnlockSkill(SkillData newSkill)
    {
        if (newSkill == null) return;

        if (newSkill.skillType == SkillType.KeySkill)
        {
            if (newSkill == perishSkillData || newSkill.skillName == "Perish")
            {
                hasPerish = true;
                Debug.Log("Key Ability Unlocked: Perish!");
            }
            return;
        }

        if (!unlockedCombatSkills.Contains(newSkill))
        {
            unlockedCombatSkills.Add(newSkill);
            Debug.Log($"Unlocked new Combat Skill: {newSkill.skillName}");

            for (int i = 0; i < equippedWheelSkills.Length; i++)
            {
                if (equippedWheelSkills[i] == null)
                {
                    equippedWheelSkills[i] = newSkill;
                    UpdateHUD();
                    break;
                }
            }
        }
    }

    public void SelectWheelSlot(int index)
    {
        if (index >= 0 && index < 4)
        {
            activeWheelIndex = index;
            UpdateHUD();
        }
    }

    public void UpdateHUD()
    {
        if (activeSkillIconUI != null)
        {
            SkillData skill = CurrentCombatSkill;
            if (skill != null && skill.skillIcon != null)
            {
                activeSkillIconUI.sprite = skill.skillIcon;
                activeSkillIconUI.enabled = true;
            }
            else
            {
                activeSkillIconUI.enabled = false;
            }
        }
    }
}