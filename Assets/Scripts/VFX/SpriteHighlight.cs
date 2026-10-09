using UnityEngine;

public class SpriteHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [Tooltip("The target SpriteRenderer to apply the highlight to.")]
    public SpriteRenderer targetSprite;

    [Tooltip("The default material of the sprite (usually Sprite-Lit-Default).")]
    public Material defaultMaterial;

    [Tooltip("The material with the white outline shader applied.")]
    public Material outlineMaterial;

    private void Reset()
    {
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
        if (targetSprite != null && defaultMaterial == null) defaultMaterial = targetSprite.sharedMaterial;
    }

    public void TurnOnHighlight()
    {
        if (targetSprite != null && outlineMaterial != null)
        {
            targetSprite.material = outlineMaterial;
        }
    }

    public void TurnOffHighlight()
    {
        if (targetSprite != null && defaultMaterial != null)
        {
            targetSprite.material = defaultMaterial;
        }
    }
}