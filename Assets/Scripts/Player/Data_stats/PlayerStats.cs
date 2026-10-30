using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float maxEnergy => (player != null && player.playerData != null) ? player.playerData.maxEnergy : 100f;
    public int maxPotions => (player != null && player.playerData != null) ? player.playerData.maxPotions : 2;
    public int potionHealAmount => (player != null && player.playerData != null) ? player.playerData.potionHealAmount : 30;

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
            Debug.Log("Taken medicine! Leftover: " + currentPotions);
        }
    }

    public void ConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            OnStatsChanged?.Invoke();
        }
    }

    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        OnStatsChanged?.Invoke();
    }
}