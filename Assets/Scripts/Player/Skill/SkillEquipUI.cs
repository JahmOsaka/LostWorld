using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillEquipUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject menuPanel;
    public bool IsMenuOpen => menuPanel != null && menuPanel.activeSelf;

    [Header("Left Side: 4 Wheel Slots")]
    public Button[] wheelSlotButtons = new Button[4];
    public Image[] wheelSlotIcons = new Image[4];
    public GameObject[] wheelSlotHighlights = new GameObject[4];

    [Header("Center: Inventory Grid Slots")]
    public Button[] inventoryButtons;
    public Image[] inventoryIcons;

    [Header("Right Side: Description Panel")]
    public Image largePreviewIcon;
    public TextMeshProUGUI descriptionText;

    private List<SkillData> localUnlockedSkills;
    private SkillData[] localEquippedSkills = new SkillData[4];
    private int selectedWheelSlotIndex = 0;

    private void Start()
    {
        for (int i = 0; i < wheelSlotButtons.Length; i++)
        {
            int index = i;
            if (wheelSlotButtons[i] != null)
                wheelSlotButtons[i].onClick.AddListener(() => SelectWheelSlotToEdit(index));
        }

        for (int i = 0; i < inventoryButtons.Length; i++)
        {
            int index = i;
            if (inventoryButtons[i] != null)
                inventoryButtons[i].onClick.AddListener(() => OnInventorySlotClicked(index));
        }

        if (menuPanel != null) menuPanel.SetActive(false);

        SkillEquipService.Instance.OnMenuOpened += OpenMenu;
        SkillEquipService.Instance.OnMenuClosed += CloseMenu;
    }

    private void OnDestroy()
    {
        if (SkillEquipService.Instance != null)
        {
            SkillEquipService.Instance.OnMenuOpened -= OpenMenu;
            SkillEquipService.Instance.OnMenuClosed -= CloseMenu;
        }
    }

    private void Update()
    {
        if (IsMenuOpen && UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SkillEquipService.Instance.CloseMenuFromUI();
        }
    }

    private void OpenMenu(List<SkillData> unlocked, SkillData[] equipped)
    {
        localUnlockedSkills = new List<SkillData>(unlocked);
        equipped.CopyTo(localEquippedSkills, 0);

        if (menuPanel != null) menuPanel.SetActive(true);
        Time.timeScale = 0f;

        SelectWheelSlotToEdit(0);
        RefreshUI();
    }

    public void CloseMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public SkillData[] GetCurrentWheelSkills()
    {
        return localEquippedSkills;
    }

    public void SelectWheelSlotToEdit(int slotIndex)
    {
        selectedWheelSlotIndex = slotIndex;

        for (int i = 0; i < wheelSlotHighlights.Length; i++)
        {
            if (wheelSlotHighlights[i] != null)
                wheelSlotHighlights[i].SetActive(i == selectedWheelSlotIndex);
        }

        ShowSkillDetails(localEquippedSkills[selectedWheelSlotIndex]);
    }

    private void OnInventorySlotClicked(int inventoryIndex)
    {
        if (localUnlockedSkills == null || inventoryIndex >= localUnlockedSkills.Count) return;

        SkillData chosenSkill = localUnlockedSkills[inventoryIndex];

        for (int i = 0; i < localEquippedSkills.Length; i++)
        {
            if (localEquippedSkills[i] == chosenSkill)
                localEquippedSkills[i] = null;
        }

        localEquippedSkills[selectedWheelSlotIndex] = chosenSkill;

        RefreshUI();
        ShowSkillDetails(chosenSkill);
    }

    private void RefreshUI()
    {
        for (int i = 0; i < wheelSlotIcons.Length; i++)
        {
            if (wheelSlotIcons[i] != null)
            {
                SkillData skill = localEquippedSkills[i];
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
                if (i < localUnlockedSkills.Count)
                {
                    SkillData skill = localUnlockedSkills[i];
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
                if (skill is ActiveSkillData activeSkill)
                {
                    descriptionText.text = $"<b>{skill.skillName}</b>\n\n{skill.description}\n\n<color=#00FFCC>MP Cost: {activeSkill.mpCost}</color> | <color=#FFCC00>Cooldown: {activeSkill.cooldownTime}s</color>";
                }
                else
                {
                    descriptionText.text = $"<b>{skill.skillName}</b>\n\n{skill.description}";
                }
            }
        }
        else
        {
            if (largePreviewIcon != null) largePreviewIcon.enabled = false;
            if (descriptionText != null) descriptionText.text = "Empty Slot\n\nSelect an unlocked skill from the list to equip it.";
        }
    }
}