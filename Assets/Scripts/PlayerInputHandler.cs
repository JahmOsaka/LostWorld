using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public Vector2 RawMovementInput { get; private set; }
    public bool JumpInput { get; private set; }
    public bool DodgeInput { get; private set; }

    public void OnMoveInput(InputAction.CallbackContext context)
    {
        RawMovementInput = context.ReadValue<Vector2>();
    }

    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if (context.started) JumpInput = true;
        if (context.canceled) JumpInput = false;
    }

    public void OnDodgeInput(InputAction.CallbackContext context)
    {
        if (context.started) DodgeInput = true;
        if (context.canceled) DodgeInput = false;
    }

    public void UseJumpInput() => JumpInput = false;
    public void UseDodgeInput() => DodgeInput = false;
}