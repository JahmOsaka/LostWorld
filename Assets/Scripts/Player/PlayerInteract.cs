using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerInteract : MonoBehaviour
{
    private PlayerInputHandler inputHandler;
    private IInteractable currentInteractable;
    private BoxCollider2D myCollider;

    private void Awake()
    {
        myCollider = GetComponent<BoxCollider2D>();
        inputHandler = GetComponentInParent<PlayerInputHandler>();
    }

    private void Update()
    {
        if (currentInteractable != null && inputHandler != null && inputHandler.InteractInput)
        {
            currentInteractable.Interact();
            inputHandler.UseInteractInput();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (myCollider != null && !myCollider.IsTouching(collision)) return;

        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentInteractable = interactable;
            currentInteractable.ShowInteractPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = collision.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentInteractable)
        {
            currentInteractable.HideInteractPrompt();
            currentInteractable = null;

            if (DialogueManager.Instance != null && DialogueManager.Instance.isOpen)
            {
                DialogueManager.Instance.EndDialogue();
            }
        }
    }
}