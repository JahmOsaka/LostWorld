using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    public CoreHealth playerHealth;
    public PlayerStats playerStats;

    [Header("UI Elements")]
    public Image healthBarFill;
    public Image energyCircleFill;
    public GameObject[] potionIcons;
    public TextMeshProUGUI currencyText;

    private void OnEnable()
    {
        CoreHealth.OnHealthChanged += UpdateHealthUI;
        PlayerStats.OnStatsChanged += UpdateStatsUI;
    }

    private void OnDisable()
    {
        CoreHealth.OnHealthChanged -= UpdateHealthUI;
        PlayerStats.OnStatsChanged -= UpdateStatsUI;
    }

    private void Start()
    {
        UpdateHealthUI();
        UpdateStatsUI();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth != null)
        {
            healthBarFill.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        }
    }

    private void UpdateStatsUI()
    {
        if (playerStats != null)
        {
            energyCircleFill.fillAmount = playerStats.currentEnergy / playerStats.maxEnergy;

            for (int i = 0; i < potionIcons.Length; i++)
            {
                potionIcons[i].SetActive(i < playerStats.currentPotions);
            }

            if (currencyText != null)
            {
                currencyText.text = playerStats.currentCurrency.ToString();
            }
        }
    }
}