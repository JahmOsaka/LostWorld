using System;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public static event Action OnItemCollected;

    [Header("Pickup Notification")]
    [SerializeField] private string itemName = "Quest Item";
    [SerializeField] private int amount = 1;
    [Tooltip("Optional. If left empty, uses the SpriteRenderer on this object.")]
    [SerializeField] private Sprite itemIcon;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnItemCollected?.Invoke();

            Sprite iconToShow = itemIcon != null ? itemIcon : GetComponentInChildren<SpriteRenderer>()?.sprite;
            PickupNotificationUI.Instance?.ShowItemObtained(itemName, iconToShow, amount);

            Destroy(gameObject);
        }
    }
}