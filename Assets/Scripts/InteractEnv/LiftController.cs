using UnityEngine;

public class LiftController : MonoBehaviour
{
    [Header("Lift Settings")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private bool isPlayerOnPlatform = false;

    private enum LiftState { WaitingAtA, MovingToB, WaitingAtB, MovingToA }
    private LiftState currentState = LiftState.WaitingAtA;

    private void Start()
    {
        transform.position = pointA.position;
        currentState = LiftState.WaitingAtA;
    }

    private void FixedUpdate()
    {
        switch (currentState)
        {
            case LiftState.WaitingAtA:
                if (isPlayerOnPlatform)
                {
                    currentState = LiftState.MovingToB;
                }
                break;

            case LiftState.MovingToB:
                transform.position = Vector3.MoveTowards(transform.position, pointB.position, speed * Time.fixedDeltaTime);

                if (Vector3.Distance(transform.position, pointB.position) < 0.01f)
                {
                    if (isPlayerOnPlatform)
                    {
                        currentState = LiftState.WaitingAtB;
                    }
                    else
                    {
                        currentState = LiftState.MovingToA;
                    }
                }
                break;

            case LiftState.WaitingAtB:
                if (!isPlayerOnPlatform)
                {
                    currentState = LiftState.MovingToA;
                }
                break;

            case LiftState.MovingToA:
                transform.position = Vector3.MoveTowards(transform.position, pointA.position, speed * Time.fixedDeltaTime);

                if (Vector3.Distance(transform.position, pointA.position) < 0.01f)
                {
                    currentState = LiftState.WaitingAtA;
                }
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnPlatform = true;
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerOnPlatform = false;

            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}