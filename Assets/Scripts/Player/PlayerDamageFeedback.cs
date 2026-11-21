using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageFeedback : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The child transform holding the player's sprite visual.")]
    [SerializeField] private Transform visualTransform;
    [Tooltip("The SpriteRenderer used for alpha blinking during invincibility frames.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Full-screen UI Image used for the red vignette damage flash.")]
    [SerializeField] private RawImage redVignetteImage;
    [Tooltip("Optional 360-degree particle prefab spawned when taking damage.")]
    [SerializeField] private GameObject hurtBurstPrefab;

    [Header("1. Heavy Hitstop & Screen Shake")]
    [Tooltip("Duration of the hitstop freeze when the player takes damage.")]
    [SerializeField] private float hurtHitstopDuration = 0.13f;
    [Tooltip("Duration of the camera shake when the player takes damage.")]
    [SerializeField] private float hurtShakeDuration = 0.22f;
    [Tooltip("Intensity of the camera shake when the player takes damage.")]
    [SerializeField] private float hurtShakeMagnitude = 0.3f;

    [Header("2. Procedural Hurt Pose (Tilt + Squash & Stretch)")]
    [Tooltip("Backward tilt angle in degrees applied on impact.")]
    [SerializeField] private float tiltAngle = 14f;
    [Tooltip("Squash and stretch multiplier applied on impact before recovering.")]
    [SerializeField] private Vector2 squashScale = new Vector2(0.75f, 1.25f);
    [Tooltip("Duration in seconds to recover from the hurt pose back to normal.")]
    [SerializeField] private float poseRecoveryDuration = 0.16f;

    [Header("3. I-Frames Alpha Blink")]
    [Tooltip("Match this value with Invincibility Duration on the player's CoreHealth.")]
    [SerializeField] private float iFrameBlinkDuration = 0.8f;
    [Tooltip("Time interval between each alpha blink toggle.")]
    [SerializeField] private float blinkInterval = 0.07f;
    [Tooltip("Minimum sprite alpha while blinking during invincibility frames.")]
    [SerializeField] private float blinkMinAlpha = 0.3f;

    [Header("4. Red Vignette Flash")]
    [Tooltip("Peak alpha of the red vignette overlay immediately on hit.")]
    [SerializeField] private float vignetteMaxAlpha = 0.45f;
    [Tooltip("Duration in seconds for the red vignette overlay to fade out.")]
    [SerializeField] private float vignetteFadeDuration = 0.3f;

    private Player player;
    private CoreHealth coreHealth;
    private Vector3 baseAbsScale = Vector3.one;

    private Coroutine poseCoroutine;
    private Coroutine blinkCoroutine;
    private Coroutine vignetteCoroutine;

    private void Awake()
    {
        player = GetComponent<Player>();
        coreHealth = GetComponent<CoreHealth>();

        if (redVignetteImage != null && redVignetteImage.texture == null)
        {
            redVignetteImage.texture = CreateVignetteTexture(128, 0.35f, 0.75f);
        }

        if (visualTransform == null)
        {
            visualTransform = transform.Find("PlayerVisual") ?? transform.Find("Visual");
        }

        if (spriteRenderer == null && visualTransform != null)
        {
            spriteRenderer = visualTransform.GetComponent<SpriteRenderer>();
        }

        if (visualTransform != null)
        {
            Vector3 s = visualTransform.localScale;
            baseAbsScale = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), s.z);
        }

        SetVignetteAlpha(0f);
    }

    public void TriggerDamageFeedback()
    {
        if (!gameObject.activeInHierarchy) return;

        bool isLethalHit = coreHealth != null && coreHealth.currentHealth <= 0;

        if (GameFeelManager.Instance != null)
        {
            if (hurtHitstopDuration > 0f)
            {
                GameFeelManager.Instance.TriggerHitstop(hurtHitstopDuration);
            }

            if (Camera.main != null)
            {
                GameFeelManager.Instance.TriggerShake(hurtShakeDuration, hurtShakeMagnitude, Camera.main.transform);
            }
        }

        if (redVignetteImage != null)
        {
            if (vignetteCoroutine != null) StopCoroutine(vignetteCoroutine);
            vignetteCoroutine = StartCoroutine(VignetteRoutine());
        }

        if (hurtBurstPrefab != null)
        {
            Vector3 spawnPos = visualTransform != null ? visualTransform.position : transform.position;
            GameObject fx = Instantiate(hurtBurstPrefab, spawnPos, Quaternion.identity);
            Destroy(fx, 0.5f);
        }
        if (isLethalHit) return;

        if (visualTransform != null)
        {
            if (poseCoroutine != null) StopCoroutine(poseCoroutine);
            poseCoroutine = StartCoroutine(HurtPoseRoutine());
        }

        if (spriteRenderer != null && iFrameBlinkDuration > 0f)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    private IEnumerator HurtPoseRoutine()
    {
        float facing = (player != null) ? player.FacingDirection : 1f;
        float startZ = facing * tiltAngle;

        ApplyVisualPose(startZ, squashScale.x, squashScale.y);

        float elapsed = 0f;
        while (elapsed < poseRecoveryDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / poseRecoveryDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            float currentZ = Mathf.Lerp(startZ, 0f, ease);
            float multX = Mathf.Lerp(squashScale.x, 1f, ease);
            float multY = Mathf.Lerp(squashScale.y, 1f, ease);

            ApplyVisualPose(currentZ, multX, multY);
            yield return null;
        }

        ApplyVisualPose(0f, 1f, 1f);
        poseCoroutine = null;
    }

    private void ApplyVisualPose(float zRotation, float multX, float multY)
    {
        if (visualTransform == null) return;

        float signX = visualTransform.localScale.x < 0f ? -1f : 1f;
        visualTransform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
        visualTransform.localScale = new Vector3(
            baseAbsScale.x * multX * signX,
            baseAbsScale.y * multY,
            baseAbsScale.z
        );
    }

    private IEnumerator BlinkRoutine()
    {
        if (hurtHitstopDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(hurtHitstopDuration);
        }

        float elapsed = 0f;
        bool isFaded = false;

        while (elapsed < iFrameBlinkDuration)
        {
            isFaded = !isFaded;
            SetSpriteAlpha(isFaded ? blinkMinAlpha : 1f);

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetSpriteAlpha(1f);
        blinkCoroutine = null;
    }

    private IEnumerator VignetteRoutine()
    {
        SetVignetteAlpha(vignetteMaxAlpha);

        float elapsed = 0f;
        while (elapsed < vignetteFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / vignetteFadeDuration);
            SetVignetteAlpha(Mathf.Lerp(vignetteMaxAlpha, 0f, t));
            yield return null;
        }

        SetVignetteAlpha(0f);
        vignetteCoroutine = null;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer == null) return;
        Color c = spriteRenderer.color;
        c.a = alpha;
        spriteRenderer.color = c;
    }

    private void SetVignetteAlpha(float alpha)
    {
        if (redVignetteImage == null) return;
        Color c = redVignetteImage.color;
        c.a = alpha;
        redVignetteImage.color = c;
    }

    private Texture2D CreateVignetteTexture(int size, float innerRadius, float outerRadius)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float v = (y + 0.5f) / size;
                float dist = Vector2.Distance(new Vector2(u, v), new Vector2(0.5f, 0.5f));
                float t = Mathf.Clamp01((dist - innerRadius) / (outerRadius - innerRadius));
                float alpha = t * t * (3f - 2f * t); // Smoothstep

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        return tex;
    }

    private void OnDisable()
    {
        ApplyVisualPose(0f, 1f, 1f);
        SetSpriteAlpha(1f);
        SetVignetteAlpha(0f);
        poseCoroutine = null;
        blinkCoroutine = null;
        vignetteCoroutine = null;
    }
}