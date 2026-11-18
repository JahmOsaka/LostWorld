using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player References")]
    public CoreHealth playerHealth;
    public PlayerStats playerStats;

    [Header("UI Elements: Bars")]
    public Image healthBarFill;
    public Image energyBarFill;

    [Header("UI Elements: Skill")]
    public Image currentSkillIcon;

    [Header("UI Elements: Life Gems (Potions)")]
    public Image[] potionIcons;
    public Sprite gemFullSprite;
    public Sprite gemEmptySprite;

    [Header("UI Elements: Others")]
    public TextMeshProUGUI currencyText;

    [Header("UI Elements: Absorb Echo Prompt (WuWa Style)")]
    [Tooltip("Assign the screen-space prompt panel for skill absorption here.")]
    public GameObject absorbPromptPanel;
    [Tooltip("(Optional) Text element displaying the skill name to absorb.")]
    public TextMeshProUGUI absorbPromptText;
    [Tooltip("(Optional) Image element displaying the icon of the skill to absorb.")]
    public Image absorbSkillIcon;

    private EnemySkillDrop currentPromptOwner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (absorbPromptPanel != null)
            absorbPromptPanel.SetActive(false);
    }

    private void OnEnable()
    {
        CoreHealth.OnHealthChanged += UpdateHealthUI;
        PlayerStats.OnStatsChanged += UpdateStatsUI;
    }

    private void OnDisable()
    {
        CoreHealth.OnHealthChanged -= UpdateHealthUI;
        PlayerStats.OnStatsChanged -= UpdateStatsUI;
    }

    private void Start()
    {
        TryFindPlayer();
        UpdateHealthUI();
        UpdateStatsUI();
    }

    private void Update()
    {
        if (absorbPromptPanel != null && absorbPromptPanel.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.04f;
            absorbPromptPanel.transform.localScale = Vector3.Lerp(
                absorbPromptPanel.transform.localScale,
                new Vector3(pulse, pulse, 1f),
                Time.unscaledDeltaTime * 15f
            );
        }
    }

    public void ShowAbsorbPrompt(EnemySkillDrop requester, SkillData skill)
    {
        currentPromptOwner = requester;
        if (absorbPromptPanel == null) return;

        if (!absorbPromptPanel.activeSelf)
        {
            absorbPromptPanel.transform.localScale = Vector3.one * 0.75f;
            absorbPromptPanel.SetActive(true);
        }

        if (absorbPromptText != null)
        {
            string sName = (skill != null && !string.IsNullOrEmpty(skill.skillName)) ? skill.skillName : "Essence";
            absorbPromptText.text = $"Absorb <color=#FFD700>{sName}</color>";
        }

        if (absorbSkillIcon != null)
        {
            if (skill != null && skill.skillIcon != null)
            {
                absorbSkillIcon.sprite = skill.skillIcon;
                absorbSkillIcon.enabled = true;
            }
            else
            {
                absorbSkillIcon.enabled = false;
            }
        }
    }

    public void HideAbsorbPrompt(EnemySkillDrop requester)
    {
        if (currentPromptOwner == requester || currentPromptOwner == null)
        {
            currentPromptOwner = null;
            if (absorbPromptPanel != null && absorbPromptPanel.activeSelf)
            {
                absorbPromptPanel.SetActive(false);
            }
        }
    }

    private void TryFindPlayer()
    {
        if (playerStats == null)
            playerStats = FindAnyObjectByType<PlayerStats>();

        if (playerHealth == null && playerStats != null)
            playerHealth = playerStats.GetComponent<CoreHealth>();
    }

    private void UpdateHealthUI()
    {
        if (playerHealth == null) TryFindPlayer();

        if (playerHealth != null && healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)playerHealth.currentHealth / playerHealth.maxHealth;
        }
    }

    private void UpdateStatsUI()
    {
        if (playerStats == null) TryFindPlayer();

        if (playerStats != null)
        {
            if (energyBarFill != null)
                energyBarFill.fillAmount = playerStats.currentEnergy / playerStats.maxEnergy;

            if (potionIcons != null)
            {
                for (int i = 0; i < potionIcons.Length; i++)
                {
                    if (potionIcons[i] != null)
                        potionIcons[i].sprite = (i < playerStats.currentPotions) ? gemFullSprite : gemEmptySprite;
                }
            }

            if (currencyText != null)
                currencyText.text = playerStats.currentCurrency.ToString();
        }
    }

    public void UpdateSkillIcon(Sprite newSkillSprite)
    {
        if (currentSkillIcon != null && newSkillSprite != null)
        {
            currentSkillIcon.sprite = newSkillSprite;
        }
    }
}