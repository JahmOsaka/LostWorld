using System.Collections;
using UnityEngine;

public class HitKnockback : MonoBehaviour
{
    [Header("Knockback Settings")]
    [Tooltip("Set to 0 for immovable enemies or heavy bosses.")]
    [SerializeField] private float knockbackMultiplier = 1f;
    [SerializeField] private float knockbackDuration = 0.12f;

    private Rigidbody2D rb;
    private Coroutine knockbackCoroutine;

    public bool IsKnockedBack { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockback(int direction, float force)
    {
        if (rb == null || !rb.simulated || knockbackMultiplier <= 0f || !gameObject.activeInHierarchy)
            return;

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
        }

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(direction, force * knockbackMultiplier));
    }

    private IEnumerator KnockbackRoutine(int direction, float startSpeed)
    {
        IsKnockedBack = true;
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            if (rb == null || !rb.simulated) break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            float currentX = Mathf.Lerp(startSpeed, 0f, t);

            rb.linearVelocity = new Vector2(direction * currentX, rb.linearVelocity.y);
            yield return new WaitForFixedUpdate();
        }

        if (rb != null && rb.simulated)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        IsKnockedBack = false;
        knockbackCoroutine = null;
    }

    private void OnDisable()
    {
        IsKnockedBack = false;
        knockbackCoroutine = null;
    }
}