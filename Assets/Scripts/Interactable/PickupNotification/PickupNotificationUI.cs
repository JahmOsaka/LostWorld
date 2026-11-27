using System.Collections.Generic;
using UnityEngine;

public class PickupNotificationUI : MonoBehaviour
{
    public static PickupNotificationUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform listContainer;
    [SerializeField] private PickupNotificationRow rowPrefab;

    [Header("Settings")]
    [SerializeField] private int maxVisibleRows = 4;
    [SerializeField] private float defaultDuration = 3f;

    [Header("Accent Colors")]
    [SerializeField] private Color skillAccentColor = new Color(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private Color keyAbilityAccentColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private Color itemAccentColor = new Color(0.85f, 0.9f, 0.95f, 1f);

    private readonly List<PickupNotificationRow> activeRows = new List<PickupNotificationRow>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Show(string message, Sprite icon = null, Color? accentColor = null, float duration = -1f)
    {
        if (rowPrefab == null || listContainer == null || string.IsNullOrEmpty(message)) return;

        while (activeRows.Count >= maxVisibleRows && activeRows.Count > 0)
        {
            PickupNotificationRow oldest = activeRows[0];
            activeRows.RemoveAt(0);
            if (oldest != null) oldest.ForceDismiss();
        }

        PickupNotificationRow row = Instantiate(rowPrefab, listContainer);
        row.transform.SetAsLastSibling();
        activeRows.Add(row);

        float holdTime = duration > 0f ? duration : defaultDuration;
        Color barColor = accentColor ?? itemAccentColor;
        row.Setup(message, icon, barColor, holdTime, HandleRowDismissed);
    }

    public void ShowSkillObtained(SkillData skill)
    {
        if (skill == null) return;

        bool isKeyAbility = skill.category == SkillCategory.KeyAbility;
        Color barColor = isKeyAbility ? keyAbilityAccentColor : skillAccentColor;
        string colorHex = ColorUtility.ToHtmlStringRGB(barColor);
        string displayName = string.IsNullOrEmpty(skill.skillName) ? skill.name : skill.skillName;

        Show($"Obtained <color=#{colorHex}>{displayName}</color>", skill.skillIcon, barColor);
    }

    public void ShowItemObtained(string itemName, Sprite icon = null, int amount = 1)
    {
        string suffix = amount > 1 ? $" x{amount}" : string.Empty;
        Show($"Obtained <color=#FFFFFF>{itemName}</color>{suffix}", icon, itemAccentColor);
    }

    private void HandleRowDismissed(PickupNotificationRow row)
    {
        activeRows.Remove(row);
    }
}