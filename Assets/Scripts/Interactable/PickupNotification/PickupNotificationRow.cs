using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PickupNotificationRow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private Image accentBar;
    [SerializeField] private GameObject iconContainer;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text messageText;

    [Header("Animation")]
    [SerializeField] private float slideDistance = 40f;
    [SerializeField] private float enterDuration = 0.22f;
    [SerializeField] private float exitDuration = 0.25f;

    private Coroutine lifecycleCoroutine;
    private Action<PickupNotificationRow> onDismissed;
    private bool isExiting;

    public void Setup(
        string message,
        Sprite icon,
        Color accentColor,
        float holdDuration,
        Action<PickupNotificationRow> onComplete)
    {
        onDismissed = onComplete;

        if (messageText != null) messageText.text = message;
        if (accentBar != null) accentBar.color = accentColor;

        if (iconContainer != null) iconContainer.SetActive(icon != null);
        if (iconImage != null && icon != null) iconImage.sprite = icon;

        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (lifecycleCoroutine != null) StopCoroutine(lifecycleCoroutine);
        lifecycleCoroutine = StartCoroutine(LifecycleRoutine(holdDuration));
    }

    public void ForceDismiss()
    {
        if (isExiting) return;

        if (lifecycleCoroutine != null) StopCoroutine(lifecycleCoroutine);
        lifecycleCoroutine = StartCoroutine(ExitRoutine());
    }

    private IEnumerator LifecycleRoutine(float holdDuration)
    {
        yield return EnterRoutine();
        yield return new WaitForSeconds(holdDuration);
        yield return ExitRoutine();
    }

    private IEnumerator EnterRoutine()
    {
        float elapsed = 0f;
        while (elapsed < enterDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, enterDuration));
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            if (canvasGroup != null) canvasGroup.alpha = eased;
            if (contentRoot != null)
            {
                Vector2 pos = contentRoot.anchoredPosition;
                pos.x = Mathf.Lerp(-slideDistance, 0f, eased);
                contentRoot.anchoredPosition = pos;
            }
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        if (contentRoot != null)
        {
            Vector2 pos = contentRoot.anchoredPosition;
            pos.x = 0f;
            contentRoot.anchoredPosition = pos;
        }
    }

    private IEnumerator ExitRoutine()
    {
        isExiting = true;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float elapsed = 0f;

        while (elapsed < exitDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, exitDuration));

            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            if (contentRoot != null)
            {
                Vector2 pos = contentRoot.anchoredPosition;
                pos.x = Mathf.Lerp(0f, -slideDistance * 0.5f, t);
                contentRoot.anchoredPosition = pos;
            }
            yield return null;
        }

        onDismissed?.Invoke(this);
        Destroy(gameObject);
    }
}