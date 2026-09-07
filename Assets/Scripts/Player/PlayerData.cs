using UnityEngine;
[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Data/Player Data/Base Data")]
public class PlayerData : ScriptableObject
{
    [Header("Movement Settings")]
    [Tooltip("Maximum movement speed. No acceleration as per GDD.")]
    [Range(1f, 20f)]
    public float moveSpeed = 8f;

    [Space(10)]
    [Header("Jump Settings")]
    [Tooltip("Initial velocity when the jump starts.")]
    public float maxJumpVelocity = 15f;

    [Tooltip("Velocity multiplier when the jump button is released early (e.g., 0.5 cuts the velocity in half).")]
    [Range(0.1f, 1f)]
    public float minJumpMultiplier = 0.5f;

    [Tooltip("Maximum duration the jump button can be held down to increase height.")]
    public float maxJumpTime = 0.25f;

    [Space(10)]
    [Header("Assist Settings")]
    [Tooltip("The time window allowed to jump after walking off a ledge (in seconds).")]
    public float coyoteTime = 0.15f;

    [Space(10)]
    [Header("Dodge Settings")]
    [Tooltip("The speed of the dodge roll.")]
    [SerializeField] private float dodgeSpeed = 12f;

    [Tooltip("The duration of the dodge roll in seconds.")]
    [SerializeField] private float dodgeDuration = 0.4f;

    [Tooltip("The cooldown time before the player can dodge again.")]
    [SerializeField] private float dodgeCooldown = 1f;

    [Space(10)]
    [Header("Combat Settings")]
    public int attackDamage = 10;
    public float comboWindow = 0.2f;
    [SerializeField] private float attackSpeedMultiplier = 1f;

    // Getters for Read-only access
    public float DodgeSpeed => dodgeSpeed;
    public float DodgeDuration => dodgeDuration;
    public float DodgeCooldown => dodgeCooldown;
    public float AttackSpeedMultiplier => attackSpeedMultiplier;
}
