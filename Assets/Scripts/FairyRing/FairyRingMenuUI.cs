using System;
using UnityEngine;
using UnityEngine.UI;

public class FairyRingMenuUI : MonoBehaviour
{
    public static event Action<GameObject> OnRestRequested;
    public static event Action<GameObject> OnSaveMenuRequested;

    [Header("UI Panels")]
    public GameObject mainFairyRingPanel;
    public GameObject saveLoadSlotPanel;

    [Header("Menu Buttons")]
    public Button restButton;
    public Button skillButton;
    public Button saveLoadButton;
    public Button fastTravelButton;
    public Button closeButton;

    private GameObject currentPlayer;

    private void Start()
    {
        if (mainFairyRingPanel != null) mainFairyRingPanel.SetActive(false);
        if (saveLoadSlotPanel != null) saveLoadSlotPanel.SetActive(false);

        restButton?.onClick.AddListener(OnRestClicked);
        skillButton?.onClick.AddListener(OnSkillClicked);
        saveLoadButton?.onClick.AddListener(OnSaveLoadClicked);
        fastTravelButton?.onClick.AddListener(OnFastTravelClicked);
        closeButton?.onClick.AddListener(CloseAllMenus);
    }

    private void OnEnable()
    {
        FairyRingActivation.OnFairyRingInteracted += HandleFairyRingInteracted;
        FairyRingActivation.OnPlayerLeftFairyRing += HandlePlayerLeft;

        if (SkillEquipService.Instance != null)
            SkillEquipService.Instance.OnMenuClosed += ReopenMainMenu;
    }

    private void OnDisable()
    {
        FairyRingActivation.OnFairyRingInteracted -= HandleFairyRingInteracted;
        FairyRingActivation.OnPlayerLeftFairyRing -= HandlePlayerLeft;

        if (SkillEquipService.Instance != null)
            SkillEquipService.Instance.OnMenuClosed -= ReopenMainMenu;
    }

    private void HandleFairyRingInteracted(FairyRingActivation ring, GameObject player)
    {
        currentPlayer = player;
        OpenMainMenu();
    }

    private void HandlePlayerLeft(GameObject player)
    {
        CloseAllMenus();
        currentPlayer = null;
    }

    private void OpenMainMenu()
    {
        if (mainFairyRingPanel != null)
        {
            mainFairyRingPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    private void ReopenMainMenu()
    {
        if (currentPlayer != null) OpenMainMenu();
    }

    public void CloseAllMenus()
    {
        if (mainFairyRingPanel != null) mainFairyRingPanel.SetActive(false);
        if (saveLoadSlotPanel != null) saveLoadSlotPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnRestClicked()
    {
        Debug.Log("FairyRingMenu: Rest Requested (Heal & Respawn Enemies)");
        OnRestRequested?.Invoke(currentPlayer);
    }

    private void OnSkillClicked()
    {
        Debug.Log("FairyRingMenu: Skill Menu Requested");
        if (mainFairyRingPanel != null) mainFairyRingPanel.SetActive(false);

        SkillEquipService.Instance.OpenMenuForPlayer(currentPlayer);
    }

    private void OnSaveLoadClicked()
    {
        Debug.Log("FairyRingMenu: Save/Load Menu Requested");
        OnSaveMenuRequested?.Invoke(currentPlayer);

        // mainFairyRingPanel.SetActive(false);
        // saveLoadSlotPanel.SetActive(true);
    }

    private void OnFastTravelClicked()
    {
        Debug.Log("FairyRingMenu: Fast Travel (Coming Soon)");
    }
}