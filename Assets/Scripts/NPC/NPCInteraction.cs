using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Prompt & Highlight")]
    public GameObject interactPrompt;

    [Tooltip("The highlight effect script attached to this object.")]
    public SpriteHighlight highlightEffect;

    [Header("Dialogue Sets")]
    [SerializeField] private DialogueLine[] defaultDialogues;
    [SerializeField] private DialogueLine[] alternateDialogues;

    [Header("Portraits")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite npcSprite;

    [Header("Conditions")]
    public bool isConditionMet = false;

    public void ShowInteractPrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(true);
        if (highlightEffect != null) highlightEffect.TurnOnHighlight();
    }

    public void HideInteractPrompt()
    {
        if (interactPrompt != null) interactPrompt.SetActive(false);
        if (highlightEffect != null) highlightEffect.TurnOffHighlight();
    }

    public void Interact()
    {
        if (!DialogueManager.Instance.isOpen)
        {
            DialogueLine[] dialogueToPlay = isConditionMet ? alternateDialogues : defaultDialogues;
            DialogueManager.Instance.StartDialogue(dialogueToPlay, playerSprite, npcSprite);
            HideInteractPrompt();
        }
        else
        {
            DialogueManager.Instance.DisplayNextSentence();
        }
    }

    private void OnEnable()
    {
        ItemPickup.OnItemCollected += CompleteQuestCondition;
    }

    private void OnDisable()
    {
        ItemPickup.OnItemCollected -= CompleteQuestCondition;
    }

    private void CompleteQuestCondition()
    {
        isConditionMet = true;
    }
}