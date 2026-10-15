using UnityEngine;
using Unity.Cinemachine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public PlayerInputHandler inputHandler;
    public Rigidbody2D playerRb;
    private CinemachineCamera cinemachineCamera;

    [Header("Look Up/Down")]
    public float panDistance = 4f;
    public float panSpeed = 5f;

    [Header("Dynamic Damping")]
    [Tooltip("Normal Y Damping (Higher = Smoother)")]
    public float normalDampingY = 1.5f;
    [Tooltip("Fall Y Damping (Set to 0-0.1 for instant catch-up)")]
    public float fallDampingY = 0.1f;

    private CinemachinePositionComposer positionComposer;
    private float defaultOffsetY;

    private void Start()
    {
        cinemachineCamera = FindAnyObjectByType<CinemachineCamera>();
        if (cinemachineCamera != null)
        {
            positionComposer = cinemachineCamera.GetComponent<CinemachinePositionComposer>();
            if (positionComposer != null)
            {
                defaultOffsetY = positionComposer.TargetOffset.y;
            }
        }
    }

    private void Update()
    {
        if (positionComposer == null) return;

        float targetOffsetY = defaultOffsetY + (inputHandler.LookAxis * panDistance);
        Vector3 currentOffset = positionComposer.TargetOffset;
        currentOffset.y = Mathf.Lerp(currentOffset.y, targetOffsetY, Time.deltaTime * panSpeed);
        positionComposer.TargetOffset = currentOffset;

        Vector3 currentDamping = positionComposer.Damping;
        if (playerRb.linearVelocity.y < -1f)
        {
            currentDamping.y = Mathf.MoveTowards(currentDamping.y, fallDampingY, Time.deltaTime * 10f);
        }
        else
        {
            currentDamping.y = Mathf.MoveTowards(currentDamping.y, normalDampingY, Time.deltaTime * 3f);
        }
        positionComposer.Damping = currentDamping;
    }
}