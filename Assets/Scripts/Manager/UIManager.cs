using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Player References")]
    public CoreHealth playerHealth;
    public PlayerStats playerStats;

    [Header("UI Elements: Health Bar")]
    public Image healthBarFill;
    [Tooltip("(Optional) Secondary fill image placed BEHIND healthBarFill for delayed damage catch-up.")]
    public Image healthBarDelayedFill;
    [Tooltip("(Optional) Parent RectTransform of the HP bar to widen smoothly when Max HP increases.")]
    public RectTransform healthBarContainer;
    [Tooltip("(Optional) Text element displaying current/max HP (e.g. 5 / 6).")]
    public TextMeshProUGUI healthText;
    [Tooltip("Starting Max HP used to calculate width scaling. Keep 0 to auto-detect from CoreHealth on Start.")]
    [SerializeField] private float baseMaxHealth = 0f;
    [Tooltip("Maximum width multiplier so the HP bar never stretches off-screen.")]
    [SerializeField] private float maxHealthBarWidthMultiplier = 2.5f;

    [Header("UI Elements: Energy Bar")]
    public Image energyBarFill;
    [Tooltip("(Optional) Parent RectTransform of the MP bar to widen smoothly when Max MP increases.")]
    public RectTransform energyBarContainer;
    [Tooltip("(Optional) Text element displaying current/max MP (e.g. 100 / 120).")]
    public TextMeshProUGUI energyText;
    [Tooltip("Maximum width multiplier so the MP bar never stretches off-screen.")]
    [SerializeField] private float maxEnergyBarWidthMultiplier = 2.5f;

    [Header("Bar Feel Settings")]
    [Tooltip("Smoothly animate bar fill changes. Set 0 for instant snap.")]
    [SerializeField] private float barLerpSpeed = 12f;
    [Tooltip("How long the delayed damage bar holds before catching up.")]
    [SerializeField] private float damageDelayDuration = 0.35f;
    [Tooltip("Speed at which the delayed damage bar drains down to the current HP.")]
    [SerializeField] private float damageCatchupSpeed = 4.5f;
    [Tooltip("Speed at which HP and MP containers widen after collecting Max upgrades.")]
    [SerializeField] private float containerResizeSpeed = 8f;

    [Header("Health & Energy Juice")]
    [SerializeField] private float healthShakeDuration = 0.16f;
    [SerializeField] private float healthShakeMagnitude = 5f;
    [Range(0f, 0.5f)][SerializeField] private float lowHealthThreshold = 0.25f;
    [SerializeField] private Color lowHealthPulseColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private float lowHealthPulseSpeed = 6f;

    [SerializeField] private float energyFailShakeDuration = 0.22f;
    [SerializeField] private float energyFailShakeMagnitude = 6f;
    [SerializeField] private Color energyFailFlashColor = new Color(1f, 0.28f, 0.35f, 1f);

    [Header("UI Elements: Character Portrait")]
    public Image characterPortraitImage;

    [Header("UI Elements: Skill Carousel (3 Diamonds)")]
    public Image prevSkillIcon;
    public Image currentSkillIcon;
    public Image nextSkillIcon;
    [Tooltip("Center diamond RectTransform for punch-scale feedback when cycling skills.")]
    public RectTransform currentSkillSlotTransform;
    [SerializeField] private float previewSlotAlpha = 0.55f;
    [SerializeField] private Color insufficientEnergySkillTint = new Color(0.42f, 0.42f, 0.5f, 0.72f);
    [SerializeField] private float switchPunchScale = 1.18f;
    [SerializeField] private float switchPunchReturnSpeed = 14f;

    [Header("UI Elements: Life Gems (Potions)")]
    public Image[] potionIcons;
    public Sprite gemFullSprite;
    public Sprite gemEmptySprite;
    [SerializeField] private float gemPunchScale = 1.28f;
    [SerializeField] private float gemPunchReturnSpeed = 12f;

    [Header("UI Elements: Currency")]
    public TextMeshProUGUI currencyText;
    [Tooltip("Duration in seconds for the rolling currency counter to reach the new amount.")]
    [SerializeField] private float currencyRollDuration = 0.35f;
    [SerializeField] private float currencyPunchScale = 1.18f;
    [SerializeField] private float currencyPunchReturnSpeed = 14f;

    private SkillData currentDisplayedSkill;

    private float baseHealthBarWidth = -1f;
    private float targetHealthBarWidth = -1f;
    private Vector2 baseHealthBarAnchoredPos;
    private bool hasCachedHealthBarPos;

    private float baseEnergyBarWidth = -1f;
    private float targetEnergyBarWidth = -1f;
    private Vector2 baseEnergyBarAnchoredPos;
    private bool hasCachedEnergyBarPos;

    private float targetHealthFill = 1f;
    private float targetEnergyFill = 1f;
    private float lastKnownHealth = -1f;
    private int lastKnownPotions = -1;

    private float damageDelayTimer = 0f;
    private float healthShakeTimer = 0f;
    private float energyFailTimer = 0f;

    private Color originalHealthFillColor = Color.white;
    private Color originalEnergyFillColor = Color.white;

    private float displayedCurrency = 0f;
    private int targetCurrency = 0;
    private float currencyRollSpeed = 0f;
    private bool hasInitializedCurrency = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (healthBarFill != null)
            originalHealthFillColor = healthBarFill.color;

        if (energyBarFill != null)
            originalEnergyFillColor = energyBarFill.color;

        if (healthBarContainer != null)
        {
            baseHealthBarWidth = healthBarContainer.sizeDelta.x > 0f
                ? healthBarContainer.sizeDelta.x
                : healthBarContainer.rect.width;
            targetHealthBarWidth = baseHealthBarWidth;
        }

        if (energyBarContainer != null)
        {
            baseEnergyBarWidth = energyBarContainer.sizeDelta.x > 0f
                ? energyBarContainer.sizeDelta.x
                : energyBarContainer.rect.width;
            targetEnergyBarWidth = baseEnergyBarWidth;
        }
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

        if (playerHealth != null)
        {
            if (baseMaxHealth <= 0f)
                baseMaxHealth = Mathf.Max(1f, playerHealth.maxHealth);

            lastKnownHealth = playerHealth.currentHealth;
        }

        if (playerStats != null)
        {
            lastKnownPotions = playerStats.currentPotions;
            targetCurrency = playerStats.currentCurrency;
            displayedCurrency = targetCurrency;
            hasInitializedCurrency = true;
        }

        if (healthBarContainer != null)
        {
            baseHealthBarAnchoredPos = healthBarContainer.anchoredPosition;
            hasCachedHealthBarPos = true;
        }

        if (energyBarContainer != null)
        {
            baseEnergyBarAnchoredPos = energyBarContainer.anchoredPosition;
            hasCachedEnergyBarPos = true;
        }

        UpdateHealthUI();
        UpdateStatsUI();

        if (healthBarFill != null) healthBarFill.fillAmount = targetHealthFill;
        if (healthBarDelayedFill != null) healthBarDelayedFill.fillAmount = targetHealthFill;
        if (energyBarFill != null) energyBarFill.fillAmount = targetEnergyFill;

        if (healthBarContainer != null && targetHealthBarWidth > 0f)
        {
            Vector2 size = healthBarContainer.sizeDelta;
            size.x = targetHealthBarWidth;
            healthBarContainer.sizeDelta = size;
        }

        if (energyBarContainer != null && targetEnergyBarWidth > 0f)
        {
            Vector2 size = energyBarContainer.sizeDelta;
            size.x = targetEnergyBarWidth;
            energyBarContainer.sizeDelta = size;
        }

        if (currencyText != null)
            currencyText.text = targetCurrency.ToString();
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (barLerpSpeed > 0f)
        {
            float step = dt * barLerpSpeed;
            if (healthBarFill != null)
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetHealthFill, step);
            if (energyBarFill != null)
                energyBarFill.fillAmount = Mathf.Lerp(energyBarFill.fillAmount, targetEnergyFill, step);
        }

        if (healthBarDelayedFill != null)
        {
            if (healthBarDelayedFill.fillAmount < targetHealthFill)
            {
                healthBarDelayedFill.fillAmount = healthBarFill != null
                    ? healthBarFill.fillAmount
                    : targetHealthFill;
                damageDelayTimer = 0f;
            }
            else if (healthBarDelayedFill.fillAmount > targetHealthFill)
            {
                if (damageDelayTimer > 0f)
                {
                    damageDelayTimer -= dt;
                }
                else
                {
                    healthBarDelayedFill.fillAmount = Mathf.MoveTowards(
                        healthBarDelayedFill.fillAmount,
                        targetHealthFill,
                        dt * damageCatchupSpeed * 0.45f
                    );
                }
            }
        }

        SmoothResizeContainer(healthBarContainer, targetHealthBarWidth, dt);
        SmoothResizeContainer(energyBarContainer, targetEnergyBarWidth, dt);

        if (healthBarContainer != null && hasCachedHealthBarPos && healthShakeTimer > 0f)
        {
            healthShakeTimer -= dt;
            if (healthShakeTimer <= 0f)
            {
                healthBarContainer.anchoredPosition = baseHealthBarAnchoredPos;
            }
            else
            {
                float progress = healthShakeTimer / Mathf.Max(0.01f, healthShakeDuration);
                Vector2 jitter = Random.insideUnitCircle * (healthShakeMagnitude * progress);
                healthBarContainer.anchoredPosition = baseHealthBarAnchoredPos + jitter;
            }
        }

        if (energyFailTimer > 0f)
        {
            energyFailTimer -= dt;
            float progress = Mathf.Clamp01(energyFailTimer / Mathf.Max(0.01f, energyFailShakeDuration));

            if (energyBarContainer != null && hasCachedEnergyBarPos)
            {
                if (energyFailTimer <= 0f)
                {
                    energyBarContainer.anchoredPosition = baseEnergyBarAnchoredPos;
                }
                else
                {
                    float shakeX = Mathf.Sin(Time.unscaledTime * 55f) * energyFailShakeMagnitude * progress;
                    energyBarContainer.anchoredPosition = baseEnergyBarAnchoredPos + new Vector2(shakeX, 0f);
                }
            }

            if (energyBarFill != null)
            {
                energyBarFill.color = energyFailTimer <= 0f
                    ? originalEnergyFillColor
                    : Color.Lerp(originalEnergyFillColor, energyFailFlashColor, progress);
            }
        }

        if (healthBarFill != null)
        {
            if (targetHealthFill > 0f && targetHealthFill <= lowHealthThreshold)
            {
                float pulse = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
                healthBarFill.color = Color.Lerp(originalHealthFillColor, lowHealthPulseColor, pulse * 0.65f);
            }
            else if (healthBarFill.color != originalHealthFillColor)
            {
                healthBarFill.color = originalHealthFillColor;
            }
        }

        if (potionIcons != null)
        {
            for (int i = 0; i < potionIcons.Length; i++)
            {
                if (potionIcons[i] == null) continue;
                Transform t = potionIcons[i].transform;
                if (t.localScale != Vector3.one)
                {
                    t.localScale = Vector3.Lerp(t.localScale, Vector3.one, dt * gemPunchReturnSpeed);
                    if ((t.localScale - Vector3.one).sqrMagnitude < 0.0001f) t.localScale = Vector3.one;
                }
            }
        }

        if (currencyText != null)
        {
            if (!Mathf.Approximately(displayedCurrency, targetCurrency))
            {
                displayedCurrency = Mathf.MoveTowards(displayedCurrency, targetCurrency, currencyRollSpeed * dt);
                currencyText.text = Mathf.RoundToInt(displayedCurrency).ToString();
            }

            Transform ct = currencyText.transform;
            if (ct.localScale != Vector3.one)
            {
                ct.localScale = Vector3.Lerp(ct.localScale, Vector3.one, dt * currencyPunchReturnSpeed);
                if ((ct.localScale - Vector3.one).sqrMagnitude < 0.0001f) ct.localScale = Vector3.one;
            }
        }

        if (currentSkillSlotTransform != null && currentSkillSlotTransform.localScale != Vector3.one)
        {
            currentSkillSlotTransform.localScale = Vector3.Lerp(
                currentSkillSlotTransform.localScale,
                Vector3.one,
                dt * switchPunchReturnSpeed
            );
            if ((currentSkillSlotTransform.localScale - Vector3.one).sqrMagnitude < 0.0001f)
                currentSkillSlotTransform.localScale = Vector3.one;
        }
    }
    public void PlayNotEnoughEnergyFeedback()
    {
        energyFailTimer = energyFailShakeDuration;

        if (currentSkillSlotTransform != null)
        {
            currentSkillSlotTransform.localScale = Vector3.one * 0.88f;
        }
    }

    private void SmoothResizeContainer(RectTransform container, float targetWidth, float dt)
    {
        if (container == null || targetWidth <= 0f) return;

        Vector2 size = container.sizeDelta;
        if (!Mathf.Approximately(size.x, targetWidth))
        {
            size.x = Mathf.Lerp(size.x, targetWidth, dt * containerResizeSpeed);
            if (Mathf.Abs(size.x - targetWidth) < 0.2f) size.x = targetWidth;
            container.sizeDelta = size;
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
        if (playerHealth == null) return;

        float maxHp = Mathf.Max(1f, playerHealth.maxHealth);
        if (baseMaxHealth <= 0f)
            baseMaxHealth = maxHp;

        float currentHp = Mathf.Clamp(playerHealth.currentHealth, 0f, maxHp);
        targetHealthFill = Mathf.Clamp01(currentHp / maxHp);

        if (lastKnownHealth >= 0f && currentHp < lastKnownHealth)
        {
            damageDelayTimer = damageDelayDuration;
            if (healthShakeMagnitude > 0f && healthShakeDuration > 0f)
                healthShakeTimer = healthShakeDuration;
        }
        lastKnownHealth = currentHp;

        if (healthBarFill != null && barLerpSpeed <= 0f)
            healthBarFill.fillAmount = targetHealthFill;

        if (healthBarContainer != null && baseHealthBarWidth > 0f && baseMaxHealth > 0f)
        {
            float widthMultiplier = Mathf.Clamp(maxHp / baseMaxHealth, 1f, maxHealthBarWidthMultiplier);
            targetHealthBarWidth = baseHealthBarWidth * widthMultiplier;
        }

        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(currentHp)} / {Mathf.RoundToInt(maxHp)}";
    }

    private void UpdateStatsUI()
    {
        if (playerStats == null) TryFindPlayer();
        if (playerStats == null) return;

        float maxMp = Mathf.Max(1f, playerStats.maxEnergy);
        targetEnergyFill = Mathf.Clamp01(playerStats.currentEnergy / maxMp);

        if (energyBarFill != null && barLerpSpeed <= 0f)
            energyBarFill.fillAmount = targetEnergyFill;

        if (energyBarContainer != null && baseEnergyBarWidth > 0f && playerStats.baseMaxEnergy > 0f)
        {
            float widthMultiplier = Mathf.Clamp(maxMp / playerStats.baseMaxEnergy, 1f, maxEnergyBarWidthMultiplier);
            targetEnergyBarWidth = baseEnergyBarWidth * widthMultiplier;
        }

        if (energyText != null)
            energyText.text = $"{Mathf.RoundToInt(playerStats.currentEnergy)} / {Mathf.RoundToInt(maxMp)}";

        UpdateCurrentSkillAffordability();

        if (potionIcons != null)
        {
            int newPotions = playerStats.currentPotions;

            for (int i = 0; i < potionIcons.Length; i++)
            {
                if (potionIcons[i] == null) continue;

                bool withinMaxSlots = i < playerStats.maxPotions;
                potionIcons[i].gameObject.SetActive(withinMaxSlots);

                if (withinMaxSlots)
                {
                    potionIcons[i].sprite = (i < newPotions) ? gemFullSprite : gemEmptySprite;

                    if (lastKnownPotions >= 0 && newPotions != lastKnownPotions)
                    {
                        bool gemWasConsumed = (newPotions < lastKnownPotions && i >= newPotions && i < lastKnownPotions);
                        bool gemWasRefilled = (newPotions > lastKnownPotions && i >= lastKnownPotions && i < newPotions);
                        if (gemWasConsumed || gemWasRefilled)
                        {
                            potionIcons[i].transform.localScale = Vector3.one * gemPunchScale;
                        }
                    }
                }
            }

            lastKnownPotions = newPotions;
        }

        if (currencyText != null)
        {
            int newCurrency = playerStats.currentCurrency;
            if (!hasInitializedCurrency)
            {
                targetCurrency = newCurrency;
                displayedCurrency = newCurrency;
                currencyText.text = newCurrency.ToString();
                hasInitializedCurrency = true;
            }
            else if (newCurrency != targetCurrency)
            {
                targetCurrency = newCurrency;
                float diff = Mathf.Abs(targetCurrency - displayedCurrency);
                currencyRollSpeed = Mathf.Max(20f, diff / Mathf.Max(0.05f, currencyRollDuration));
                currencyText.transform.localScale = Vector3.one * currencyPunchScale;
            }
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
        currentDisplayedSkill = currentSkill;

        ApplySkillSlotIcon(prevSkillIcon, prevSkill, previewSlotAlpha);
        ApplySkillSlotIcon(currentSkillIcon, currentSkill, 1f);
        ApplySkillSlotIcon(nextSkillIcon, nextSkill, previewSlotAlpha);

        UpdateCurrentSkillAffordability();

        if (playSwitchFeedback && currentSkillSlotTransform != null)
        {
            currentSkillSlotTransform.localScale = Vector3.one * switchPunchScale;
        }
    }

    private void UpdateCurrentSkillAffordability()
    {
        if (currentSkillIcon == null || !currentSkillIcon.enabled) return;

        bool canAfford = true;
        if (currentDisplayedSkill is ActiveSkillData activeSkill && playerStats != null)
        {
            canAfford = playerStats.currentEnergy >= activeSkill.mpCost;
        }

        currentSkillIcon.color = canAfford ? Color.white : insufficientEnergySkillTint;
    }

    private void ApplySkillSlotIcon(Image targetImage, SkillData skill, float alpha)
    {
        if (targetImage == null) return;

        if (skill != null && skill.skillIcon != null)
        {
            targetImage.sprite = skill.skillIcon;
            Color c = Color.white;
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