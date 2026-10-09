using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerInteract : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Drag and drop the PlayerInputHandler component here to link the interaction input.")]
    public PlayerInputHandler inputHandler;

    private IInteractable currentInteractable;

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