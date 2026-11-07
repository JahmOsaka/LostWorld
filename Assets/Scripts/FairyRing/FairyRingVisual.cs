using UnityEngine;

[RequireComponent(typeof(FairyRingActivation))]
public class FairyRingVisual : MonoBehaviour
{
    [Header("Visuals & UI")]
    public GameObject interactPromptUI;
    public GameObject activatedVFX;
    public SpriteHighlight highlightEffect;

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
        activation.OnPlayerProximityChanged += HandleProximity;
        activation.OnActivated += HandleActivation;
    }

    private void OnDisable()
    {
        activation.OnPlayerProximityChanged -= HandleProximity;
        activation.OnActivated -= HandleActivation;
    }

    private void HandleProximity(bool inRange)
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(inRange);

        if (highlightEffect != null)
        {
            if (inRange) highlightEffect.TurnOnHighlight();
            else highlightEffect.TurnOffHighlight();
        }
    }

    private void HandleActivation()
    {
        if (activatedVFX != null) activatedVFX.SetActive(true);
    }
}