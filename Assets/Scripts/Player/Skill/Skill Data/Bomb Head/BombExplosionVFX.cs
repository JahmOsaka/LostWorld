using System.Collections;
using UnityEngine;

public class BombExplosionVFX : MonoBehaviour
{
    [Header("Shockwave Ring")]
    [Tooltip("Optional material for the expanding shockwave ring (falls back to Sprites/Default).")]
    [SerializeField] private Material shockwaveMaterial;
    [Tooltip("Initial color of the shockwave ring.")]
    [SerializeField] private Color shockwaveStartColor = new Color(1f, 0.85f, 0.4f, 1f);
    [Tooltip("Final color of the shockwave ring as it fades out.")]
    [SerializeField] private Color shockwaveEndColor = new Color(1f, 0.25f, 0.1f, 0f);
    [Tooltip("Starting line thickness of the shockwave ring.")]
    [SerializeField] private float startWidth = 0.28f;
    [Tooltip("Duration in seconds for the shockwave ring to expand and fade.")]
    [SerializeField] private float shockwaveDuration = 0.24f;
    [Tooltip("Number of segments for the shockwave circle.")]
    [SerializeField] private int segments = 48;
    [Tooltip("Sorting order for the shockwave ring.")]
    [SerializeField] private int sortingOrder = 15;

    [Header("Impact Flash Core (Optional)")]
    [Tooltip("Optional child SpriteRenderer (white circle) that flashes briefly on impact.")]
    [SerializeField] private SpriteRenderer flashCoreRenderer;
    [Tooltip("Duration in seconds for the bright center flash.")]
    [SerializeField] private float flashDuration = 0.08f;

    private LineRenderer shockwaveRing;

    public void Initialize(float explosionRadius)
    {
        Material mat = shockwaveMaterial != null ? shockwaveMaterial : new Material(Shader.Find("Sprites/Default"));

        GameObject ringObj = new GameObject("ShockwaveRing");
        ringObj.transform.SetParent(transform, false);

        shockwaveRing = ringObj.AddComponent<LineRenderer>();
        shockwaveRing.useWorldSpace = true;
        shockwaveRing.loop = true;
        shockwaveRing.positionCount = segments;
        shockwaveRing.material = mat;
        shockwaveRing.sortingOrder = sortingOrder;

        StartCoroutine(AnimateShockwaveRoutine(explosionRadius));

        if (flashCoreRenderer != null)
        {
            StartCoroutine(AnimateFlashCoreRoutine(explosionRadius));
        }
    }

    private IEnumerator AnimateShockwaveRoutine(float targetRadius)
    {
        float elapsed = 0f;
        while (elapsed < shockwaveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shockwaveDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            float currentRadius = Mathf.Lerp(0.15f, targetRadius * 1.05f, ease);
            float currentWidth = Mathf.Lerp(startWidth, 0f, t);
            Color currentColor = Color.Lerp(shockwaveStartColor, shockwaveEndColor, t);

            shockwaveRing.startWidth = currentWidth;
            shockwaveRing.endWidth = currentWidth;
            shockwaveRing.startColor = currentColor;
            shockwaveRing.endColor = currentColor;

            DrawCircle(shockwaveRing, transform.position, currentRadius);
            yield return null;
        }

        if (shockwaveRing != null)
        {
            shockwaveRing.enabled = false;
        }
    }

    private IEnumerator AnimateFlashCoreRoutine(float targetRadius)
    {
        flashCoreRenderer.enabled = true;
        Color baseColor = flashCoreRenderer.color;
        float diameter = targetRadius * 1.4f;
        flashCoreRenderer.transform.localScale = new Vector3(diameter, diameter, 1f);

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            Color c = baseColor;
            c.a = Mathf.Lerp(0.85f, 0f, t);
            flashCoreRenderer.color = c;
            yield return null;
        }

        flashCoreRenderer.enabled = false;
    }

    private void DrawCircle(LineRenderer lr, Vector3 center, float radius)
    {
        float step = (Mathf.PI * 2f) / segments;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * step;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            lr.SetPosition(i, pos);
        }
    }
}