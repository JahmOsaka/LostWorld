using UnityEngine;

public class CameraProxy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Rigidbody2D playerRb;
    public PlayerInputHandler inputHandler;

    [Header("Camera Smoothness (Lower = Faster)")]
    [Tooltip("Smooth time during normal movement or jumping (0.2 to 0.3 recommended).")]
    public float normalSmoothTime = 0.25f;
    [Tooltip("Smooth time when falling (0.05 recommended for tight tracking).")]
    public float fallSmoothTime = 0.05f;

    [Header("Look Up/Down")]
    public float panDistance = 4f;

    private float yVelocity = 0.0f;
    private float currentSmoothTime;

    private void Start()
    {
        transform.parent = null;
        currentSmoothTime = normalSmoothTime;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        float targetX = player.position.x;
        float targetY = player.position.y + (inputHandler.LookAxis * panDistance);

        float targetSmoothTime = (playerRb.linearVelocity.y < -1f) ? fallSmoothTime : normalSmoothTime;
        currentSmoothTime = Mathf.MoveTowards(currentSmoothTime, targetSmoothTime, Time.deltaTime * 2f);

        float newY = Mathf.SmoothDamp(transform.position.y, targetY, ref yVelocity, currentSmoothTime);

        transform.position = new Vector3(targetX, newY, transform.position.z);
    }
}