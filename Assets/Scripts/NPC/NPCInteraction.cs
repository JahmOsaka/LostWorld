using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Dialogue Info")]
    [SerializeField] private string npcName = "Mushroom Girl";
    [TextArea(2, 5)]
    [SerializeField] private string dialogueSentence = "";

    [Header("Portraits")]
    [SerializeField] private Sprite playerSprite;
    [SerializeField] private Sprite npcSprite;

    private PlayerInputHandler playerInput;
    private bool playerIsClose = false;
    private bool isTalking = false;

    private void Update()
    {
        if (playerIsClose && playerInput != null && playerInput.InteractInput)
        {
            playerInput.UseInteractInput();

            if (!isTalking)
            {
                DialogueManager.Instance.StartDialogue(npcName, dialogueSentence, playerSprite, npcSprite);
                isTalking = true;
            }
            else
            {
                DialogueManager.Instance.EndDialogue();
                isTalking = false;
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

            if (isTalking)
            {
                DialogueManager.Instance.EndDialogue();
                isTalking = false;
            }
        }
    }
}