using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Processors;
using System;

public class CoreHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth { get; private set; }

    [Header("Tech Art / FX Events")]
    public UnityEvent OnTakDamage;
    public UnityEvent OnDeath;

    private bool isDead;
    private bool isInvincible;

    public static event Action OnHealthChanged;
    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void SetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }
    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        OnHealthChanged?.Invoke();
        Debug.Log($"{gameObject.name} healed {amount} HP! Current HP: {currentHealth}");
    }
    public void TakeDamage(int amount)
    {
        if (isInvincible || isDead) return;

        currentHealth -= amount;

        OnTakDamage?.Invoke();
        Debug.Log($"{gameObject.name} took {amount} damage! HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }

        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            player.TriggerCombatState();
        }
        OnHealthChanged?.Invoke();
    }

   public void Die()
    {
        isDead = true;
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} is Dead!");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.enabled = false;
            if (player.InputHandler != null) player.InputHandler.enabled = false;
        }

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.enabled = false;
        }
    }
}
