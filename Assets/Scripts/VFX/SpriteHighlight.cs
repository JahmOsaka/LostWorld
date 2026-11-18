using System.Collections;
using UnityEngine;

public class SpriteHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("The target SpriteRenderer to apply the highlight to.")]
    public SpriteRenderer targetSprite;

    [Tooltip("The default material of the sprite (usually Sprite-Lit-Default).")]
    public Material defaultMaterial;

    [Tooltip("The material with the white outline shader applied.")]
    public Material outlineMaterial;

    [Header("Smooth Blend Settings")]
    [Tooltip("Duration in seconds to smoothly fade the outline in and out.")]
    public float fadeDuration = 0.2f;
    [Tooltip("Enable a gentle breathing glow effect while highlighted.")]
    public bool enableBreathingGlow = true;
    [Tooltip("Name of the Color or Thickness property in InteractOutline.shadergraph (e.g., _OutlineColor or _Color).")]
    public string shaderColorProperty = "_OutlineColor";

    private Coroutine blendCoroutine;
    private MaterialPropertyBlock propBlock;
    private Color baseOutlineColor = Color.white;
    private int colorPropID;

    private void Reset()
    {
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
        if (targetSprite != null && defaultMaterial == null) defaultMaterial = targetSprite.sharedMaterial;
    }

    private void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        colorPropID = Shader.PropertyToID(shaderColorProperty);

        if (outlineMaterial != null && outlineMaterial.HasProperty(colorPropID))
        {
            baseOutlineColor = outlineMaterial.GetColor(colorPropID);
        }
    }

    public void TurnOnHighlight()
    {
        if (targetSprite == null || outlineMaterial == null) return;

        if (!gameObject.activeInHierarchy)
        {
            targetSprite.material = outlineMaterial;
            return;
        }

        if (blendCoroutine != null) StopCoroutine(blendCoroutine);
        blendCoroutine = StartCoroutine(BlendHighlightRoutine(true));
    }

    public void TurnOffHighlight()
    {
        if (targetSprite == null || defaultMaterial == null) return;

        if (!gameObject.activeInHierarchy)
        {
            targetSprite.material = defaultMaterial;
            return;
        }

        if (blendCoroutine != null) StopCoroutine(blendCoroutine);
        blendCoroutine = StartCoroutine(BlendHighlightRoutine(false));
    }

    private IEnumerator BlendHighlightRoutine(bool turningOn)
    {
        bool hasColorProp = outlineMaterial.HasProperty(colorPropID);

        if (turningOn)
        {
            targetSprite.material = outlineMaterial;
        }

        if (hasColorProp)
        {
            float elapsed = 0f;
            float startAlpha = turningOn ? 0f : baseOutlineColor.a;
            float endAlpha = turningOn ? baseOutlineColor.a : 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, Mathf.SmoothStep(0f, 1f, t));

                ApplyOutlineAlpha(currentAlpha);
                yield return null;
            }

            ApplyOutlineAlpha(endAlpha);

            while (turningOn && enableBreathingGlow)
            {
                float breath = (Mathf.Sin(Time.unscaledTime * 4f) + 1f) * 0.5f;
                float pulsedAlpha = baseOutlineColor.a * Mathf.Lerp(0.45f, 0.95f, breath);
                ApplyOutlineAlpha(pulsedAlpha);
                yield return null;
            }
        }

        if (!turningOn)
        {
            targetSprite.SetPropertyBlock(null);
            targetSprite.material = defaultMaterial;
        }
    }

    private void ApplyOutlineAlpha(float alpha)
    {
        targetSprite.GetPropertyBlock(propBlock);
        Color c = baseOutlineColor;
        c.a = alpha;
        propBlock.SetColor(colorPropID, c);
        targetSprite.SetPropertyBlock(propBlock);
    }
}