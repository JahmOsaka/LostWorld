using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public Vector2 parallaxEffectMultiplier;

    [Header("Infinite Loop Settings")]
    public bool infiniteLoop = true;
    public float layerWidth = 20f; 

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y, 0);
        lastCameraPosition = cameraTransform.position;

        if (infiniteLoop)
        {
            if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= layerWidth)
            {
                float offsetPositionX = (cameraTransform.position.x - transform.position.x) % layerWidth;
                transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y, transform.position.z);
            }
        }
    }
}