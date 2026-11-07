using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSkillController : MonoBehaviour
{
    [Header("Key Abilities (Progression)")]
    public bool hasPerish = false;
    public SkillData perishSkillData;

    public bool hasDoubleJump = false;
    public SkillData doubleJumpSkillData;

    public bool hasSuperDash = false;
    public SkillData superDashSkillData;

    [Header("Combat Skills Inventory & Wheel")]
    public List<SkillData> unlockedCombatSkills = new List<SkillData>();
    public SkillData[] equippedWheelSkills = new SkillData[4];
    public int activeWheelIndex = 0;

    [Header("HUD UI Reference")]
    public Image activeSkillIconUI;

    private PlayerInputHandler inputHandler;
    private PlayerStats playerStats;
    private float cooldownTimer;
    private Player player;

    public SkillData CurrentCombatSkill => equippedWheelSkills[activeWheelIndex];

    private void Awake()
    {
        inputHandler = GetComponentInParent<PlayerInputHandler>();
        playerStats = GetComponentInParent<PlayerStats>();
        player = GetComponentInParent<Player>();
    }

    private void Start() => UpdateHUD();

    private void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        if (inputHandler == null) return;

        if (inputHandler.SkillInput)
        {
            inputHandler.UseSkillInput();
            if (cooldownTimer <= 0 && CurrentCombatSkill != null && HasEnoughEnergy())
            {
                player.SkillState.SetSkill(CurrentCombatSkill);
                player.StateMachine.ChangeState(player.SkillState);

                if (CurrentCombatSkill is ActiveSkillData activeSkill)
                {
                    cooldownTimer = activeSkill.cooldownTime;
                    playerStats?.ConsumeEnergy(activeSkill.mpCost);
                }
            }
        }

        if (hasPerish && perishSkillData != null && IsNearBlueGate())
        {
            if (inputHandler.InteractInput)
            {
                inputHandler.UseInteractInput();
                perishSkillData.ExecuteSkill(gameObject);
            }
        }

        // DEBUG : Toggle abilities Keys
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            hasDoubleJump = !hasDoubleJump;

        }
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            hasPerish = !hasPerish;

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

    private bool HasEnoughEnergy()
    {
        if (CurrentCombatSkill is ActiveSkillData activeSkill)
        {
            return playerStats == null || playerStats.currentEnergy >= activeSkill.mpCost;
        }
        return false;
    }

    public void UpdateEquippedSkills(SkillData[] newEquippedSkills)
    {
        newEquippedSkills.CopyTo(equippedWheelSkills, 0);
        UpdateHUD();
    }

    public void UnlockSkill(SkillData newSkill)
    {
        if (newSkill == null) return;

        if (newSkill.category == SkillCategory.KeyAbility)
        {
            if (newSkill == perishSkillData || newSkill.skillName == "Perish")
                hasPerish = true;
            else if (newSkill == doubleJumpSkillData || newSkill.skillName == "Double Jump")
                hasDoubleJump = true;
            else if (newSkill == superDashSkillData || newSkill.skillName == "Super Dash")
                hasSuperDash = true;

            return;
        }

        if (!unlockedCombatSkills.Contains(newSkill))
        {
            unlockedCombatSkills.Add(newSkill);
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
            activeSkillIconUI.sprite = skill != null ? skill.skillIcon : null;
            activeSkillIconUI.enabled = (skill != null && skill.skillIcon != null);
        }
    }
}