using System.Collections;
using UnityEngine;

public class HitJiggle : MonoBehaviour
{
    [Header("Jiggle Settings")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float intensity = 0.12f;

    private Vector3 originalLocalPos;
    private Coroutine jiggleCoroutine;

    private void Awake()
    {
        if (visualTransform == null)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) visualTransform = sr.transform;
        }

        if (visualTransform != null)
        {
            originalLocalPos = visualTransform.localPosition;
        }
    }

    public void TriggerJiggle()
    {
        if (visualTransform == null || !gameObject.activeInHierarchy) return;

        if (jiggleCoroutine != null)
        {
            StopCoroutine(jiggleCoroutine);
            visualTransform.localPosition = originalLocalPos;
        }

        jiggleCoroutine = StartCoroutine(JiggleRoutine());
    }

    private IEnumerator JiggleRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float decay = 1f - Mathf.Clamp01(elapsed / duration);

            float offsetX = Random.Range(-1f, 1f) * intensity * decay;
            float offsetY = Random.Range(-0.5f, 0.5f) * intensity * decay;

            visualTransform.localPosition = originalLocalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }

        visualTransform.localPosition = originalLocalPos;
        jiggleCoroutine = null;
    }

    private void OnDisable()
    {
        if (visualTransform != null)
        {
            visualTransform.localPosition = originalLocalPos;
        }
        jiggleCoroutine = null;
    }
}