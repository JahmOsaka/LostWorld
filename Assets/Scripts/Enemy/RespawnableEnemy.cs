using UnityEngine;

public class RespawnableEnemy : MonoBehaviour
{
    [Header("Respawn Settings")]
    [Tooltip("If enemy can respawn")]
    public bool respawnsOnRest = true;

    private Vector3 initialPosition;
    private CoreHealth health;
    private Enemy enemyScript;

    private void Awake()
    {
        initialPosition = transform.position;
        health = GetComponent<CoreHealth>();
        enemyScript = GetComponent<Enemy>();
    }

    private void Start()
    {
        EnemyRespawnManager.RegisterEnemy(this);
    }

    private void OnDestroy()
    {
        EnemyRespawnManager.UnregisterEnemy(this);
    }

    public void Respawn()
    {
        if (this == null || gameObject == null || enemyScript == null || enemyScript.anim == null) return;
        if (!respawnsOnRest && health.currentHealth <= 0) return;

        gameObject.SetActive(true);
        transform.position = initialPosition;
        transform.localScale = Vector3.one;

        if (health != null)
        {
            health.Revive();
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (enemyScript != null)
        {
            enemyScript.enabled = true;
            enemyScript.ResetEnemyState();
        }
    }
}
