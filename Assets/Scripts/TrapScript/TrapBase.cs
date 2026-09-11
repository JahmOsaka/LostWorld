using UnityEngine;
using System.Collections;

public abstract class TrapBase : MonoBehaviour
{
    [Header("Trap Timing")]
    public float triggerDelay = 0.5f;
    public float resetDelay = 1.5f;

    protected Animator anim;
    protected Collider2D playerCollider;
    private bool isActive = true;

    protected virtual void Awake()
    {
        anim = GetComponentInChildren<Animator>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            isActive = false;
            playerCollider = other;
            StartCoroutine(TriggerSequence());
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other == playerCollider)
        {
            playerCollider = null;
        }
    }

    private IEnumerator TriggerSequence()
    {
        yield return new WaitForSeconds(triggerDelay);

        OnSnap();

        yield return new WaitForSeconds(resetDelay);

        OnReset();
        isActive = true;
    }

    protected abstract void OnSnap();
    protected abstract void OnReset();
}