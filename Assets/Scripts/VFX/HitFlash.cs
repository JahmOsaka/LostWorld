using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    [Tooltip("Target SpriteRenderer to flash. Auto-detected in children if left empty.")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [Tooltip("Assign a solid white unlit material for full-silhouette flash, or leave empty to tint with Flash Color instead.")]
    [SerializeField] private Material flashMaterial;
    [Tooltip("Fallback tint color used when Flash Material is unassigned.")]
    [SerializeField] private Color flashColor = Color.red;
    [Tooltip("Default flash duration in real-time seconds when triggered without a custom duration.")]
    [SerializeField] private float defaultFlashDuration = 0.08f;

    private Material materialBeforeFlash;
    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    public void TriggerFlash() => TriggerFlash(defaultFlashDuration);

    public void TriggerFlash(float customDuration)
    {
        if (spriteRenderer == null || !gameObject.activeInHierarchy) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            RestoreVisual();
        }

        flashCoroutine = StartCoroutine(FlashRoutine(customDuration));
    }

    private IEnumerator FlashRoutine(float duration)
    {
        if (flashMaterial != null)
        {
            materialBeforeFlash = spriteRenderer.sharedMaterial;
            spriteRenderer.material = flashMaterial;
        }
        else
        {
            spriteRenderer.color = flashColor;
        }

        yield return new WaitForSecondsRealtime(duration);

        RestoreVisual();
        flashCoroutine = null;
    }

    private void RestoreVisual()
    {
        if (spriteRenderer == null) return;

        if (flashMaterial != null && materialBeforeFlash != null)
        {
            spriteRenderer.material = materialBeforeFlash;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            RestoreVisual();
            flashCoroutine = null;
        }
    }
}