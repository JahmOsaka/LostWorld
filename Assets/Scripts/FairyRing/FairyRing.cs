using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class FairyRing : MonoBehaviour
{
    [Header("Fairy Ring State")]
    [Tooltip("Indicates if this checkpoint has been activated by the player.")]
    public bool isActivated = false;

    [Header("Visuals & UI")]
    [Tooltip("Prompt icon shown when the player stands near the Fairy Ring.")]
    public GameObject interactPromptUI;
    [Tooltip("Visual effect (e.g., glowing particles) enabled once activated.")]
    public GameObject activatedVFX;
    public SpriteHighlight highlightEffect;

    [Header("Menu Reference")]
    [Tooltip("Reference to the Skill Select UI manager in the Canvas.")]
    public SkillEquipUI skillEquipMenu;

    private bool isPlayerInRange = false;
    private PlayerInputHandler playerInput;
    private CoreHealth playerHealth;
    private PlayerSkillController playerSkill;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 1.5f;
        }
    }

    private void Start()
    {
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (activatedVFX != null) activatedVFX.SetActive(isActivated);
    }

    private void Update()
    {
        if (isPlayerInRange && playerInput != null && playerInput.InteractInput)
        {
            if (skillEquipMenu != null && !skillEquipMenu.IsMenuOpen)
            {
                playerInput.UseInteractInput();
                InteractWithFairyRing();
            }
        }
    }

    private void InteractWithFairyRing()
    {
        if (!isActivated)
        {
            isActivated = true;
            if (activatedVFX != null) activatedVFX.SetActive(true);
            Debug.Log("Fairy Ring Activated!");
        }

        if (playerHealth != null)
        {
            playerHealth.Heal(playerHealth.maxHealth);
        }

        if (skillEquipMenu != null && playerSkill != null)
        {
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
            skillEquipMenu.OpenMenu(playerSkill);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponentInParent<PlayerInputHandler>();
            playerHealth = collision.GetComponentInParent<CoreHealth>();
            playerSkill = collision.GetComponentInParent<PlayerSkillController>();

            isPlayerInRange = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
            if (highlightEffect != null) highlightEffect.TurnOnHighlight();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerInput = null;
            playerHealth = null;
            playerSkill = null;

            if (interactPromptUI != null) interactPromptUI.SetActive(false);
            if (highlightEffect != null) highlightEffect.TurnOffHighlight();

            if (skillEquipMenu != null && skillEquipMenu.IsMenuOpen)
            {
                skillEquipMenu.CloseMenu();
            }
        }
    }
}