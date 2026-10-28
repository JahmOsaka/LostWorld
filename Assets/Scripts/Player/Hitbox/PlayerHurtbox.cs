using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerHurtbox : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private Vector2 defaultSize;
    private Vector2 defaultOffset;
    private bool isInitialized = false;

    public BoxCollider2D BoxCollider => boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        InitializeDefaults();
    }

    private void InitializeDefaults()
    {
        if (!isInitialized && boxCollider != null)
        {
            defaultSize = boxCollider.size;
            defaultOffset = boxCollider.offset;
            isInitialized = true;
        }
    }

    [Tooltip("Adjusts the size and offset of the hurtbox collider dynamically based on animations.")]
    public void SetHurtboxSize(Vector2 newSize, Vector2 newOffset)
    {
        InitializeDefaults();
        if (boxCollider != null)
        {
            boxCollider.size = newSize;
            boxCollider.offset = newOffset;
        }
    }

    [Tooltip("Restores the hurtbox collider to its original standing size and offset.")]
    public void ResetHurtbox()
    {
        if (boxCollider != null && isInitialized)
        {
            boxCollider.size = defaultSize;
            boxCollider.offset = defaultOffset;
        }
    }
}