using UnityEngine;
using System.Collections;

public class BouncyDeathFX : MonoBehaviour
{
    [Header("Squash & Stretch Settings")]
    [Tooltip("Total duration from the start of the bounce until it disappears (in seconds).")]
    public float duration = 0.5f;

    [Tooltip("X-axis scale curve: Recommended to scale up (>1) then shrink down to 0.")]
    public AnimationCurve scaleXCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.3f, 1.5f),
        new Keyframe(0.7f, 0.2f),
        new Keyframe(1f, 0f)
    );

    [Tooltip("Y-axis scale curve: Set inverse to X to maintain volume (Squash & Stretch).")]
    public AnimationCurve scaleYCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.3f, 0.5f),
        new Keyframe(0.7f, 1.8f),
        new Keyframe(1f, 0f)
    );

    [Header("End Action")]
    [Tooltip("Check to Destroy the GameObject. Uncheck to use SetActive(false) for object pooling or respawning.")]
    public bool destroyOnEnd = false;

    private void Awake()
    {
        if (GetComponentInParent<Enemy>() != null)
        {
            destroyOnEnd = false;
        }
    }

    public void PlayDeathEffect()
    {
        StartCoroutine(BouncyRoutine());
    }

    private IEnumerator BouncyRoutine()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float currentX = startScale.x * scaleXCurve.Evaluate(t);
            float currentY = startScale.y * scaleYCurve.Evaluate(t);

            transform.localScale = new Vector3(currentX, currentY, startScale.z);

            yield return null;
        }

        transform.localScale = startScale;

        if (destroyOnEnd)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}