using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SkillPickup : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("The skill (Key or Combat) to unlock when interacting with this corpse/remnant.")]
    public SkillData skillToGive;

    [Header("Interact HUD Prompt")]
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string interactLabel = "Absorb Skill";

    [Header("Visuals & VFX")]
    public GameObject absorbVFX;
    [Tooltip("The highlight effect script attached to this object.")]
    public SpriteHighlight highlightEffect;

    private PlayerSkillController playerSkillController;
    private GameObject currentPlayer;
    private bool isPlayerInRange = false;
    private bool hasCollected = false;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 1.2f;
        }
    }

    private void OnDisable()
    {
        if (isPlayerInRange)
        {
            isPlayerInRange = false;
            currentPlayer = null;
            playerSkillController = null;
        }

        if (highlightEffect != null)
        {
            highlightEffect.TurnOffHighlight();
        }

        InteractPromptUI.Instance?.Unregister(this);
    }

    private void HandlePromptInteract()
    {
        if (!isPlayerInRange || hasCollected) return;
        CollectSkill();
    }

    private void HandleFocusChanged(bool isFocused)
    {
        if (highlightEffect == null) return;

        if (isFocused)
            highlightEffect.TurnOnHighlight();
        else
            highlightEffect.TurnOffHighlight();
    }

    private void CollectSkill()
    {
        if (playerSkillController != null && skillToGive != null)
        {
            hasCollected = true;
            InteractPromptUI.Instance?.Unregister(this);

            playerSkillController.UnlockSkill(skillToGive);

            if (absorbVFX != null)
            {
                Instantiate(absorbVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPlayerInRange && !hasCollected)
        {
            PlayerSkillController skillController = collision.GetComponentInParent<PlayerSkillController>();
            if (skillController != null)
            {
                currentPlayer = collision.transform.root.gameObject;
                playerSkillController = skillController;
                isPlayerInRange = true;

                InteractPromptUI.Instance?.Register(this, interactLabel, HandlePromptInteract, HandleFocusChanged, promptKey);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isPlayerInRange)
        {
            if (collision.transform.root.gameObject == currentPlayer)
            {
                isPlayerInRange = false;
                currentPlayer = null;
                playerSkillController = null;

                InteractPromptUI.Instance?.Unregister(this);
                HandleFocusChanged(false);
            }
        }
    }
}