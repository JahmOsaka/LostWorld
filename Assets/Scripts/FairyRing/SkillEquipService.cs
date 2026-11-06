using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillEquipService : MonoBehaviour
{
    public static SkillEquipService Instance { get; private set; }

    [Tooltip("Drop SkillEquipUI to Canvas this")]
    public SkillEquipUI skillEquipMenu;

    private PlayerSkillController currentPlayerSkill;

    public event Action<List<SkillData>, SkillData[]> OnMenuOpened;
    public event Action OnMenuClosed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        FairyRingActivation.OnFairyRingInteracted += HandleOpenMenu;
        FairyRingActivation.OnPlayerLeftFairyRing += HandleCloseMenu;
    }

    private void OnDisable()
    {
        FairyRingActivation.OnFairyRingInteracted -= HandleOpenMenu;
        FairyRingActivation.OnPlayerLeftFairyRing -= HandleCloseMenu;
    }

    private void HandleOpenMenu(FairyRingActivation ring, GameObject player)
    {
        currentPlayerSkill = player.GetComponentInChildren<PlayerSkillController>();
        if (currentPlayerSkill != null && skillEquipMenu != null)
        {
            OnMenuOpened?.Invoke(currentPlayerSkill.unlockedCombatSkills, currentPlayerSkill.equippedWheelSkills);
        }
    }

    private void HandleCloseMenu(GameObject player)
    {
        ExecuteCloseMenu();
    }

    public void CloseMenuFromUI()
    {
        ExecuteCloseMenu();
    }

    private void ExecuteCloseMenu()
    {
        if (skillEquipMenu != null && skillEquipMenu.IsMenuOpen)
        {
            if (currentPlayerSkill != null)
            {
                currentPlayerSkill.UpdateEquippedSkills(skillEquipMenu.GetCurrentWheelSkills());
                currentPlayerSkill = null;
            }
            OnMenuClosed?.Invoke();
        }
    }
}