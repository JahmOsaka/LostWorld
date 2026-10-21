using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillEquipUI : MonoBehaviour
{
    [Header("Main Panel")]
    [Tooltip("The root panel of the Skill Select UI.")]
    public GameObject menuPanel;
    public bool IsMenuOpen => menuPanel != null && menuPanel.activeSelf;

    [Header("Left Side: 4 Wheel Slots")]
    public Button[] wheelSlotButtons = new Button[4];
    public Image[] wheelSlotIcons = new Image[4];
    public GameObject[] wheelSlotHighlights = new GameObject[4];

    [Header("Center: Inventory Grid Slots")]
    [Tooltip("Buttons inside the grid representing unlocked combat skills.")]
    public Button[] inventoryButtons;
    public Image[] inventoryIcons;

    [Header("Right Side: Description Panel")]
    public Image largePreviewIcon;
    public TextMeshProUGUI descriptionText;

    private PlayerSkillController skillController;
    private int selectedWheelSlotIndex = 0;

    private void Start()
    {
        for (int i = 0; i < wheelSlotButtons.Length; i++)
        {
            int index = i;
            if (wheelSlotButtons[i] != null)
            {
                wheelSlotButtons[i].onClick.AddListener(() => SelectWheelSlotToEdit(index));
            }
        }

        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            int index = i;
            if (inventoryButtons[i] != null)
            {
                inventoryButtons[i].onClick.AddListener(() => OnInventorySlotClicked(index));
            }
        }

        if (menuPanel != null) menuPanel.SetActive(false);
    }

    private void Update()
    {
        if (IsMenuOpen && UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseMenu();
        }
    }

    public void OpenMenu(PlayerSkillController controller)
    {
        skillController = controller;
        if (menuPanel != null) menuPanel.SetActive(true);

        Time.timeScale = 0f;

        SelectWheelSlotToEdit(0);
        RefreshUI();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);

        Time.timeScale = 1f;

        if (skillController != null)
        {
            skillController.UpdateHUD();
        }
    }

    public void SelectWheelSlotToEdit(int slotIndex)
    {
        selectedWheelSlotIndex = slotIndex;

        for (int i = 0; i < wheelSlotHighlights.Length; i++)
        {
            if (wheelSlotHighlights[i] != null)
                wheelSlotHighlights[i].SetActive(i == selectedWheelSlotIndex);
        }

        if (skillController != null)
        {
            ShowSkillDetails(skillController.equippedWheelSkills[selectedWheelSlotIndex]);
        }
    }

    private void OnInventorySlotClicked(int inventoryIndex)
    {
        if (skillController == null) return;

        if (inventoryIndex < skillController.unlockedCombatSkills.Count)
        {
            SkillData chosenSkill = skillController.unlockedCombatSkills[inventoryIndex];

            for (int i = 0; i < skillController.equippedWheelSkills.Length; i++)
            {
                if (skillController.equippedWheelSkills[i] == chosenSkill)
                {
                    skillController.equippedWheelSkills[i] = null;
                }
            }

            skillController.equippedWheelSkills[selectedWheelSlotIndex] = chosenSkill;

            RefreshUI();
            ShowSkillDetails(chosenSkill);
        }
    }

    private void RefreshUI()
    {
        if (skillController == null) return;

        for (int i = 0; i < wheelSlotIcons.Length; i++)
        {
            if (wheelSlotIcons[i] != null)
            {
                SkillData skill = skillController.equippedWheelSkills[i];
                if (skill != null && skill.skillIcon != null)
                {
                    wheelSlotIcons[i].sprite = skill.skillIcon;
                    wheelSlotIcons[i].enabled = true;
                }
                else
                {
                    wheelSlotIcons[i].enabled = false;
                }
            }
        }

        for (int i = 0; i < inventoryIcons.Length; i++)
        {
            if (inventoryIcons[i] != null)
            {
                if (i < skillController.unlockedCombatSkills.Count)
                {
                    SkillData skill = skillController.unlockedCombatSkills[i];
                    inventoryIcons[i].sprite = skill.skillIcon;
                    inventoryIcons[i].enabled = true;
                    if (inventoryButtons[i] != null) inventoryButtons[i].interactable = true;
                }
                else
                {
                    inventoryIcons[i].enabled = false;
                    if (inventoryButtons[i] != null) inventoryButtons[i].interactable = false;
                }
            }
        }
    }

    private void ShowSkillDetails(SkillData skill)
    {
        if (skill != null)
        {
            if (largePreviewIcon != null)
            {
                largePreviewIcon.sprite = skill.skillIcon;
                largePreviewIcon.enabled = true;
            }
            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{skill.skillName}</b>\n\n{skill.description}\n\n<color=#00FFCC>MP Cost: {skill.mpCost}</color> | <color=#FFCC00>Cooldown: {skill.cooldownTime}s</color>";
            }
        }
        else
        {
            if (largePreviewIcon != null) largePreviewIcon.enabled = false;
            if (descriptionText != null) descriptionText.text = "Empty Slot\n\nSelect an unlocked skill from the list to equip it.";
        }
    }
}