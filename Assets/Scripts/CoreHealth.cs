using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;

public class CoreHealth : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [Tooltip("Maximum health points of the entity.")]
    public int maxHealth = 100;
    public int currentHealth { get; private set; }

    [Header("Hurtbox & I-Frames Settings")]
    [Tooltip("Assign the specific collider designated as the hurtbox. If left unassigned, any collider on this object will take damage.")]
    [SerializeField] private Collider2D designatedHurtbox;
    [Tooltip("Duration of invincibility after taking damage to prevent double hits (set 0 for enemies if needed).")]
    [SerializeField] private float invincibilityDuration = 0.2f;

    [Header("Tech Art / FX Events")]
    public UnityEvent OnTakDamage;
    public UnityEvent OnDeath;

    private bool isDead;
    private bool isInvincible;
    private Coroutine invincibilityCoroutine;

    public static event Action OnHealthChanged;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (designatedHurtbox != null)
        {
            designatedHurtbox.isTrigger = true;
        }
    }

    public void InitializeMaxHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke();
    }

    public bool CanBeDamagedBy(Collider2D col) => IsValidHurtbox(col);

    public bool IsValidHurtbox(Collider2D col)
    {
        if (isDead || isInvincible) return false;
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

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
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
        else if (invincibilityDuration > 0f)
        {
            if (invincibilityCoroutine != null) StopCoroutine(invincibilityCoroutine);
            invincibilityCoroutine = StartCoroutine(InvincibilityRoutine());
        }

        Player player = GetComponentInParent<Player>();
        if (player != null)
        {
            player.TriggerCombatState();
        }
        OnHealthChanged?.Invoke();
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} is Dead!");

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (designatedHurtbox != null) designatedHurtbox.enabled = false;

        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.enabled = false;
            if (player.InputHandler != null) player.InputHandler.enabled = false;
        }

        Enemy enemy = GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.ClearActiveOrbs();
            enemy.enabled = false;
        }
    }
}