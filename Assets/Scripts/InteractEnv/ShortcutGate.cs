using UnityEngine;

public class ShortcutGate : MonoBehaviour, IInteractable
{
    public GameObject interactPrompt;
    public SpriteHighlight highlightEffect;
    public GameObject gateVisual;
    public Collider2D solidCollider;

    public bool isUnlocked = false;

    public void ShowInteractPrompt()
    {
        if (!isUnlocked && interactPrompt != null) 
        {
            interactPrompt.SetActive(true); 
        } 
        if (!isUnlocked && highlightEffect != null)
        {
            highlightEffect.TurnOnHighlight();
        }
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt != null)
        { 
            interactPrompt.SetActive(false); 
        }
        if (highlightEffect != null)
        {
            highlightEffect.TurnOffHighlight();
        }
    }

    public void Interact()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        HideInteractPrompt();

        if (gateVisual != null) gateVisual.SetActive(false);
        if (solidCollider != null) solidCollider.enabled = false;
    }
}