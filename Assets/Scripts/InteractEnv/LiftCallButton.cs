using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LiftCallButton : MonoBehaviour, IInteractable
{
    public enum TargetFloor { PointA, PointB }

    [Header("References")]
    [SerializeField] private LiftController targetLift;
    [SerializeField] private TargetFloor floor = TargetFloor.PointA;

    [Header("Interact HUD Prompt")]
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.4f, 0f);
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string callLabel = "Call the elevator";

    private bool isPromptShowing;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnDisable()
    {
        isPromptShowing = false;
        InteractPromptUI.Instance?.Hide(this);
    }

    private void Update()
    {
        if (!isPromptShowing) return;

        if (CanCall())
        {
            InteractPromptUI.Instance?.Show(this, transform, callLabel, promptKey, promptOffset);
        }
        else
        {
            InteractPromptUI.Instance?.Hide(this);
        }
    }

    public void ShowInteractPrompt()
    {
        isPromptShowing = true;
        if (CanCall())
        {
            InteractPromptUI.Instance?.Show(this, transform, callLabel, promptKey, promptOffset);
        }
    }

    public void HideInteractPrompt()
    {
        isPromptShowing = false;
        InteractPromptUI.Instance?.Hide(this);
    }

    public void Interact()
    {
        if (!CanCall()) return;

        HideInteractPrompt();
        if (floor == TargetFloor.PointA) targetLift.CallToPointA();
        else targetLift.CallToPointB();
    }

    private bool CanCall()
    {
        if (targetLift == null || targetLift.IsMoving) return false;

        return (floor == TargetFloor.PointA && targetLift.CurrentState != LiftController.LiftState.AtA) ||
               (floor == TargetFloor.PointB && targetLift.CurrentState != LiftController.LiftState.AtB);
    }
}