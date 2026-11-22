using UnityEngine;

[RequireComponent(typeof(CoreHealth))]
public class RespawnableEnemy : MonoBehaviour
{
    [Tooltip("If true, respawns when player rests. If false, stays dead (like a boss).")]
    public bool respawnsOnRest = true;

    [Tooltip("Assign the child GameObject named 'Visual' here. This ensures it can be reactivated if disabled by death effects.")]
    public GameObject visualObject;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 initialVisualScale;
    private CoreHealth health;
    private Enemy enemyScript;

    private void Awake()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;
        health = GetComponent<CoreHealth>();
        enemyScript = GetComponent<Enemy>();

        if (visualObject == null)
        {
            Transform v = transform.Find("Visual");
            if (v != null)
                visualObject = v.gameObject;
        }

        if (visualObject != null)
        {
            initialVisualScale = visualObject.transform.localScale;
        }
    }

    public void Respawn()
    {
        if (this == null || gameObject == null || enemyScript == null || enemyScript.anim == null) return;

        if (!respawnsOnRest && health.currentHealth <= 0) return;

        gameObject.SetActive(true);
        transform.position = initialPosition;

        if (visualObject != null)
        {
            visualObject.SetActive(true);

            SpriteRenderer sr = visualObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;
                sr.color = Color.white;
            }
        }

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

        transform.localScale = initialScale;
        if (visualObject != null)
        {
            visualObject.transform.localScale = initialVisualScale;
        }
    }
}