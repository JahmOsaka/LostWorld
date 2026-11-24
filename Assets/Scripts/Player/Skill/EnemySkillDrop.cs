using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkillDrop : MonoBehaviour
{
    private static readonly List<EnemySkillDrop> activeCandidates = new List<EnemySkillDrop>();
    private static EnemySkillDrop closestCandidate;

    private static readonly Color DefaultWhipColor = new Color32(0x43, 0x25, 0x23, 0xFF);

    [Header("Skill Drop Settings (GDD: Perish Absorb)")]
    [Tooltip("The skill or ability unlocked upon successfully absorbing this enemy.")]
    public SkillData skillToDrop;

    [Tooltip("HP ratio threshold required to trigger absorption (GDD specifies 10% = 0.1).")]
    [Range(0.05f, 0.9f)]
    public float hpThresholdPercent = 0.1f;

    [Tooltip("Maximum distance from the player to allow skill absorption.")]
    public float absorbRange = 3.5f;

    [Header("Target Highlight & Visual Cue")]
    [Tooltip("(Optional) SpriteHighlight component to show an outline when this enemy is the active absorb target.")]
    public SpriteHighlight highlightEffect;
    [Tooltip("Tint color pulsed on the enemy when its HP is low enough to be absorbed.")]
    public Color absorbablePulseColor = new Color(1f, 0.55f, 0.55f, 1f);

    [Header("Tech Art: Rubber Band Whip Bind")]
    [Tooltip("Duration of the whip binding and enemy struggling sequence (in seconds).")]
    public float bindDuration = 1.0f;
    [Tooltip("Line width of the procedural whip.")]
    public float whipWidth = 0.07f;
    [Tooltip("Number of tight horizontal whip bands wrapped around the enemy's waist.")]
    [Range(1, 4)] public int bandCount = 2;

    [Header("Tech Art: Jelly Struggle (Squash, Stretch & Tilt)")]
    [Tooltip("Speed of the jelly wobble and left-right tilting (lower values feel more jelly-like).")]
    public float jellyWobbleSpeed = 15f;
    [Tooltip("Intensity of the squash and stretch deformation while bound.")]
    public float jellyScaleAmount = 0.18f;
    [Tooltip("Maximum angle (in degrees) the enemy tilts left and right while struggling.")]
    public float struggleTiltAngle = 14f;
    [Tooltip("Horizontal shifting distance while struggling.")]
    public float struggleShiftX = 0.05f;

    [Header("Visuals")]
    [Tooltip("VFX prefab spawned when the skill absorption completes.")]
    public GameObject absorbSuccessVFX;

    [Header("References (Auto-assigned if empty)")]
    [Tooltip("Reference to the enemy's CoreHealth component.")]
    public CoreHealth enemyHealth;
    private Enemy enemyScript;
    private Transform enemyVisual;
    private SpriteRenderer enemySpriteRenderer;
    private Color originalSpriteColor = Color.white;
    private Transform playerTransform;
    private Player playerComp;
    private PlayerSkillController playerSkill;
    private PlayerInputHandler playerInput;

    private bool isAbsorbing = false;
    private bool isHighlighted = false;

    private void Awake()
    {
        if (enemyHealth == null) enemyHealth = GetComponent<CoreHealth>();
        enemyScript = GetComponent<Enemy>();
        enemySpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (enemySpriteRenderer != null) originalSpriteColor = enemySpriteRenderer.color;

        if (highlightEffect == null) highlightEffect = GetComponentInChildren<SpriteHighlight>();

        Transform foundVisual = transform.Find("Visual");
        if (foundVisual != null)
        {
            enemyVisual = foundVisual;
        }
        else
        {
            enemyVisual = (enemySpriteRenderer != null) ? enemySpriteRenderer.transform : transform;
        }
    }

    private void Start()
    {
        FindPlayerReferences();
    }

    private void OnDisable()
    {
        SetTargetHighlight(false);
        InteractPromptUI.Instance?.Unregister(this);
    }

    private void FindPlayerReferences()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerComp = playerObj.GetComponentInParent<Player>() ?? playerObj.GetComponent<Player>();
            playerSkill = playerObj.GetComponentInParent<PlayerSkillController>() ?? playerObj.GetComponentInChildren<PlayerSkillController>();
            playerInput = playerObj.GetComponentInParent<PlayerInputHandler>() ?? playerObj.GetComponentInChildren<PlayerInputHandler>();
        }
    }

    private void Update()
    {
        if (isAbsorbing) return;

        if (playerTransform == null || playerSkill == null)
        {
            FindPlayerReferences();
            return;
        }

        if (enemyHealth == null || enemyHealth.currentHealth <= 0 || !playerSkill.hasPerish)
        {
            SetTargetHighlight(false);
            InteractPromptUI.Instance?.Unregister(this);
            return;
        }

        float currentHpRatio = (float)enemyHealth.currentHealth / enemyHealth.maxHealth;
        bool isLowHP = currentHpRatio <= hpThresholdPercent;

        if (isLowHP && enemySpriteRenderer != null)
        {
            float pulse = (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f;
            enemySpriteRenderer.color = Color.Lerp(originalSpriteColor, absorbablePulseColor, pulse * 0.6f);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        bool isInRange = distanceToPlayer <= absorbRange;

        if (isLowHP && isInRange)
        {
            string skillName = (skillToDrop != null && !string.IsNullOrEmpty(skillToDrop.skillName))
                ? skillToDrop.skillName
                : "Essence";
            string label = $"Absorb <color=#FFD700>{skillName}</color>";

            InteractPromptUI.Instance?.Register(this, label, StartAbsorbFromPrompt, SetTargetHighlight, "F");
        }
        else
        {
            SetTargetHighlight(false);
            InteractPromptUI.Instance?.Unregister(this);
        }
    }

    private void SetTargetHighlight(bool active)
    {
        if (isHighlighted == active) return;
        isHighlighted = active;

        if (highlightEffect != null)
        {
            if (active) highlightEffect.TurnOnHighlight();
            else highlightEffect.TurnOffHighlight();
        }
    }

    private Color GetWhipColor()
    {
        if (playerComp != null && playerComp.playerData != null)
        {
            return playerComp.playerData.whipBindColor;
        }
        return DefaultWhipColor;
    }

    private void StartAbsorbFromPrompt()
    {
        if (isAbsorbing) return;
        SetTargetHighlight(false);
        InteractPromptUI.Instance?.Unregister(this);
        StartCoroutine(AbsorbBindRoutine());
    }

    private IEnumerator AbsorbBindRoutine()
    {
        isAbsorbing = true;
        InteractPromptUI.Instance?.Unregister(this);
        if (enemySpriteRenderer != null) enemySpriteRenderer.color = originalSpriteColor;

        if (enemyScript != null)
        {
            enemyScript.DisableAllAttackHitboxes();
            enemyScript.enabled = false;
        }
        if (enemyScript != null && enemyScript.anim != null)
        {
            enemyScript.anim.speed = 0f;
        }

        Rigidbody2D enemyRb = GetComponent<Rigidbody2D>();
        if (enemyRb != null) enemyRb.linearVelocity = Vector2.zero;

        if (playerComp != null)
        {
            float dirToEnemy = transform.position.x - playerTransform.position.x;
            if (dirToEnemy != 0) playerComp.CheckIfShouldFlip(Mathf.Sign(dirToEnemy));
            playerComp.RB.linearVelocity = Vector2.zero;
            playerComp.enabled = false;
        }

        Bounds spriteBounds = (enemySpriteRenderer != null)
            ? enemySpriteRenderer.bounds
            : new Bounds(transform.position + Vector3.up * 0.4f, new Vector3(0.8f, 0.8f, 1f));

        Vector3 baseCenterOffset = spriteBounds.center - transform.position;
        float halfWidth = Mathf.Max(0.25f, spriteBounds.extents.x * 0.85f);
        float bandSpacing = Mathf.Clamp(spriteBounds.extents.y * 0.25f, 0.04f, 0.12f);

        Color activeWhipColor = GetWhipColor();

        GameObject whipLineObj = new GameObject("WhipBindLine_VFX");
        LineRenderer lr = whipLineObj.AddComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = activeWhipColor;
        lr.endColor = activeWhipColor;
        lr.startWidth = whipWidth;
        lr.endWidth = whipWidth * 0.85f;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;

        if (enemySpriteRenderer != null)
        {
            lr.sortingLayerID = enemySpriteRenderer.sortingLayerID;
            lr.sortingOrder = enemySpriteRenderer.sortingOrder + 2;
        }
        else
        {
            lr.sortingOrder = 20;
        }

        int ropeSegments = 8;
        int pointsPerBand = 12;
        int wrapSegments = bandCount * pointsPerBand;
        lr.positionCount = ropeSegments + wrapSegments;

        Vector3 originalVisualScale = enemyVisual.localScale;
        Vector3 originalVisualLocalPos = enemyVisual.localPosition;
        Quaternion originalVisualLocalRot = enemyVisual.localRotation;
        HitFlash enemyFlash = GetComponentInChildren<HitFlash>();

        float elapsed = 0f;
        float nextFlashTime = 0.15f;

        while (elapsed < bindDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / bindDuration);

            float wave = Mathf.Sin(elapsed * jellyWobbleSpeed);
            float secondaryWave = Mathf.Cos(elapsed * jellyWobbleSpeed * 0.7f);

            float constrictX = Mathf.Lerp(1f, 0.72f, progress);
            float bulgeY = Mathf.Lerp(1f, 1.28f, progress);

            float currentScaleX = originalVisualScale.x * (constrictX + wave * jellyScaleAmount * (1f - progress * 0.3f));
            float currentScaleY = originalVisualScale.y * (bulgeY - wave * jellyScaleAmount);
            enemyVisual.localScale = new Vector3(currentScaleX, currentScaleY, originalVisualScale.z);

            float tiltZ = wave * struggleTiltAngle * (0.6f + progress * 0.4f);
            enemyVisual.localRotation = originalVisualLocalRot * Quaternion.Euler(0f, 0f, tiltZ);

            float shiftX = wave * struggleShiftX;
            float shiftY = Mathf.Abs(secondaryWave) * 0.03f;
            enemyVisual.localPosition = originalVisualLocalPos + new Vector3(shiftX, shiftY, 0f);

            float handHeightOffset = (playerComp != null) ? -0.1f : 0f;
            Vector3 handPos = playerTransform != null
                ? playerTransform.position + new Vector3(playerComp != null ? playerComp.FacingDirection * 0.35f : 0f, handHeightOffset, 0f)
                : transform.position;

            Vector3 waistCenter = transform.position + baseCenterOffset + new Vector3(shiftX, shiftY, 0f);

            float extendT = Mathf.Clamp01(progress / 0.12f);
            Vector3 currentTarget = Vector3.Lerp(handPos, waistCenter, extendT);

            for (int i = 0; i < ropeSegments; i++)
            {
                float t = (float)i / (ropeSegments - 1);
                Vector3 pos = Vector3.Lerp(handPos, currentTarget, t);

                float sag = Mathf.Sin(t * Mathf.PI) * Mathf.Lerp(0.08f, 0.01f, progress);
                float tensionVibration = Mathf.Sin(t * Mathf.PI) * wave * 0.025f;
                pos.y += -sag + tensionVibration;

                lr.SetPosition(i, pos);
            }

            float currentBandRadiusX = halfWidth * (constrictX + wave * jellyScaleAmount * 0.5f) * extendT;
            float currentBandRadiusY = 0.045f * extendT;

            for (int b = 0; b < bandCount; b++)
            {
                float bandOffsetY = (b - (bandCount - 1) * 0.5f) * bandSpacing;

                for (int p = 0; p < pointsPerBand; p++)
                {
                    float pt = (float)p / (pointsPerBand - 1);
                    float angle = pt * Mathf.PI * 2f;

                    Vector3 localBandPos = new Vector3(
                        Mathf.Cos(angle) * currentBandRadiusX,
                        bandOffsetY + Mathf.Sin(angle) * currentBandRadiusY,
                        0f
                    );

                    Vector3 rotatedBandPos = Quaternion.Euler(0f, 0f, tiltZ) * localBandPos;
                    lr.SetPosition(ropeSegments + (b * pointsPerBand) + p, currentTarget + rotatedBandPos);
                }
            }

            if (elapsed >= nextFlashTime)
            {
                enemyFlash?.TriggerFlash();
                nextFlashTime = elapsed + Mathf.Lerp(0.25f, 0.1f, progress);
            }

            yield return null;
        }

        if (whipLineObj != null) Destroy(whipLineObj);

        if (enemyVisual != null)
        {
            enemyVisual.localScale = originalVisualScale;
            enemyVisual.localPosition = originalVisualLocalPos;
            enemyVisual.localRotation = originalVisualLocalRot;
        }
        if (enemyScript != null && enemyScript.anim != null)
        {
            enemyScript.anim.speed = 1f;
        }

        if (playerComp != null)
        {
            playerComp.enabled = true;
            playerComp.StateMachine?.ChangeState(playerComp.IdleState);
        }

        if (GameFeelManager.Instance != null)
        {
            GameFeelManager.Instance.TriggerHitstop(0.08f);
            if (Camera.main != null)
                GameFeelManager.Instance.TriggerShake(0.15f, 0.12f, Camera.main.transform);
        }

        ExecuteAbsorb();
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