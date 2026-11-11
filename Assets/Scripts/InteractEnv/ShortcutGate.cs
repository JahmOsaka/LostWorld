using UnityEngine;

public class ShortcutGate : MonoBehaviour, IInteractable
{
    [Header("Save System")]
    [Tooltip("ตั้งชื่อ ID ให้ไม่ซ้ำกัน เช่น gate_forest_01, rock_cave_02")]
    public string gateID;

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

        if (gateVisual != null)
        {
            gateVisual.SetActive(false);
        }

        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }
    }

    public void ResetToLocked()
    {
        isUnlocked = false;

        if (gateVisual == null)
        {
            Transform visualChild = transform.Find("Visual");
            if (visualChild != null) gateVisual = visualChild.gameObject;
        }

        if (gateVisual != null)
        {
            gateVisual.SetActive(true);
        }

        if (solidCollider != null)
        {
            solidCollider.enabled = true;
        }
    }
}