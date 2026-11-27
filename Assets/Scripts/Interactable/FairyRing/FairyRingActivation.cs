using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class FairyRingActivation : MonoBehaviour
{
    [Header("Fairy Ring State")]
    public string checkpointId = Guid.NewGuid().ToString();
    public bool isActivated = false;

    [Header("Interact HUD Prompt")]
    [SerializeField] private string promptKey = "F";
    [SerializeField] private string activateLabel = "Activate Fairy Ring";
    [SerializeField] private string restLabel = "Rest at Fairy Ring";

    public static event Action<FairyRingActivation, GameObject> OnFairyRingInteracted;
    public static event Action<GameObject> OnPlayerLeftFairyRing;
    public event Action<bool> OnPlayerProximityChanged;
    public event Action<bool> OnFocusChanged;
    public event Action OnActivated;

    private bool isPlayerInRange = false;
    private GameObject currentPlayer;

    private float interactCooldown = 0.5f;
    private float lastInteractTime = -1f;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 1.5f;
        }
    }

    private void OnDisable()
    {
        if (isPlayerInRange)
        {
            isPlayerInRange = false;
            currentPlayer = null;
            OnFocusChanged?.Invoke(false);
            OnPlayerProximityChanged?.Invoke(false);
        }

        InteractPromptUI.Instance?.Unregister(this);
    }

    private void HandlePromptInteract()
    {
        if (!isPlayerInRange || Time.time <= lastInteractTime + interactCooldown) return;

        lastInteractTime = Time.time;
        Interact();
    }

    private void HandleFocusChanged(bool isFocused)
    {
        OnFocusChanged?.Invoke(isFocused);
    }

    private void Interact()
    {
        if (!isActivated)
        {
            isActivated = true;
            OnActivated?.Invoke();
            RegisterPrompt();
        }

        Debug.Log("FairyRing: Triggering Interaction Events");
        OnFairyRingInteracted?.Invoke(this, currentPlayer);
    }

    private void RegisterPrompt()
    {
        if (!isPlayerInRange) return;

        string label = isActivated ? restLabel : activateLabel;
        InteractPromptUI.Instance?.Register(this, label, HandlePromptInteract, HandleFocusChanged, promptKey);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPlayerInRange)
        {
            currentPlayer = collision.transform.root.gameObject;
            isPlayerInRange = true;

            OnPlayerProximityChanged?.Invoke(true);
            RegisterPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && isPlayerInRange)
        {
            if (collision.transform.root.gameObject == currentPlayer)
            {
                OnPlayerLeftFairyRing?.Invoke(currentPlayer);

                isPlayerInRange = false;
                currentPlayer = null;

                InteractPromptUI.Instance?.Unregister(this);
                OnFocusChanged?.Invoke(false);
                OnPlayerProximityChanged?.Invoke(false);
            }
        }
    }
}