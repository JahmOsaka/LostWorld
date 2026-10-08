using UnityEngine;

public class ShortcutGate : MonoBehaviour
{
    [Header("Gate Components")]
    [Tooltip("Visual")]
    public GameObject gateVisual;

    [Tooltip("Collider (Is Trigger)")]
    public Collider2D solidCollider;

    [Header("State")]
    public bool isUnlocked = false;

    private bool playerInGreenZone = false;
    private Player playerRef;

    private void Update()
    {
        if (!isUnlocked && playerInGreenZone && playerRef != null && playerRef.InputHandler != null)
        {
            if (playerRef.InputHandler.InteractInput)
            {
                UnlockGate();
                playerRef.InputHandler.UseInteractInput();
            }
        }
    }

    private void UnlockGate()
    {
        isUnlocked = true;

        if (gateVisual != null)
        {
            gateVisual.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Forget Gate Visual in Inspector?");
        }

        if (solidCollider != null)
        {
            solidCollider.enabled = false;
        }
        else
        {
            Debug.LogWarning("Forget Solid Collider in Inspector?");
        }

        Debug.Log("ShortcutGate Unlock!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInGreenZone = true;
            playerRef = collision.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInGreenZone = false;
            playerRef = null;
        }
    }
}