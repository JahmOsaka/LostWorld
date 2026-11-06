using System;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class FairyRingActivation : MonoBehaviour
{
    [Header("Fairy Ring State")]
    public string checkpointId = Guid.NewGuid().ToString();
    public bool isActivated = false;

    public static event Action<FairyRingActivation, GameObject> OnFairyRingInteracted;
    public static event Action<GameObject> OnPlayerLeftFairyRing;
    public event Action<bool> OnPlayerProximityChanged;
    public event Action OnActivated;

    private bool isPlayerInRange = false;
    private PlayerInputHandler playerInput;
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

    private void Update()
    {
        if (isPlayerInRange && playerInput != null && Time.time > lastInteractTime + interactCooldown)
        {
            if (playerInput.InteractInput)
            {
                playerInput.UseInteractInput();
                lastInteractTime = Time.time;
                Interact();
            }
        }
    }

    private void Interact()
    {
        if (!isActivated)
        {
            isActivated = true;
            OnActivated?.Invoke();
        }

        Debug.Log("FairyRing: Triggering Interaction Events");
        OnFairyRingInteracted?.Invoke(this, currentPlayer);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isPlayerInRange)
        {
            currentPlayer = collision.transform.root.gameObject;

            playerInput = currentPlayer.GetComponentInChildren<PlayerInputHandler>();

            if (playerInput != null)
            {
                isPlayerInRange = true;
                OnPlayerProximityChanged?.Invoke(true);
            }
            else
            {
                Debug.LogWarning("FairyRing: Found Player tag but no PlayerInputHandler!");
            }
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
                playerInput = null;
                currentPlayer = null;

                OnPlayerProximityChanged?.Invoke(false);
            }
        }
    }
}