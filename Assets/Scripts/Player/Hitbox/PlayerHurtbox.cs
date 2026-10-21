using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerHurtbox : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    public BoxCollider2D BoxCollider => boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.isTrigger = true;
        }
    }

    [Tooltip("Adjusts the size and offset of the hurtbox collider dynamically based on animations.")]
    public void SetHurtboxSize(Vector2 newSize, Vector2 newOffset)
    {
        if (boxCollider != null)
        {
            boxCollider.size = newSize;
            boxCollider.offset = newOffset;
        }
    }
}