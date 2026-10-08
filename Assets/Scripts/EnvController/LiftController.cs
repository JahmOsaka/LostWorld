using UnityEngine;

public class LiftController : MonoBehaviour
{
    [Header("Lift Settings")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    [Tooltip("Destination wait time (S)")]
    public float waitTime = 1.5f;

    private Vector3 targetPos;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    private void Start()
    {
        targetPos = pointB.position;
    }

    private void FixedUpdate()
    {
        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                targetPos = (targetPos == pointA.position) ? pointB.position : pointA.position;
            }

            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.01f)
        {
            isWaiting = true;
            waitTimer = waitTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!gameObject.activeInHierarchy || !collision.gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}