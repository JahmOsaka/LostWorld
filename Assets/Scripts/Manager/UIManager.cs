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
    [Tooltip("(Optional) Parent RectTransform of the MP bar to widen automatically when Max MP increases.")]
    public RectTransform energyBarContainer;
    [Tooltip("(Optional) Text element displaying current/max MP (e.g. 100/120).")]
    public TextMeshProUGUI energyText;

    [Header("Bar Feel Settings")]
    [Tooltip("Smoothly animate bar fill changes. Set 0 for instant snap.")]
    [SerializeField] private float barLerpSpeed = 12f;

    [Header("UI Elements: Character Portrait")]
    public Image characterPortraitImage;

    [Header("UI Elements: Skill Carousel (3 Diamonds)")]
    public Image prevSkillIcon;
    public Image currentSkillIcon;
    public Image nextSkillIcon;
    [Tooltip("Center diamond RectTransform for punch-scale feedback when cycling skills.")]
    public RectTransform currentSkillSlotTransform;
    [SerializeField] private float previewSlotAlpha = 0.55f;
    [SerializeField] private float switchPunchScale = 1.18f;
    [SerializeField] private float switchPunchReturnSpeed = 14f;

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
    private float baseEnergyBarWidth = -1f;
    private float targetHealthFill = 1f;
    private float targetEnergyFill = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (energyBarContainer != null)
            baseEnergyBarWidth = energyBarContainer.sizeDelta.x;

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

        if (healthBarFill != null) healthBarFill.fillAmount = targetHealthFill;
        if (energyBarFill != null) energyBarFill.fillAmount = targetEnergyFill;
    }

    private void Update()
    {
        if (barLerpSpeed > 0f)
        {
            float step = Time.unscaledDeltaTime * barLerpSpeed;
            if (healthBarFill != null)
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealthFill, step);
            if (energyBarFill != null)
                energyBarFill.fillAmount = Mathf.Lerp(energyBarFill.fillAmount, targetEnergyFill, step);
        }

        if (absorbPromptPanel != null && absorbPromptPanel.activeSelf)
        {
            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 6f) * 0.04f;
            absorbPromptPanel.transform.localScale = Vector3.Lerp(
                absorbPromptPanel.transform.localScale,
                new Vector3(pulse, pulse, 1f),
                Time.unscaledDeltaTime * 15f
            );
        }

        if (currentSkillSlotTransform != null && currentSkillSlotTransform.localScale != Vector3.one)
        {
            currentSkillSlotTransform.localScale = Vector3.Lerp(
                currentSkillSlotTransform.localScale,
                Vector3.one,
                Time.unscaledDeltaTime * switchPunchReturnSpeed
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
            float maxHp = Mathf.Max(1f, playerHealth.maxHealth);
            targetHealthFill = Mathf.Clamp01(playerHealth.currentHealth / maxHp);

            if (barLerpSpeed <= 0f)
                healthBarFill.fillAmount = targetHealthFill;
        }
    }

    private void UpdateStatsUI()
    {
        if (playerStats == null) TryFindPlayer();

        if (playerStats != null)
        {
            float maxMp = Mathf.Max(1f, playerStats.maxEnergy);
            targetEnergyFill = Mathf.Clamp01(playerStats.currentEnergy / maxMp);

            if (energyBarFill != null && barLerpSpeed <= 0f)
                energyBarFill.fillAmount = targetEnergyFill;

            if (energyBarContainer != null && baseEnergyBarWidth > 0f && playerStats.baseMaxEnergy > 0f)
            {
                float widthMultiplier = maxMp / playerStats.baseMaxEnergy;
                Vector2 size = energyBarContainer.sizeDelta;
                size.x = baseEnergyBarWidth * widthMultiplier;
                energyBarContainer.sizeDelta = size;
            }

            if (energyText != null)
                energyText.text = $"{Mathf.RoundToInt(playerStats.currentEnergy)} / {Mathf.RoundToInt(maxMp)}";

            if (potionIcons != null)
            {
                for (int i = 0; i < potionIcons.Length; i++)
                {
                    if (potionIcons[i] == null) continue;

                    bool withinMaxSlots = i < playerStats.maxPotions;
                    potionIcons[i].gameObject.SetActive(withinMaxSlots);

                    if (withinMaxSlots)
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

    public void UpdateSkillCarousel(SkillData prevSkill, SkillData currentSkill, SkillData nextSkill, bool playSwitchFeedback = false)
    {
        ApplySkillSlotIcon(prevSkillIcon, prevSkill, previewSlotAlpha);
        ApplySkillSlotIcon(currentSkillIcon, currentSkill, 1f);
        ApplySkillSlotIcon(nextSkillIcon, nextSkill, previewSlotAlpha);

        if (playSwitchFeedback && currentSkillSlotTransform != null)
        {
            currentSkillSlotTransform.localScale = Vector3.one * switchPunchScale;
        }
    }

    private void ApplySkillSlotIcon(Image targetImage, SkillData skill, float alpha)
    {
        if (targetImage == null) return;

        if (skill != null && skill.skillIcon != null)
        {
            targetImage.sprite = skill.skillIcon;
            Color c = targetImage.color;
            c.a = alpha;
            targetImage.color = c;
            targetImage.enabled = true;
        }
        else
        {
            targetImage.sprite = null;
            targetImage.enabled = false;
        }
    }

    public void UpdatePortrait(Sprite portraitSprite)
    {
        if (characterPortraitImage != null && portraitSprite != null)
        {
            characterPortraitImage.sprite = portraitSprite;
        }
    }
}