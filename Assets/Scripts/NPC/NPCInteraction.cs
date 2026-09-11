using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue Sets")]
    [Tooltip("Default dialogues to play when the condition is not met.")]
    [SerializeField] private DialogueLine[] defaultDialogues;

    [Tooltip("Alternate dialogues to play once the condition is met.")]
    [SerializeField] private DialogueLine[] alternateDialogues;

    [Header("Portraits")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite npcSprite;

    [Header("Conditions")]
    [Tooltip("Check this to simulate that the condition is met for testing purposes.")]
    public bool isConditionMet = false;

    private PlayerInputHandler playerInput;
    private bool playerIsClose = false;

    private void Update()
    {
        if (playerIsClose && playerInput != null && playerInput.InteractInput)
        {
            playerInput.UseInteractInput();

            if (!DialogueManager.Instance.isOpen)
            {
                DialogueLine[] dialogueToPlay;

                if (isConditionMet)
                {
                    dialogueToPlay = alternateDialogues;
                }
                else
                {
                    dialogueToPlay = defaultDialogues;
                }

                DialogueManager.Instance.StartDialogue(dialogueToPlay, playerSprite, npcSprite);
            }
            else
            {
                DialogueManager.Instance.DisplayNextSentence();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = true;
            playerInput = other.GetComponent<PlayerInputHandler>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsClose = false;
            playerInput = null;

            if (DialogueManager.Instance.isOpen)
            {
                DialogueManager.Instance.EndDialogue();
            }
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
        Debug.Log(gameObject.name + ": Condition Met! Ready for new dialogue.");
    }
}