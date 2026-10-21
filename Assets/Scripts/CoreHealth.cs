using UnityEngine;
using UnityEngine.Events;
using System;

public class CoreHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points of the entity.")]
    public int maxHealth = 100;
    [Tooltip("Current health points.")]
    public int currentHealth { get; private set; }

    [Header("Hurtbox Settings")]
    [Tooltip("Assign the specific collider designated as the hurtbox. If left unassigned, the main collider will take damage.")]
    [SerializeField] private Collider2D designatedHurtbox;

    [Header("Tech Art / FX Events")]
    [Tooltip("Events triggered when taking damage.")]
    public UnityEvent OnTakDamage;
    [Tooltip("Events triggered when health drops to zero.")]
    public UnityEvent OnDeath;

    private bool isDead;
    private bool isInvincible;

    public static event Action OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        SetupColliders();
    }

    private void SetupColliders()
    {
        if (designatedHurtbox != null)
        {
            designatedHurtbox.isTrigger = true;
        }
    }

    public bool IsValidHurtbox(Collider2D col)
    {
        if (designatedHurtbox != null)
        {
            return col == designatedHurtbox;
        }
        return true;
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

    public void UpgradeMaxHealth(int amount)
    {
        if (isDead) return;

        maxHealth += amount;
        currentHealth = maxHealth;

        OnHealthChanged?.Invoke();
        Debug.Log($"{gameObject.name} upgraded Max HP by {amount}! New Max HP: {maxHealth}");
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