using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class LiftCallButton : MonoBehaviour
{
    public enum TargetFloor { PointA, PointB }

    [Header("References")]
    [SerializeField] private LiftController targetLift;
    [SerializeField] private TargetFloor floor = TargetFloor.PointA;
    [SerializeField] private InputActionReference interactAction;

    private bool isPlayerInRange;
    private Vector3 promptBaseScale = Vector3.one;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteractPerformed;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteractPerformed;
        }
        InteractPromptUI.Instance?.Hide(this);
    }

    private void Update()
    {
        if (InteractPromptUI.Instance == null) return;

        if (CanCall())
        {
            InteractPromptUI.Instance.Show(this, transform, "Call Elevator", "F", new Vector3(0f, 1.4f, 0f));
        }
        else
        {
            InteractPromptUI.Instance.Hide(this);
        }
    }

    private bool CanCall()
    {
        if (!isPlayerInRange || targetLift == null || targetLift.IsMoving) return false;

        return (floor == TargetFloor.PointA && targetLift.CurrentState != LiftController.LiftState.AtA) ||
               (floor == TargetFloor.PointB && targetLift.CurrentState != LiftController.LiftState.AtB);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!CanCall()) return;

        if (floor == TargetFloor.PointA) targetLift.CallToPointA();
        else targetLift.CallToPointB();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerInRange = false;
    }
}