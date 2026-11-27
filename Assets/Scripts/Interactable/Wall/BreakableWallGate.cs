using System.Collections;
using UnityEngine;

public class BreakableWall : MonoBehaviour, IDamageable
{
    public enum BreakableSide
    {
        Left,
        Right,
        Both
    }

    [Header("Optional Save ID")]
    public string wallID;
    public bool isBroken = false;

    [Header("References")]
    [Tooltip("Visual GameObject of the wall (will be hidden when broken).")]
    [SerializeField] private GameObject wallVisual;
    [Tooltip("Solid collider blocking player movement.")]
    [SerializeField] private Collider2D solidCollider;
    [Tooltip("Optional hurtbox collider for weapon hits. If left empty, any collider on this object accepts hits.")]
    [SerializeField] private Collider2D designatedHurtbox;

    [Header("Breakable Direction")]
    [Tooltip("Side from which attacks can damage and break this wall (Green = breakable, Red = blocked).")]
    [SerializeField] private BreakableSide breakableSide = BreakableSide.Left;

    [Header("Durability")]
    [Tooltip("Total durability (hits or HP) required to shatter the wall.")]
    [SerializeField] private int maxDurability = 3;
    [Tooltip("If true, every valid attack deals 1 durability damage regardless of weapon damage.")]
    [SerializeField] private bool countByHits = true;
    [Tooltip("Minimum delay between hits to prevent a single swing from hitting multiple times.")]
    [SerializeField] private float hitCooldown = 0.12f;

    [Header("Wrong Side Reaction (DialogueUI)")]
    [Tooltip("If true, hitting from the wrong side plays a player reaction line.")]
    [SerializeField] private bool showWrongSideReaction = true;
    [TextArea(2, 3)]
    [SerializeField] private string wrongSideReactionText = "It's too thick to break from this side.";
    [SerializeField] private float reactionDuration = 2.5f;
    [Tooltip("(Optional) Overrides the player's default portrait for this reaction.")]
    [SerializeField] private Sprite reactionOverridePortrait;
    [Tooltip("Cooldown in seconds before the wrong-side reaction can play again.")]
    [SerializeField] private float wrongSideReactionCooldown = 3.0f;

    [Header("Hit & Break Feel")]
    [Tooltip("Optional sprites representing progressive cracks (index 0 = intact, last = almost broken).")]
    [SerializeField] private Sprite[] crackStageSprites;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.08f;
    [SerializeField] private float rattleMagnitude = 0.06f;
    [SerializeField] private float hitRattleDuration = 0.14f;
    [SerializeField] private float wrongSideRattleDuration = 0.12f;
    [SerializeField] private float hitstopOnHit = 0.03f;
    [SerializeField] private float hitstopOnBreak = 0.07f;
    [SerializeField] private float breakShakeDuration = 0.14f;
    [SerializeField] private float breakShakeMagnitude = 0.12f;
    [Tooltip("Optional debris/spark VFX spawned on each valid hit.")]
    [SerializeField] private GameObject hitVfxPrefab;
    [Tooltip("Optional dull spark VFX spawned when hitting from the wrong side.")]
    [SerializeField] private GameObject wrongSideHitVfxPrefab;
    [Tooltip("Optional shatter VFX spawned when the wall breaks.")]
    [SerializeField] private GameObject breakVfxPrefab;

    private int currentDurability;
    private float lastHitTime = -999f;
    private float lastWrongSideReactionTime = -999f;
    private Coroutine hitFeedbackCoroutine;

    private Transform cachedPlayerTransform;
    private Vector3 visualBaseLocalPos;
    private SpriteRenderer[] visualRenderers;
    private Color[] originalColors;
    private SpriteRenderer primarySpriteRenderer;
    private Sprite initialSprite;
    private bool hasCachedVisual;

    private void Awake()
    {
        CacheVisualIfNeeded();
        currentDurability = Mathf.Max(1, maxDurability);
    }

    public bool CanBeDamagedBy(Collider2D col)
    {
        if (isBroken) return false;
        if (designatedHurtbox != null) return col == designatedHurtbox;
        return true;
    }

    public void TakeDamage(int amount)
    {
        if (isBroken) return;
        if (Time.time - lastHitTime < hitCooldown) return;
        lastHitTime = Time.time;

        if (!IsPlayerOnBreakableSide())
        {
            HandleWrongSideHit();
            return;
        }

        int damageTaken = countByHits ? 1 : Mathf.Max(1, amount);
        currentDurability = Mathf.Max(0, currentDurability - damageTaken);

        if (currentDurability <= 0)
        {
            Die();
        }
        else
        {
            HandleValidHit();
        }
    }

    public void Die()
    {
        if (isBroken) return;

        isBroken = true;
        currentDurability = 0;

        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }

        if (GameFeelManager.Instance != null && hitstopOnBreak > 0f)
        {
            GameFeelManager.Instance.TriggerHitstop(hitstopOnBreak);
        }

        TriggerCameraShake(breakShakeDuration, breakShakeMagnitude);

        if (solidCollider != null) solidCollider.enabled = false;
        if (designatedHurtbox != null) designatedHurtbox.enabled = false;

        if (breakVfxPrefab != null)
        {
            Vector3 spawnPos = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
            GameObject fx = Instantiate(breakVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        if (wallVisual != null)
        {
            wallVisual.SetActive(false);
        }
    }

    public void BreakImmediate()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }

        isBroken = true;
        currentDurability = 0;

        if (solidCollider != null) solidCollider.enabled = false;
        if (designatedHurtbox != null) designatedHurtbox.enabled = false;
        if (wallVisual != null) wallVisual.SetActive(false);
    }

    public void ResetWall()
    {
        if (hitFeedbackCoroutine != null)
        {
            StopCoroutine(hitFeedbackCoroutine);
            hitFeedbackCoroutine = null;
        }

        CacheVisualIfNeeded();

        isBroken = false;
        currentDurability = Mathf.Max(1, maxDurability);

        if (solidCollider != null) solidCollider.enabled = true;
        if (designatedHurtbox != null) designatedHurtbox.enabled = true;

        if (wallVisual != null)
        {
            wallVisual.transform.localPosition = visualBaseLocalPos;
            RestoreVisualColors();
            wallVisual.SetActive(true);
        }

        if (primarySpriteRenderer != null)
        {
            if (crackStageSprites != null && crackStageSprites.Length > 0 && crackStageSprites[0] != null)
                primarySpriteRenderer.sprite = crackStageSprites[0];
            else if (initialSprite != null)
                primarySpriteRenderer.sprite = initialSprite;
        }
    }

    private bool IsPlayerOnBreakableSide()
    {
        if (breakableSide == BreakableSide.Both) return true;

        Transform player = GetPlayerTransform();
        if (player == null) return false;

        float referenceX = (solidCollider != null) ? solidCollider.bounds.center.x : transform.position.x;
        return breakableSide == BreakableSide.Left
            ? player.position.x < referenceX
            : player.position.x > referenceX;
    }

    private Transform GetPlayerTransform()
    {
        if (cachedPlayerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player player = playerObj.GetComponentInParent<Player>();
                cachedPlayerTransform = player != null ? player.transform : playerObj.transform;
            }
        }
        return cachedPlayerTransform;
    }

    private void HandleValidHit()
    {
        UpdateCrackVisual();

        if (GameFeelManager.Instance != null && hitstopOnHit > 0f)
        {
            GameFeelManager.Instance.TriggerHitstop(hitstopOnHit);
        }

        TriggerCameraShake(0.08f, breakShakeMagnitude * 0.55f);

        if (hitVfxPrefab != null)
        {
            Vector3 spawnPos = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
            GameObject fx = Instantiate(hitVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(fx, 1.2f);
        }

        if (hitFeedbackCoroutine != null) StopCoroutine(hitFeedbackCoroutine);
        hitFeedbackCoroutine = StartCoroutine(HitFeedbackRoutine(true));
    }

    private void HandleWrongSideHit()
    {
        TriggerCameraShake(0.06f, breakShakeMagnitude * 0.3f);

        if (wrongSideHitVfxPrefab != null)
        {
            Vector3 spawnPos = (solidCollider != null) ? solidCollider.bounds.center : transform.position;
            GameObject fx = Instantiate(wrongSideHitVfxPrefab, spawnPos, Quaternion.identity);
            Destroy(fx, 1.0f);
        }

        if (showWrongSideReaction &&
            !string.IsNullOrEmpty(wrongSideReactionText) &&
            Time.time - lastWrongSideReactionTime >= wrongSideReactionCooldown)
        {
            lastWrongSideReactionTime = Time.time;
            DialogueUI.Instance?.ShowPlayerReaction(wrongSideReactionText, reactionDuration, reactionOverridePortrait);
        }

        if (hitFeedbackCoroutine != null) StopCoroutine(hitFeedbackCoroutine);
        hitFeedbackCoroutine = StartCoroutine(HitFeedbackRoutine(false));
    }

    private void UpdateCrackVisual()
    {
        if (primarySpriteRenderer == null || crackStageSprites == null || crackStageSprites.Length == 0) return;

        float damageProgress = 1f - ((float)currentDurability / Mathf.Max(1, maxDurability));
        int stageIndex = Mathf.Clamp(
            Mathf.FloorToInt(damageProgress * crackStageSprites.Length),
            0,
            crackStageSprites.Length - 1
        );

        if (crackStageSprites[stageIndex] != null)
        {
            primarySpriteRenderer.sprite = crackStageSprites[stageIndex];
        }
    }

    private IEnumerator HitFeedbackRoutine(bool validHit)
    {
        CacheVisualIfNeeded();

        if (validHit)
        {
            ApplyVisualTint(hitFlashColor, 0.85f);
        }

        float elapsed = 0f;
        float duration = validHit ? hitRattleDuration : wrongSideRattleDuration;
        float mag = validHit ? rattleMagnitude * 1.2f : rattleMagnitude * 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (validHit && elapsed >= hitFlashDuration)
            {
                RestoreVisualColors();
            }

            if (wallVisual != null)
            {
                Vector2 jitter = Random.insideUnitCircle * (mag * (1f - elapsed / duration));
                wallVisual.transform.localPosition = visualBaseLocalPos + new Vector3(jitter.x, jitter.y, 0f);
            }

            yield return null;
        }

        RestoreVisualColors();
        if (wallVisual != null) wallVisual.transform.localPosition = visualBaseLocalPos;
        hitFeedbackCoroutine = null;
    }

    private void CacheVisualIfNeeded()
    {
        if (wallVisual == null)
        {
            Transform visualChild = transform.Find("Visual");
            if (visualChild != null) wallVisual = visualChild.gameObject;
        }

        if (wallVisual != null && !hasCachedVisual)
        {
            visualBaseLocalPos = wallVisual.transform.localPosition;
            visualRenderers = wallVisual.GetComponentsInChildren<SpriteRenderer>(true);
            originalColors = new Color[visualRenderers.Length];
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                originalColors[i] = visualRenderers[i].color;
            }

            primarySpriteRenderer = wallVisual.GetComponentInChildren<SpriteRenderer>(true);
            if (primarySpriteRenderer != null) initialSprite = primarySpriteRenderer.sprite;

            hasCachedVisual = true;
        }
    }

    private void ApplyVisualTint(Color tintColor, float tintAmount)
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] == null) continue;
            Color c = Color.Lerp(originalColors[i], tintColor, tintAmount);
            c.a = originalColors[i].a;
            visualRenderers[i].color = c;
        }
    }

    private void RestoreVisualColors()
    {
        if (visualRenderers == null) return;

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            if (visualRenderers[i] != null)
            {
                visualRenderers[i].color = originalColors[i];
            }
        }
    }

    private void TriggerCameraShake(float duration, float magnitude)
    {
        if (GameFeelManager.Instance != null && Camera.main != null && duration > 0f)
        {
            GameFeelManager.Instance.TriggerShake(duration, magnitude, Camera.main.transform);
        }
    }

    [ContextMenu("Generate Wall ID")]
    private void GenerateWallID()
    {
        string sceneName = gameObject.scene.name;
        string shortGuid = System.Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        wallID = $"{sceneName}_Wall_{shortGuid}";
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }

    private void OnDrawGizmos()
    {
        Vector3 center = (solidCollider != null) ? solidCollider.bounds.center : transform.position;

        if (breakableSide == BreakableSide.Both)
        {
            Gizmos.color = new Color(0.15f, 1f, 0.3f, 0.85f);
            Gizmos.DrawSphere(center + new Vector3(-0.65f, 0f, 0f), 0.16f);
            Gizmos.DrawSphere(center + new Vector3(0.65f, 0f, 0f), 0.16f);
            return;
        }

        float dir = (breakableSide == BreakableSide.Left) ? -1f : 1f;

        Gizmos.color = new Color(0.15f, 1f, 0.3f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(dir * 0.65f, 0f, 0f), 0.16f);

        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.85f);
        Gizmos.DrawSphere(center + new Vector3(-dir * 0.65f, 0f, 0f), 0.16f);
    }
}