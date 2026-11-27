using UnityEngine;

[RequireComponent(typeof(FairyRingActivation))]
public class FairyRingVisual : MonoBehaviour
{
    [Header("Visuals & VFX")]
    public GameObject activatedVFX;
    public SpriteHighlight highlightEffect;

    [Header("Legacy (Optional - Can be removed from Prefab)")]
    [SerializeField] private GameObject interactPromptUI;

    private FairyRingActivation activation;

    private void Awake()
    {
        activation = GetComponent<FairyRingActivation>();
    }

    private void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (activatedVFX != null) activatedVFX.SetActive(activation.isActivated);
    }

    private void OnEnable()
    {
        activation.OnFocusChanged += HandleFocusChanged;
        activation.OnActivated += HandleActivation;
    }

    private void OnDisable()
    {
        activation.OnFocusChanged -= HandleFocusChanged;
        activation.OnActivated -= HandleActivation;
    }

    private void HandleFocusChanged(bool isFocused)
    {
        if (highlightEffect != null)
        {
            if (isFocused) highlightEffect.TurnOnHighlight();
            else highlightEffect.TurnOffHighlight();
        }
    }

    private void HandleActivation()
    {
        if (activatedVFX != null) activatedVFX.SetActive(true);
    }
}