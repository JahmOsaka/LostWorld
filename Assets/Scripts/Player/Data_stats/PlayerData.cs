using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject
{
    [Header("1. Stats & Resources")]
    public float maxEnergy = 100f;
    public float energyRegenPerWhipHit = 10f;
    public float maxEnergyPerCollectable = 20f;
    public int maxPotions = 2;
    public int potionHealAmount = 30;

    [Space(10)]
    [Header("2. Movement Settings")]
    [Range(1f, 20f)] public float moveSpeed = 8f;

    [Space(10)]
    [Header("3. Jump & Landing Settings")]
    public float defaultGravity = 2.5f;
    public float minJumpHoldTime = 0.08f;
    public float maxJumpHeight = 3f;
    public float jumpApexTime = 0.25f;
    [Range(0.1f, 1f)] public float minJumpMultiplier = 0.5f;
    public float fallVelocityThreshold = -0.1f;
    public float maxFallSpeed = 20f;
    public float hardLandingThreshold = 0.6f;
    public float softLandingAnimSpeed = 3f;
    public float hardLandingAnimSpeed = 1f;
    public float softLandingExitTime = 0.1f;
    public float hardLandingExitTime = 0.4f;

    [Space(10)]
    [Header("4. Input Assists & Buffers")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.15f;
    public float dodgeBufferTime = 0.15f;
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
    public float combatIdleDuration = 4f;

    [Space(10)]
    [Header("7. Combat Game Feel & Juice")]
    public Color whipBindColor = new Color32(0x43, 0x25, 0x23, 0xFF);
    public float attackPushSpeed = 3f;
    public float hitstopHit1 = 0.04f;
    public float hitstopHit2 = 0.05f;
    public float hitstopHit3 = 0.12f;
    public float screenShakeDuration = 0.15f;
    public float screenShakeMagnitude = 0.1f;

    [Header("Air Attack (Wide Arc Slash)")]
    public float airAttackGravityScale = 1.2f;
    public float airAttackMoveSpeed = 3.5f;
    public float airAttackDuration = 0.35f;

    [Space(10)]
    [Header("8. Dynamic Collider & Hurtbox Sizes")]
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