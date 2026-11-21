using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractPromptUI : MonoBehaviour
{
    public static InteractPromptUI Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("Root RectTransform of the prompt panel (with Image + LayoutGroup + ContentSizeFitter).")]
    [SerializeField] private RectTransform promptPanel;
    [Tooltip("CanvasGroup on the panel for smooth alpha fade.")]
    [SerializeField] private CanvasGroup canvasGroup;
    [Tooltip("Optional text showing the key binding (e.g., F).")]
    [SerializeField] private TMP_Text keyText;
    [Tooltip("Text showing the action label (e.g., Up, Down, Call Elevator).")]
    [SerializeField] private TMP_Text actionText;

    [Header("Position & Animation")]
    [Tooltip("Default world-space offset above the target object.")]
    [SerializeField] private Vector3 defaultWorldOffset = new Vector3(0f, 1.5f, 0f);
    [Tooltip("Pop-in/out speed for alpha and scale.")]
    [SerializeField] private float animSpeed = 14f;
    [Tooltip("Starting scale when hidden (gives a subtle pop-in feel).")]
    [SerializeField] private float hiddenScale = 0.85f;

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private object currentRequester;
    private Transform followTarget;
    private Vector3 currentOffset;
    private bool isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (promptPanel == null) promptPanel = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = promptPanel.GetComponent<CanvasGroup>();

        parentCanvas = promptPanel.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        promptPanel.localScale = Vector3.one * hiddenScale;
    }

    private void LateUpdate()
    {
        if (promptPanel == null) return;

        if (isVisible && followTarget == null)
        {
            isVisible = false;
            currentRequester = null;
        }

        if (followTarget != null)
        {
            UpdateScreenPosition(followTarget.position + currentOffset);
        }

        float targetAlpha = isVisible ? 1f : 0f;
        float targetScale = isVisible ? 1f : hiddenScale;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.unscaledDeltaTime * animSpeed);
        }

        float currentS = Mathf.Lerp(promptPanel.localScale.x, targetScale, Time.unscaledDeltaTime * animSpeed);
        promptPanel.localScale = Vector3.one * currentS;
    }

    public void Show(object requester, Transform target, string actionLabel, string keyLabel = "F", Vector3? customOffset = null)
    {
        currentRequester = requester;
        followTarget = target;
        currentOffset = customOffset ?? defaultWorldOffset;
        isVisible = true;

        bool textChanged = false;
        if (keyText != null && keyText.text != keyLabel)
        {
            keyText.text = keyLabel;
            textChanged = true;
        }

        if (actionText != null && actionText.text != actionLabel)
        {
            actionText.text = actionLabel;
            textChanged = true;
        }

        if (textChanged && promptPanel != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(promptPanel);
        }

        if (followTarget != null)
        {
            UpdateScreenPosition(followTarget.position + currentOffset);
        }
    }

    public void Hide(object requester)
    {
        if (currentRequester != requester) return;

        isVisible = false;
        currentRequester = null;
    }

    private void UpdateScreenPosition(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        if (cam == null || canvasRect == null) return;

        Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
        Camera uiCam = (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : parentCanvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCam, out Vector2 localPoint))
        {
            promptPanel.anchoredPosition = localPoint;
        }
    }
}