using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerInteract : MonoBehaviour
{
    private readonly HashSet<IInteractable> activeInteractables = new HashSet<IInteractable>();
    private BoxCollider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
    }

    private void OnDisable()
    {
        foreach (IInteractable interactable in activeInteractables)
        {
            if (interactable is UnityEngine.Object unityObj && unityObj == null) continue;
            interactable.HideInteractPrompt();
        }
        activeInteractables.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (myCollider != null && !myCollider.IsTouching(collision)) return;

        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && activeInteractables.Add(interactable))
        {
            interactable.ShowInteractPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && activeInteractables.Remove(interactable))
        {
            interactable.HideInteractPrompt();
        }
    }
}