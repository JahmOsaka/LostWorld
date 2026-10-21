using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Energy / Mana")]
    public float maxEnergy = 100f;
    public float currentEnergy { get; private set; }

    [Header("Potions")]
    public int maxPotions = 2;
    public int currentPotions { get; private set; }
    public int potionHealAmount = 30;

    [Header("Currency")]
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

    private void Update()
    {
        if (player.InputHandler.PotionInput)
        {
            player.InputHandler.UsePotionInput();

            UsePotion();
        }
    }

    public void UsePotion()
    {
        if (currentPotions > 0 && health.currentHealth < health.maxHealth)
        {
            currentPotions--;

            health.Heal(potionHealAmount); 

            OnStatsChanged?.Invoke();
            Debug.Log("taken the medicine! Leftover: " + currentPotions);
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