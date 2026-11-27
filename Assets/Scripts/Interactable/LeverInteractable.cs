using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : MonoBehaviour, IInteractable
{
    [Header("Interact HUD Prompt")]
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string pullLabel = "Pull Lever";
    [Tooltip("The highlight effect script attached to this object.")]
    [SerializeField] private SpriteHighlight highlightEffect;

    [Header("Lever Behavior")]
    [Tooltip("If true, the lever can only be pulled once (e.g., permanently unlocking a lift or shortcut gate).")]
    [SerializeField] private bool isOneTimeOnly = true;
    [Tooltip("Initial state of the lever.")]
    [SerializeField] private bool startActivated = false;

    [Header("Visual Feedback")]
    [Tooltip("Optional lever handle Transform to rotate when toggled.")]
    [SerializeField] private Transform handlePivot;
    [Tooltip("Z rotation angle when inactive.")]
    [SerializeField] private float offAngleZ = 25f;
    [Tooltip("Z rotation angle when active.")]
    [SerializeField] private float onAngleZ = -25f;
    [Tooltip("Speed at which the lever handle rotates.")]
    [SerializeField] private float handleRotateSpeed = 12f;

    [Header("Targets")]
    [Tooltip("Components implementing IActivatable (e.g., LiftPlatform, LeverGate).")]
    [SerializeField] private List<MonoBehaviour> activatableTargets = new List<MonoBehaviour>();

    [Header("Unity Events (Optional Direct Wiring)")]
    public UnityEvent OnActivated;
    public UnityEvent OnDeactivated;

    public bool IsActivated { get; private set; }

    private void Awake()
    {
        IsActivated = startActivated;
        if (handlePivot != null)
        {
            float targetAngle = IsActivated ? onAngleZ : offAngleZ;
            handlePivot.localRotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
    }

    private void OnValidate()
    {
        for (int i = activatableTargets.Count - 1; i >= 0; i--)
        {
            if (activatableTargets[i] != null && !(activatableTargets[i] is IActivatable))
            {
                Debug.LogWarning($"[LeverInteractable] {activatableTargets[i].name} does not implement IActivatable.", this);
                activatableTargets[i] = null;
            }
        }
    }

    private void Update()
    {
        if (handlePivot == null) return;

        float targetAngle = IsActivated ? onAngleZ : offAngleZ;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
        handlePivot.localRotation = Quaternion.Lerp(handlePivot.localRotation, targetRot, Time.deltaTime * handleRotateSpeed);
    }

    private void OnDisable()
    {
        HideInteractPrompt();
    }

    public void ShowInteractPrompt()
    {
        if (isOneTimeOnly && IsActivated) return;
        InteractPromptUI.Instance?.Register(this, pullLabel, Interact, SetHighlight, promptKey);
    }

    public void HideInteractPrompt()
    {
        SetHighlight(false);
        InteractPromptUI.Instance?.Unregister(this);
    }

    public void Interact()
    {
        if (isOneTimeOnly && IsActivated) return;

        SetLeverState(!IsActivated);

        if (isOneTimeOnly && IsActivated)
        {
            HideInteractPrompt();
        }
    }
    public void SetLeverState(bool active, bool triggerEvents = true)
    {
        IsActivated = active;

        if (!triggerEvents) return;

        foreach (MonoBehaviour target in activatableTargets)
        {
            if (target is IActivatable activatable)
            {
                if (IsActivated) activatable.Activate();
                else activatable.Deactivate();
            }
        }

        if (IsActivated) OnActivated?.Invoke();
        else OnDeactivated?.Invoke();
    }

    private void SetHighlight(bool active)
    {
        if (highlightEffect == null) return;
        if (active) highlightEffect.TurnOnHighlight();
        else highlightEffect.TurnOffHighlight();
    }
}