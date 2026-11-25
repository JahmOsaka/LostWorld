using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            TimeSpan time = TimeSpan.FromSeconds(data.playTime);
            string formattedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)time.TotalHours, time.Minutes, time.Seconds);
            string location = string.IsNullOrEmpty(data.locationName) ? "Unknown Area" : data.locationName;

            slotInfoText.text =
                $"Slot {slotIndex + 1} - {location}\n" +
                $"<color=#00FF00>{data.saveTime}</color> | Time: {formattedTime}\n" +
                $"Coins: {data.currentCurrency}";

            loadButton.interactable = true;
            deleteButton.interactable = true;
        }
    }
}