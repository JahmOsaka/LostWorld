using UnityEngine;

public class ShortcutGate : MonoBehaviour, IInteractable
{
    [Header("Save System")]
    public string gateID;

    public GameObject interactPrompt;

    public SpriteHighlight highlightEffect;

    public GameObject gateVisual;
    public Collider2D solidCollider;

    public bool isUnlocked = false;

    private void Awake()
    {

    }

    public void ShowInteractPrompt()
    {
        if (interactPrompt != null && !isUnlocked) interactPrompt.SetActive(true);
        if (highlightEffect != null && !isUnlocked) highlightEffect.TurnOnHighlight();
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (highlightEffect != null) highlightEffect.TurnOffHighlight();
    }

    public void Interact()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        HideInteractPrompt();

        if (gateVisual != null) gateVisual.SetActive(false);
        if (solidCollider != null) solidCollider.enabled = false;
    }

    public void ResetToLocked()
    {
        isUnlocked = false;

        if (gateVisual == null)
        {
            Transform visualChild = transform.Find("Visual");
            if (visualChild != null) gateVisual = visualChild.gameObject;
        }

        if (gateVisual != null) gateVisual.SetActive(true);
        if (solidCollider != null) solidCollider.enabled = true;
    }
}