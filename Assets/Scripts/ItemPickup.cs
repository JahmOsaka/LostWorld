using System;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public static event Action OnItemCollected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnItemCollected?.Invoke();

            Destroy(gameObject);
        }
    }
}