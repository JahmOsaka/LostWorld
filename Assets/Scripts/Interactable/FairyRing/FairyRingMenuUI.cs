using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    private readonly List<Button> menuButtons = new List<Button>();
    private GameObject currentPlayer;
    private int selectedIndex = 0;
    private int openedFrame = -1;

    private void Start()
    {
        if (mainFairyRingPanel != null) mainFairyRingPanel.SetActive(false);
        if (saveLoadSlotPanel != null) saveLoadSlotPanel.SetActive(false);

        restButton?.onClick.AddListener(OnRestClicked);
        skillButton?.onClick.AddListener(OnSkillClicked);
        saveLoadButton?.onClick.AddListener(OnSaveLoadClicked);
        fastTravelButton?.onClick.AddListener(OnFastTravelClicked);
        closeButton?.onClick.AddListener(CloseAllMenus);

        BuildButtonList();
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

        InteractPromptUI.Instance?.SetSuppressed(this, false);
    }

    private void Update()
    {
        if (mainFairyRingPanel == null || !mainFairyRingPanel.activeInHierarchy) return;
        if (Time.frameCount == openedFrame) return;

        Keyboard kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            CloseAllMenus();
            return;
        }

        if (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)
        {
            MoveSelection(-1);
        }
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)
        {
            MoveSelection(1);
        }
        else if (kb.fKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            ConfirmSelection();
        }
    }

    private void BuildButtonList()
    {
        menuButtons.Clear();
        if (restButton != null) menuButtons.Add(restButton);
        if (skillButton != null) menuButtons.Add(skillButton);
        if (saveLoadButton != null) menuButtons.Add(saveLoadButton);
        if (fastTravelButton != null) menuButtons.Add(fastTravelButton);
        if (closeButton != null) menuButtons.Add(closeButton);
    }

    private void MoveSelection(int direction)
    {
        if (menuButtons.Count == 0) return;

        int count = menuButtons.Count;
        for (int i = 1; i <= count; i++)
        {
            int candidate = (selectedIndex + direction * i + count) % count;
            if (IsButtonSelectable(menuButtons[candidate]))
            {
                SelectButton(candidate);
                return;
            }
        }
    }

    private void SelectButton(int index)
    {
        if (index < 0 || index >= menuButtons.Count) return;

        selectedIndex = index;
        Button target = menuButtons[selectedIndex];
        if (target != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(target.gameObject);
        }
    }

    private void SelectFirstAvailableButton()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            if (IsButtonSelectable(menuButtons[i]))
            {
                SelectButton(i);
                return;
            }
        }
    }

    private void ConfirmSelection()
    {
        if (selectedIndex < 0 || selectedIndex >= menuButtons.Count) return;

        Button target = menuButtons[selectedIndex];
        if (IsButtonSelectable(target))
        {
            target.onClick.Invoke();
        }
    }

    private bool IsButtonSelectable(Button btn)
    {
        return btn != null && btn.gameObject.activeInHierarchy && btn.interactable;
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
            openedFrame = Time.frameCount;
            mainFairyRingPanel.SetActive(true);
            Time.timeScale = 0f;

            InteractPromptUI.Instance?.SetSuppressed(this, true);
            SelectFirstAvailableButton();
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

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        InteractPromptUI.Instance?.SetSuppressed(this, false);
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

    }

    private void OnFastTravelClicked()
    {
        Debug.Log("FairyRingMenu: Fast Travel (Coming Soon)");
    }
}