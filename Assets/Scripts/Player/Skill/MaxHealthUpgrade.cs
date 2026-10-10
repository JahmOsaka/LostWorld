using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MaxHealthUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [Tooltip("The amount of maximum HP to increase when this item is collected.")]
    public int bonusMaxHP = 1;

    [Header("Effects (Optional)")]
    [Tooltip("The visual effect prefab to spawn when the item is collected.")]
    public GameObject collectVFX;

    private void Reset()
    {
        CircleCollider2D col = GetComponent<CircleCollider2D>();
        if (col != null)
        {
            col.isTrigger = true;
            col.radius = 0.5f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CoreHealth health = collision.GetComponent<CoreHealth>();

            if (health == null) health = collision.GetComponentInParent<CoreHealth>();
            if (health == null) health = collision.GetComponentInChildren<CoreHealth>();

            if (health != null)
            {
                health.UpgradeMaxHealth(bonusMaxHP);

                if (collectVFX != null)
                {
                    Instantiate(collectVFX, transform.position, Quaternion.identity);
                }

                Destroy(gameObject);
            }
        }
    }
}