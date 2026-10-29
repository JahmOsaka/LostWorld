using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private Player player;
    private float jumpInputStartTime;
    private float dodgeInputStartTime;
    private float attackInputStartTime;

    public bool WheelMenuInput { get; private set; }
    public Vector2 MouseWheelInput { get; private set; }
    public Vector2 RawMovementInput { get; private set; }
    public float LookAxis { get; private set; }
    public bool JumpInput { get; private set; }
    public bool IsJumpHolding { get; private set; }
    public bool DodgeInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool AttackInput { get; private set; }
    public bool PotionInput { get; private set; }
    public bool SkillInput { get; private set; }

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Start()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer != -1 && enemyLayer != -1)
        {
            Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
        }
    }

    private void Update()
    {
        float jBuffer = (player != null && player.playerData != null) ? player.playerData.jumpBufferTime : 0.15f;
        float dBuffer = (player != null && player.playerData != null) ? player.playerData.dodgeBufferTime : 0.15f;
        float aBuffer = (player != null && player.playerData != null) ? player.playerData.attackBufferTime : 0.15f;

        if (JumpInput && Time.time >= jumpInputStartTime + jBuffer)
            JumpInput = false;

        if (DodgeInput && Time.time >= dodgeInputStartTime + dBuffer)
            DodgeInput = false;

        if (AttackInput && Time.time >= attackInputStartTime + aBuffer)
            AttackInput = false;
    }

    public void OnMoveInput(InputAction.CallbackContext context) => RawMovementInput = context.ReadValue<Vector2>();

    public void OnLookCamera(InputAction.CallbackContext context)
    {
        LookAxis = context.canceled ? 0f : context.ReadValue<float>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            JumpInput = true;
            IsJumpHolding = true;
            jumpInputStartTime = Time.time;
        }
        if (context.canceled)
        {
            IsJumpHolding = false;
        }
    }

    public void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            DodgeInput = true;
            dodgeInputStartTime = Time.time;
        }
    }

    public void OnAttackInput(InputAction.CallbackContext context) 
    {
        if (context.started)
        {
            AttackInput = true;
            attackInputStartTime = Time.time;
        }
    }
    public void OnInteract(InputAction.CallbackContext context) { if (context.started) InteractInput = true; }
    public void OnPotionInput(InputAction.CallbackContext context) { if (context.started) PotionInput = true; }
    public void OnSkillInput(InputAction.CallbackContext context) { if (context.started) SkillInput = true; }

    public void OnWheelMenuInput(InputAction.CallbackContext context)
    {
        if (context.started) WheelMenuInput = true;
        if (context.canceled) WheelMenuInput = false;
    }

    public void OnMouseSelectInput(InputAction.CallbackContext context) => MouseWheelInput = context.ReadValue<Vector2>();

    public void UseInteractInput() => InteractInput = false;
    public void UseJumpInput() => JumpInput = false;
    public void UseDodgeInput() => DodgeInput = false;
    public void UseAttackInput() => AttackInput = false;
    public void UsePotionInput() => PotionInput = false;
    public void UseSkillInput() => SkillInput = false;
}