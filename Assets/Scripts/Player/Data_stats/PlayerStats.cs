using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float baseMaxEnergy => (player != null && player.playerData != null) ? player.playerData.maxEnergy : 100f;
    public float maxEnergy => baseMaxEnergy + bonusMaxEnergy;
    public float energyRegenPerWhipHit => (player != null && player.playerData != null) ? player.playerData.energyRegenPerWhipHit : 10f;
    public float maxEnergyPerCollectable => (player != null && player.playerData != null) ? player.playerData.maxEnergyPerCollectable : 20f;

    public int maxPotions => (player != null && player.playerData != null) ? player.playerData.maxPotions : 2;
    public int potionHealAmount => (player != null && player.playerData != null) ? player.playerData.potionHealAmount : 30;

    public float bonusMaxEnergy { get; private set; }
    public float currentEnergy { get; private set; }
    public int currentPotions { get; private set; }
    public int currentCurrency { get; private set; }

    public static event Action OnStatsChanged;

    private CoreHealth health;
    private Player player;

    private void Awake()
    {
        health = GetComponent<CoreHealth>();
        player = GetComponent<Player>();

        bonusMaxEnergy = 0f;
        currentEnergy = maxEnergy;
        currentPotions = maxPotions;
        currentCurrency = 0;
    }

    private void Start()
    {
        OnStatsChanged?.Invoke();
    }

    private void Update()
    {
        if (player != null && player.InputHandler != null && player.InputHandler.PotionInput)
        {
            player.InputHandler.UsePotionInput();
            UsePotion();
        }
    }

    public void UsePotion()
    {
        if (currentPotions > 0 && health != null && health.currentHealth < health.maxHealth)
        {
            currentPotions--;
            health.Heal(potionHealAmount);
            OnStatsChanged?.Invoke();
            Debug.Log("Used Life Gem! Remaining: " + currentPotions);
        }
    }

    public void ConsumeEnergy(float amount)
    {
        if (amount <= 0f) return;

        if (currentEnergy >= amount)
        {
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            OnStatsChanged?.Invoke();
        }
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f || currentEnergy >= maxEnergy) return;

        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
        OnStatsChanged?.Invoke();
    }

    public void RefillEnergy()
    {
        currentEnergy = maxEnergy;
        OnStatsChanged?.Invoke();
    }

    public void RefillPotions()
    {
        currentPotions = maxPotions;
        OnStatsChanged?.Invoke();
    }

    public void CollectMaxEnergyUpgrade(float customBonus = -1f)
    {
        float bonus = customBonus > 0f ? customBonus : maxEnergyPerCollectable;
        bonusMaxEnergy += bonus;
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + bonus);
        OnStatsChanged?.Invoke();

        PickupNotificationUI.Instance?.Show($"Max MP <color=#59D8FF>+{bonus:0}</color>", null, new Color(0.35f, 0.85f, 1f));
    }
    public void SetBonusMaxEnergy(float savedBonus, bool refillToMax = false)
    {
        bonusMaxEnergy = Mathf.Max(0f, savedBonus);
        currentEnergy = refillToMax ? maxEnergy : Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        OnStatsChanged?.Invoke();
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        OnStatsChanged?.Invoke();
    }

    public int DropAllCurrency()
    {
        int droppedAmount = currentCurrency;
        currentCurrency = 0;
        OnStatsChanged?.Invoke();
        return droppedAmount;
    }

    public void RetrieveCurrency(int amount)
    {
        currentCurrency += amount;
        OnStatsChanged?.Invoke();
    }

    public void LoadSavedStats(float savedBonusMaxEnergy, float savedCurrentEnergy, int savedPotions, int savedCurrency)
    {
        bonusMaxEnergy = Mathf.Max(0f, savedBonusMaxEnergy);
        currentEnergy = Mathf.Clamp(savedCurrentEnergy, 0f, maxEnergy);
        currentPotions = Mathf.Clamp(savedPotions, 0, maxPotions);
        currentCurrency = Mathf.Max(0, savedCurrency);
        OnStatsChanged?.Invoke();
    }
}