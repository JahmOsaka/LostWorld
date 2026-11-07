using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;

    [Header("UI References")]
    public TextMeshProUGUI slotInfoText;
    public Button saveButton;
    public Button loadButton;
    public Button deleteButton;

    private SaveMenuUI parentMenu;

    public void Initialize(SaveMenuUI menu)
    {
        parentMenu = menu;

        saveButton.onClick.RemoveAllListeners();
        loadButton.onClick.RemoveAllListeners();
        deleteButton.onClick.RemoveAllListeners();

        saveButton.onClick.AddListener(() => parentMenu.OnSaveClicked(slotIndex));
        loadButton.onClick.AddListener(() => parentMenu.OnLoadClicked(slotIndex));
        deleteButton.onClick.AddListener(() => parentMenu.OnDeleteClicked(slotIndex));
    }

    public void UpdateSlotDisplay(SaveGameData data)
    {
        if (data == null)
        {
            slotInfoText.text = $"Slot {slotIndex + 1}\n<color=#888888>Empty</color>";
            loadButton.interactable = false;
            deleteButton.interactable = false;
        }
        else
        {
            slotInfoText.text = $"Slot {slotIndex + 1}\n<color=#00FF00>{data.saveTime}</color>\nCoins: {data.currentCurrency}";
            loadButton.interactable = true;
            deleteButton.interactable = true;
        }
    }
}