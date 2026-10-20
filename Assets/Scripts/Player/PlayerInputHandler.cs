using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Jump Buffer Settings")]
    [Tooltip("Duration to hold the jump input active in memory before it expires.")]
    [SerializeField] private float jumpBufferTime = 0.15f;
    private float jumpInputStartTime;

    [Header("MoveMent Inputs")]
    public Vector2 RawMovementInput { get; private set; }
    public float LookAxis { get; private set; }
    public bool JumpInput { get; private set; }
    public bool IsJumpHolding { get; private set; }
    public bool DodgeInput { get; private set; }

    [Header("Interact Inputs")]
    public bool InteractInput { get; private set; }

    [Header("Combat Inputs")]
    public bool AttackInput { get; private set; }
    public bool PotionInput { get; private set; }
    public bool SkillInput { get; private set; }
    void Start()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");

        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, true);
    }
    private void Update()
    {
        CheckJumpInputBuffer();
    }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        RawMovementInput = context.ReadValue<Vector2>();
    }

    public void OnLookCamera(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            LookAxis = 0f;
        }
        else
        {
            LookAxis = context.ReadValue<float>();
        }
    }

    private void CheckJumpInputBuffer()
    {
        if (JumpInput && Time.time >= jumpInputStartTime + jumpBufferTime)
        {
            JumpInput = false;
        }
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
        if (context.started) DodgeInput = true;
        if (context.canceled) DodgeInput = false;
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if (context.started) AttackInput = true;
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            InteractInput = true;
        }
    }

    public void OnPotionInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            PotionInput = true;
        }
    }

    public void OnSkillInput(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            SkillInput = true;
        }
    }
    public void UseInteractInput() => InteractInput = false;
    public void UseJumpInput() => JumpInput = false;
    public void UseDodgeInput() => DodgeInput = false;
    public void UseAttackInput() => AttackInput = false;
    public void UsePotionInput() => PotionInput = false;
    public void UseSkillInput() => SkillInput = false;
}