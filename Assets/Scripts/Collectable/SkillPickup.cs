using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class SkillPickup : MonoBehaviour
{
    [Header("Skill Settings")]
    [Tooltip("The skill (Key or Combat) to unlock when interacting with this corpse/remnant.")]
    public SkillData skillToGive;

    [Header("UI Prompt")]
    [Tooltip("Child GameObject showing the interact button icon.")]
    public GameObject promptUI;
    public GameObject absorbVFX;
    [Tooltip("The highlight effect script attached to this object.")]
    public SpriteHighlight highlightEffect;

    private PlayerSkillController playerSkillController;
    private PlayerInputHandler playerInput;
    private bool isPlayerInRange = false;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 1.2f;
        }
    }

    private void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    private void Update()
    {
        if (isPlayerInRange && playerInput != null && playerInput.InteractInput)
        {
            playerInput.UseInteractInput();
            CollectSkill();
        }
    }

    private void CollectSkill()
    {
        if (playerSkillController != null && skillToGive != null)
        {
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
        if (collision.CompareTag("Player"))
        {
            playerSkillController = collision.GetComponentInParent<PlayerSkillController>();
            playerInput = collision.GetComponentInParent<PlayerInputHandler>();

            if (playerSkillController != null)
            {
                isPlayerInRange = true;
                if (promptUI != null) promptUI.SetActive(true);
                if (highlightEffect != null) highlightEffect.TurnOnHighlight();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerSkillController = null;
            playerInput = null;
            if (promptUI != null) promptUI.SetActive(false);
            if (highlightEffect != null) highlightEffect.TurnOffHighlight();
        }
    }
}