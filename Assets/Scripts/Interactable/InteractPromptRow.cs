using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractPromptRow : MonoBehaviour
{
    [Header("Row UI References")]
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject selectedFrame;

    [Header("Visual States")]
    [SerializeField] private Color selectedBgColor = new Color(0.1f, 0.1f, 0.12f, 0.85f);
    [SerializeField] private Color unselectedBgColor = new Color(0.08f, 0.08f, 0.1f, 0.45f);
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField] private Color unselectedTextColor = new Color(0.75f, 0.75f, 0.78f, 0.85f);

    public RectTransform RectTransform { get; private set; }

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
    }

    public void Setup(string label, bool isSelected)
    {
        if (labelText != null)
        {
            labelText.text = label;
            labelText.color = isSelected ? selectedTextColor : unselectedTextColor;
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedBgColor : unselectedBgColor;
        }

        if (selectedFrame != null)
        {
            selectedFrame.SetActive(isSelected);
        }
    }
}