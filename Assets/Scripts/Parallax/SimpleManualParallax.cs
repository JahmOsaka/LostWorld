using UnityEngine;

public class SimpleManualParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("Scene delay: 1 = Moves precisely with the camera, 0 = Remains stationary.")]
    public float parallaxMultiplier = 0.5f;

    private Transform cameraTransform;
    private float startPosX;
    private float startCameraX;
    private float fixedPosY;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            startCameraX = cameraTransform.position.x;
        }

        startPosX = transform.position.x;
        fixedPosY = transform.position.y;
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        float distanceX = (cameraTransform.position.x - startCameraX) * parallaxMultiplier;

        transform.position = new Vector3(startPosX + distanceX, fixedPosY, transform.position.z);
    }
}