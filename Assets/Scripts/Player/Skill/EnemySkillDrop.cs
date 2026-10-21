using UnityEngine;

public class EnemySkillDrop : MonoBehaviour
{
    [Header("Skill Drop Settings")]
    public SkillData skillToDrop;

    [Range(0.05f, 0.9f)]
    public float hpThresholdPercent = 0.5f;

    public float absorbRange = 3.5f;

    [Header("Visuals & Prompt")]
    public GameObject absorbPromptUI;
    public GameObject absorbSuccessVFX;

    [Header("References (Auto-assigned if empty)")]
    public CoreHealth enemyHealth;
    private Transform playerTransform;
    private PlayerSkillController playerSkill;
    private PlayerInputHandler playerInput;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<CoreHealth>();
        if (absorbPromptUI != null) absorbPromptUI.SetActive(false);
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerSkill = playerObj.GetComponentInParent<PlayerSkillController>();
            if (playerSkill == null) playerSkill = playerObj.GetComponentInChildren<PlayerSkillController>();

            playerInput = playerObj.GetComponentInParent<PlayerInputHandler>();
            if (playerInput == null) playerInput = playerObj.GetComponentInChildren<PlayerInputHandler>();
        }
    }

    private void Update()
    {
        if (playerTransform == null || playerSkill == null)
        {
            FindPlayerReferences();
            return;
        }

        if (enemyHealth == null || enemyHealth.currentHealth <= 0)
        {
            if (absorbPromptUI != null && absorbPromptUI.activeSelf)
                absorbPromptUI.SetActive(false);
            return;
        }

        if (!playerSkill.hasPerish) return;

        float currentHpRatio = (float)enemyHealth.currentHealth / enemyHealth.maxHealth;
        bool isLowHP = currentHpRatio <= hpThresholdPercent;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isInRange = distanceToPlayer <= absorbRange;

        bool canAbsorb = isLowHP && isInRange;
        if (absorbPromptUI != null && absorbPromptUI.activeSelf != canAbsorb)
        {
            absorbPromptUI.SetActive(canAbsorb);
        }

        if (canAbsorb && playerInput != null && playerInput.InteractInput)
        {
            playerInput.UseInteractInput();
            ExecuteAbsorb();
        }
    }

    private void ExecuteAbsorb()
    {
        if (playerSkill != null && skillToDrop != null)
        {
            playerSkill.UnlockSkill(skillToDrop);
        }

        if (absorbSuccessVFX != null)
        {
            Instantiate(absorbSuccessVFX, transform.position, Quaternion.identity);
        }

        if (absorbPromptUI != null) absorbPromptUI.SetActive(false);

        if (enemyHealth != null)
        {
            enemyHealth.Die();
        }
        else
        {
            Destroy(gameObject);
        }

        this.enabled = false;
    }
}