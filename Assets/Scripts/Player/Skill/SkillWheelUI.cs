using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillWheelUI : MonoBehaviour
{
    [Header("References")]
    public PlayerSkillController skillController;
    public PlayerInputHandler inputHandler;
    public GameObject wheelPanel;

    [Header("Wheel UI Elements (4 Directions)")]
    [Tooltip("Images for the 4 slots: 0=Up, 1=Right, 2=Down, 3=Left")]
    public Image[] slotIcons = new Image[4];
    public GameObject[] selectionHighlights = new GameObject[4];

    [Header("Description Panel Elements")]
    public Image selectedSkillLargeIcon;
    public TextMeshProUGUI descriptionText;

    private int currentHoverIndex = 0;

    private void Start()
    {
        if (wheelPanel != null) wheelPanel.SetActive(false);
    }

    private void Update()
    {
        if (inputHandler == null || skillController == null) return;

        if (inputHandler.WheelMenuInput)
        {
            if (wheelPanel != null && !wheelPanel.activeSelf)
            {
                wheelPanel.SetActive(true);
                Time.timeScale = 0.1f;
            }

            UpdateWheelSelection();
            UpdateDescriptionPanel();
        }
        else
        {
            if (wheelPanel != null && wheelPanel.activeSelf)
            {
                wheelPanel.SetActive(false);
                Time.timeScale = 1f;

                skillController.SelectWheelSlot(currentHoverIndex);
            }
        }
    }

    private void UpdateWheelSelection()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector2 mouseDir = inputHandler.MouseWheelInput - screenCenter;

        if (mouseDir.sqrMagnitude > 400f)
        {
            mouseDir.Normalize();

            if (mouseDir.y > 0.5f && Mathf.Abs(mouseDir.x) < 0.7f) currentHoverIndex = 0;
            else if (mouseDir.x < -0.5f && Mathf.Abs(mouseDir.y) < 0.7f) currentHoverIndex = 1;
            else if (mouseDir.y < -0.5f && Mathf.Abs(mouseDir.x) < 0.7f) currentHoverIndex = 2;
            else if (mouseDir.x > 0.5f && Mathf.Abs(mouseDir.y) < 0.7f) currentHoverIndex = 3;

            skillController.SelectWheelSlot(currentHoverIndex);
        }

        for (int i = 0; i < selectionHighlights.Length; i++)
        {
            if (selectionHighlights[i] != null)
            {
                selectionHighlights[i].SetActive(i == currentHoverIndex);
            }
        }

        for (int i = 0; i < slotIcons.Length; i++)
        {
            if (slotIcons[i] != null)
            {
                SkillData skill = skillController.equippedWheelSkills[i];
                if (skill != null && skill.skillIcon != null)
                {
                    slotIcons[i].sprite = skill.skillIcon;
                    slotIcons[i].enabled = true;
                }
                else
                {
                    slotIcons[i].enabled = false;
                }
            }
        }
    }

    private void UpdateDescriptionPanel()
    {
        SkillData hoveredSkill = skillController.equippedWheelSkills[currentHoverIndex];

        if (hoveredSkill != null)
        {
            if (selectedSkillLargeIcon != null)
            {
                selectedSkillLargeIcon.sprite = hoveredSkill.skillIcon;
                selectedSkillLargeIcon.enabled = true;
            }
            if (descriptionText != null)
            {
                descriptionText.text = $"<b>{hoveredSkill.skillName}</b>\n\n{hoveredSkill.description}\n\n<color=#00FFCC>MP Cost: {hoveredSkill.mpCost}</color> | <color=#FFCC00>Cooldown: {hoveredSkill.cooldownTime}s</color>";
            }
        }
        else
        {
            if (selectedSkillLargeIcon != null) selectedSkillLargeIcon.enabled = false;
            if (descriptionText != null) descriptionText.text = "Empty Slot\n\nVisit a Fairy Ring to equip a combat skill.";
        }
    }
}