using UnityEngine;

public class NPCInteraction : MonoBehaviour, IInteractable
{
    [Header("Interact HUD Prompt")]
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string talkLabel = "Talk";

    [Tooltip("The highlight effect script attached to this object.")]
    public SpriteHighlight highlightEffect;

    [Header("Dialogue Sets")]
    [SerializeField] private DialogueData defaultDialogue;
    [SerializeField] private DialogueData alternateDialogue;

    [Header("Conditions")]
    public bool isConditionMet = false;

    private void OnEnable()
    {
        ItemPickup.OnItemCollected += CompleteQuestCondition;
    }

    private void OnDisable()
    {
        ItemPickup.OnItemCollected -= CompleteQuestCondition;
        HideInteractPrompt();
    }

    public void ShowInteractPrompt()
    {
        InteractPromptUI.Instance?.Register(this, talkLabel, Interact, SetHighlight, promptKey);
    }

    public void HideInteractPrompt()
    {
        SetHighlight(false);
        InteractPromptUI.Instance?.Unregister(this);
    }

    public void Interact()
    {
        if (DialogueUI.Instance == null || DialogueUI.Instance.IsBlockingInteract) return;

        DialogueData dialogueToPlay = isConditionMet && alternateDialogue != null
            ? alternateDialogue
            : defaultDialogue;

        if (dialogueToPlay != null)
        {
            DialogueUI.Instance.PlayDialogue(dialogueToPlay);
        }
    }

    private void SetHighlight(bool active)
    {
        if (highlightEffect == null) return;
        if (active) highlightEffect.TurnOnHighlight();
        else highlightEffect.TurnOffHighlight();
    }

    private void CompleteQuestCondition()
    {
        isConditionMet = true;
    }
}