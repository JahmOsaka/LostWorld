using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Start() => UpdateHUD(false);

    private void Update()
    {
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
        if (inputHandler == null) return;

        if (inputHandler.CycleSkillNextInput)
        {
            inputHandler.UseCycleSkillNextInput();
            CycleSkill(1);
        }
        else if (inputHandler.CycleSkillPrevInput)
        {
            inputHandler.UseCycleSkillPrevInput();
            CycleSkill(-1);
        }

        if (inputHandler.SkillInput)
        {
            inputHandler.UseSkillInput();
            if (cooldownTimer <= 0 && CurrentCombatSkill != null)
            {
                if (HasEnoughEnergy())
                {
                    player.SkillState.SetSkill(CurrentCombatSkill);
                    player.StateMachine.ChangeState(player.SkillState);

                    if (CurrentCombatSkill is ActiveSkillData activeSkill)
                    {
                        cooldownTimer = activeSkill.cooldownTime;
                        playerStats?.ConsumeEnergy(activeSkill.mpCost);
                    }
                }
                else if (CurrentCombatSkill is ActiveSkillData)
                {
                    UIManager.Instance?.PlayNotEnoughEnergyFeedback();
                }
            }
        }

        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            hasDoubleJump = !hasDoubleJump;
        }
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            hasPerish = !hasPerish;
        }
    }

    public void CycleSkill(int direction)
    {
        int nextIndex = GetAdjacentSkillIndex(direction);
        if (nextIndex != -1 && nextIndex != activeWheelIndex)
        {
            activeWheelIndex = nextIndex;
            UpdateHUD(true);
        }
    }

    private int GetAdjacentSkillIndex(int direction)
    {
        int count = equippedWheelSkills.Length;
        if (count == 0) return -1;

        int step = direction >= 0 ? 1 : -1;
        for (int i = 1; i < count; i++)
        {
            int candidate = (activeWheelIndex + step * i + count) % count;
            if (equippedWheelSkills[candidate] != null)
            {
                return candidate;
            }
        }
        return -1;
    }

    public void PlayPerishCastVfx()
    {
        if (perishSkillData != null && perishSkillData.vfxPrefab != null)
        {
            Vector3 spawnPos = player != null ? player.transform.position : transform.position;
            Instantiate(perishSkillData.vfxPrefab, spawnPos, Quaternion.identity);
        }
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

        if (CurrentCombatSkill == null)
        {
            int fallback = GetAdjacentSkillIndex(1);
            if (fallback != -1) activeWheelIndex = fallback;
        }

        UpdateHUD(false);
    }

    public void UnlockSkill(SkillData newSkill)
    {
        if (newSkill == null) return;

        if (newSkill.category == SkillCategory.KeyAbility)
        {
            bool unlockedNew = false;

            if (newSkill == perishSkillData || newSkill.skillName == "Perish")
            {
                if (!hasPerish) { hasPerish = true; unlockedNew = true; }
            }
            else if (newSkill == doubleJumpSkillData || newSkill.skillName == "Double Jump")
            {
                if (!hasDoubleJump) { hasDoubleJump = true; unlockedNew = true; }
            }
            else if (newSkill == superDashSkillData || newSkill.skillName == "Super Dash")
            {
                if (!hasSuperDash) { hasSuperDash = true; unlockedNew = true; }
            }

            if (unlockedNew)
            {
                PickupNotificationUI.Instance?.ShowSkillObtained(newSkill);
            }

            return;
        }

        if (HasUnlockedCombatSkill(newSkill)) return;

        unlockedCombatSkills.Add(newSkill);
        PickupNotificationUI.Instance?.ShowSkillObtained(newSkill);

        if (IsSkillEquippedInWheel(newSkill)) return;

        for (int i = 0; i < equippedWheelSkills.Length; i++)
        {
            if (equippedWheelSkills[i] == null)
            {
                equippedWheelSkills[i] = newSkill;
                if (CurrentCombatSkill == null) activeWheelIndex = i;
                UpdateHUD(false);
                break;
            }
        }
    }

    private bool HasUnlockedCombatSkill(SkillData targetSkill)
    {
        for (int i = 0; i < unlockedCombatSkills.Count; i++)
        {
            SkillData existing = unlockedCombatSkills[i];
            if (existing == null) continue;

            if (existing == targetSkill ||
                (!string.IsNullOrEmpty(targetSkill.skillName) && existing.skillName == targetSkill.skillName))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsSkillEquippedInWheel(SkillData targetSkill)
    {
        for (int i = 0; i < equippedWheelSkills.Length; i++)
        {
            SkillData equipped = equippedWheelSkills[i];
            if (equipped == null) continue;

            if (equipped == targetSkill ||
                (!string.IsNullOrEmpty(targetSkill.skillName) && equipped.skillName == targetSkill.skillName))
            {
                return true;
            }
        }
        return false;
    }
    public void SelectWheelSlot(int index)
    {
        if (index >= 0 && index < equippedWheelSkills.Length)
        {
            bool changed = activeWheelIndex != index;
            activeWheelIndex = index;
            UpdateHUD(changed);
        }
    }

    public void UpdateHUD(bool playSwitchFeedback = false)
    {
        if (UIManager.Instance == null) return;

        int prevIdx = GetAdjacentSkillIndex(-1);
        int nextIdx = GetAdjacentSkillIndex(1);

        SkillData prevSkill = prevIdx != -1 ? equippedWheelSkills[prevIdx] : null;
        SkillData nextSkill = nextIdx != -1 ? equippedWheelSkills[nextIdx] : null;

        UIManager.Instance.UpdateSkillCarousel(prevSkill, CurrentCombatSkill, nextSkill, playSwitchFeedback);
    }

    public void LoadSavedSkills(
        bool savedPerish,
        bool savedDoubleJump,
        bool savedSuperDash,
        List<SkillData> savedUnlockedSkills,
        SkillData[] savedEquippedSkills,
        int savedActiveIndex)
    {
        hasPerish = savedPerish;
        hasDoubleJump = savedDoubleJump;
        hasSuperDash = savedSuperDash;

        unlockedCombatSkills.Clear();
        if (savedUnlockedSkills != null)
        {
            foreach (SkillData skill in savedUnlockedSkills)
            {
                if (skill != null && !unlockedCombatSkills.Contains(skill))
                {
                    unlockedCombatSkills.Add(skill);
                }
            }
        }

        for (int i = 0; i < equippedWheelSkills.Length; i++)
        {
            equippedWheelSkills[i] = (savedEquippedSkills != null && i < savedEquippedSkills.Length)
                ? savedEquippedSkills[i]
                : null;
        }

        activeWheelIndex = Mathf.Clamp(savedActiveIndex, 0, Mathf.Max(0, equippedWheelSkills.Length - 1));
        if (CurrentCombatSkill == null)
        {
            int fallback = GetAdjacentSkillIndex(1);
            if (fallback != -1) activeWheelIndex = fallback;
        }

        UpdateHUD(false);
    }
}