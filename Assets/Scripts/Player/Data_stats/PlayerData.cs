using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject
{
    [Header("1. Stats & Resources")]
    [Tooltip("Maximum energy/mana for using skills.")]
    public float maxEnergy = 100f;
    [Tooltip("Maximum number of healing potions carried.")]
    public int maxPotions = 2;
    [Tooltip("Health restored per potion used.")]
    public int potionHealAmount = 30;

    [Space(10)]
    [Header("2. Movement Settings")]
    [Tooltip("Maximum horizontal movement speed.")]
    [Range(1f, 20f)] public float moveSpeed = 8f;

    [Space(10)]
    [Header("3. Jump & Landing Settings")]
    [Tooltip("Default gravity scale for normal movement and falling.")]
    public float defaultGravity = 2.5f;
    [Tooltip("Initial vertical velocity when jumping.")]
    public float maxJumpVelocity = 15f;
    [Tooltip("Velocity cut multiplier when releasing jump early.")]
    [Range(0.1f, 1f)] public float minJumpMultiplier = 0.5f;
    [Tooltip("Vertical velocity threshold to trigger Fall State.")]
    public float fallVelocityThreshold = -12f;
    [Tooltip("Maximum falling speed.")]
    public float maxFallSpeed = 20f;
    [Tooltip("Fall duration (seconds) required to trigger Hard Landing.")]
    public float hardLandingThreshold = 0.6f;
    public float softLandingAnimSpeed = 3f;
    public float hardLandingAnimSpeed = 1f;

    [Space(10)]
    [Header("4. Input Assists & Buffers")]
    [Tooltip("Grace period (seconds) to jump after walking off a ledge.")]
    public float coyoteTime = 0.1f;
    [Tooltip("Input buffer window (seconds) for Jump before landing.")]
    public float jumpBufferTime = 0.15f;
    [Tooltip("Input buffer window (seconds) for Dodge before landing.")]
    public float dodgeBufferTime = 0.15f;
    [Tooltip("Input buffer window (seconds) for Attack before landing.")]
    public float attackBufferTime = 0.15f;

    [Space(10)]
    [Header("5. Dodge Settings")]
    [SerializeField] private float dodgeSpeed = 12f;
    [SerializeField] private float dodgeDuration = 0.4f;
    [SerializeField] private float dodgeCooldown = 1f;

    [Space(10)]
    [Header("6. Combat Settings")]
    public int attackDamage = 10;
    public float comboWindow = 0.2f;
    [SerializeField] private float attackSpeedMultiplier = 1f;
    [Tooltip("Duration (seconds) to stay in Combat Idle after attacking.")]
    public float combatIdleDuration = 4f;

    [Space(10)]
    [Header("7. Dynamic Collider & Hurtbox Sizes")]
    [Tooltip("Horizontal push speed when stuck under a low ceiling.")]
    public float autoEjectSpeed = 7f;
    public Vector2 jumpHurtboxSize = new Vector2(0.6f, 1.0f);
    public Vector2 jumpHurtboxOffset = new Vector2(0f, -0.3f);
    public Vector2 dodgeHurtboxSize = new Vector2(0.8f, 0.7f);
    public Vector2 dodgeHurtboxOffset = new Vector2(0f, -0.5f);

    [Space(10)]
    [Header("8. Ground & Ceiling Detection")]
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public Vector2 ceilingCheckSize = new Vector2(0.6f, 0.1f);
    public float boxCastDistance = 0.1f;

    public float DodgeSpeed => dodgeSpeed;
    public float DodgeDuration => dodgeDuration;
    public float DodgeCooldown => dodgeCooldown;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
}