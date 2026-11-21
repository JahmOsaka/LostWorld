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
    public float minJumpHoldTime = 0.08f;
    [Tooltip("The maximum height (Unity units) required to jump when the button is fully pressed and held.")]
    public float maxJumpHeight = 3f;
    [Tooltip("Time (in seconds) to reach the top when the button is pressed and held.")]
    public float jumpApexTime = 0.25f;
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
    public float softLandingExitTime = 0.1f;
    public float hardLandingExitTime = 0.4f;

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
    [SerializeField] private float dodgeCooldown = 0.4f;
    [SerializeField] private float dodgeIFrameDuration = -1f;
    public float superDashSpeed = 18f;

    [Space(10)]
    [Header("6. Combat Settings")]
    public float attackDuration = 0.5f;
    public int attackDamage = 10;
    public float comboWindow = 0.2f;
    [SerializeField] private float attackSpeedMultiplier = 1f;
    [Tooltip("Duration (seconds) to stay in Combat Idle after attacking.")]
    public float combatIdleDuration = 4f;

    [Space(10)]
    [Header("7. Combat Game Feel & Juice")]
    [Tooltip("Color of the whip line during skill absorption (#432523).")]
    public Color whipBindColor = new Color32(0x43, 0x25, 0x23, 0xFF);
    [Tooltip("Forward push speed applied when executing a whip attack.")]
    public float attackPushSpeed = 3f;
    [Tooltip("Hitstop duration (seconds) for Combo Hit 1.")]
    public float hitstopHit1 = 0.04f;
    [Tooltip("Hitstop duration (seconds) for Combo Hit 2.")]
    public float hitstopHit2 = 0.05f;
    [Tooltip("Hitstop duration (seconds) for Combo Hit 3 (Finisher).")]
    public float hitstopHit3 = 0.12f;
    [Tooltip("Screen shake duration (seconds) for Combo Hit 3.")]
    public float screenShakeDuration = 0.15f;
    [Tooltip("Screen shake magnitude (intensity) for Combo Hit 3.")]
    public float screenShakeMagnitude = 0.1f;
    [Header("Air Attack (Wide Arc Slash)")]
    [Tooltip("Gravity scale applied while executing an air attack for a brief hang-time feel.")]
    public float airAttackGravityScale = 1.2f;
    [Tooltip("Horizontal air drift speed allowed during an air attack.")]
    public float airAttackMoveSpeed = 3.5f;
    [Tooltip("Fallback duration if the animation event fails to fire.")]
    public float airAttackDuration = 0.35f;

    [Space(10)]
    [Header("8. Dynamic Collider & Hurtbox Sizes")]
    [Tooltip("Horizontal push speed when stuck under a low ceiling.")]
    public float autoEjectSpeed = 7f;
    public Vector2 jumpHurtboxSize = new Vector2(0.6f, 1.0f);
    public Vector2 jumpHurtboxOffset = new Vector2(0f, -0.3f);
    public Vector2 dodgeHurtboxSize = new Vector2(0.8f, 0.7f);
    public Vector2 dodgeHurtboxOffset = new Vector2(0f, -0.5f);

    [Space(10)]
    [Header("9. Ground & Ceiling Detection")]
    public Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    public Vector2 ceilingCheckSize = new Vector2(0.6f, 0.1f);
    public float boxCastDistance = 0.1f;

    public float DodgeSpeed => dodgeSpeed;
    public float DodgeDuration => dodgeDuration;
    public float DodgeCooldown => dodgeCooldown;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
    public float MaxJumpVelocity => 2f * maxJumpHeight / jumpApexTime;
    public float JumpGravityScale => (2f * maxJumpHeight / (jumpApexTime * jumpApexTime)) / Physics2D.gravity.magnitude;
    public float DodgeIFrameDuration => dodgeIFrameDuration <= 0f ? dodgeDuration : dodgeIFrameDuration;
}