using UnityEngine;
using UnityEngine.UI;

public class SaveMenuUI : MonoBehaviour
{
    [Header("Main Panel")]
    public GameObject saveLoadPanel;

    [Header("Slots")]
    public SaveSlotUI[] saveSlots = new SaveSlotUI[4];

    [Header("Navigation")]
    public Button backButton;

    private GameObject currentPlayer;

    private void Start()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            if (saveSlots[i] != null)
            {
                saveSlots[i].slotIndex = i;
                saveSlots[i].Initialize(this);
            }
        }

        backButton?.onClick.AddListener(CloseMenu);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);
    }

    private void OnEnable()
    {
        FairyRingMenuUI.OnSaveMenuRequested += OpenMenu;
    }

    private void OnDisable()
    {
        FairyRingMenuUI.OnSaveMenuRequested -= OpenMenu;
    }

    private void OpenMenu(GameObject player)
    {
        currentPlayer = player;

        FindAnyObjectByType<FairyRingMenuUI>()?.mainFairyRingPanel.SetActive(false);
        if (saveLoadPanel != null) saveLoadPanel.SetActive(true);

        RefreshAllSlots();
    }

    private void CloseMenu()
    {
        if (saveLoadPanel != null) saveLoadPanel.SetActive(false);

        FindAnyObjectByType<FairyRingMenuUI>()?.mainFairyRingPanel.SetActive(true);
    }

    private void RefreshAllSlots()
    {
        for (int i = 0; i < saveSlots.Length; i++)
        {
            SaveGameData data = SaveManager.Instance.GetSaveData(i);
            saveSlots[i].UpdateSlotDisplay(data);
        }
    }

    public void OnSaveClicked(int slotIndex)
    {
        SaveManager.Instance.SaveGame(slotIndex, currentPlayer);
        RefreshAllSlots();
    }

    public void OnLoadClicked(int slotIndex)
    {
        SaveManager.Instance.LoadGame(slotIndex, currentPlayer);

        CloseMenu();
        FindAnyObjectByType<FairyRingMenuUI>()?.CloseAllMenus();
    }

    public void OnDeleteClicked(int slotIndex)
    {
        SaveManager.Instance.DeleteSave(slotIndex);
        RefreshAllSlots();
    }
}