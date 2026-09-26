using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    [Header("Player References")]
    public CoreHealth playerHealth;
    public PlayerStats playerStats;

    [Header("UI Elements: Bars")]
    public Image healthBarFill;
    public Image energyBarFill;

    [Header("UI Elements: Skill")]
    public Image currentSkillIcon;

    [Header("UI Elements: Life Gems (Potions)")]
    public Image[] potionIcons;
    public Sprite gemFullSprite;
    public Sprite gemEmptySprite;

    [Header("UI Elements: Others")]
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
        if (playerHealth != null && healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        }
    }

    private void UpdateStatsUI()
    {
        if (playerStats != null)
        {
            if (energyBarFill != null)
            {
                energyBarFill.fillAmount = playerStats.currentEnergy / playerStats.maxEnergy;
            }

            if (potionIcons != null)
            {
                for (int i = 0; i < potionIcons.Length; i++)
                {
                    if (potionIcons[i] != null)
                    {
                        potionIcons[i].sprite = (i < playerStats.currentPotions) ? gemFullSprite : gemEmptySprite;
                    }
                }
            }

            if (currencyText != null)
            {
                currencyText.text = playerStats.currentCurrency.ToString();
            }
        }
    }

    public void UpdateSkillIcon(Sprite newSkillSprite)
    {
        if (currentSkillIcon != null && newSkillSprite != null)
        {
            currentSkillIcon.sprite = newSkillSprite;
        }
    }
}