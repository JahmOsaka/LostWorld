using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class MaxHealthUpgrade : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [Tooltip("The amount of maximum HP to increase when this item is collected.")]
    public int bonusMaxHP = 1;

    [Header("Pickup Notification UI")]
    [Tooltip("Optional icon displayed in the pickup toast notification.")]
    [SerializeField] private Sprite pickupIcon;
    [Tooltip("Accent color for the notification bar and +HP text.")]
    [SerializeField] private Color accentColor = new Color(1f, 0.35f, 0.35f, 1f);
    [Tooltip("Duration in seconds the notification stays visible.")]
    [SerializeField] private float notificationDuration = 2.5f;

    [Header("Effects (Optional)")]
    [Tooltip("The visual effect prefab to spawn when the item is collected.")]
    public GameObject collectVFX;

    private bool isCollected;

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
        if (isCollected || !collision.CompareTag("Player")) return;

        CoreHealth health = collision.GetComponent<CoreHealth>();
        if (health == null) health = collision.GetComponentInParent<CoreHealth>();
        if (health == null) health = collision.GetComponentInChildren<CoreHealth>();

        if (health != null)
        {
            isCollected = true;
            health.UpgradeMaxHealth(bonusMaxHP);

            string hexColor = ColorUtility.ToHtmlStringRGB(accentColor);
            PickupNotificationUI.Instance?.Show(
                $"Max HP <color=#{hexColor}>+{bonusMaxHP}</color>",
                pickupIcon,
                accentColor,
                notificationDuration
            );

            if (collectVFX != null)
            {
                Instantiate(collectVFX, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}