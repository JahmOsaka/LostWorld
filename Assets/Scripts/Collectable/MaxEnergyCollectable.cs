using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MaxEnergyCollectable : MonoBehaviour
{
    [Header("Collectable Settings")]
    [Tooltip("Leave at -1 to use PlayerData.maxEnergyPerCollectable (default 20).")]
    [SerializeField] private float bonusMaxEnergyOverride = -1f;
    [SerializeField] private GameObject pickupVfxPrefab;
    [SerializeField] private float vfxLifetime = 1.2f;

    private bool isCollected;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected || !other.CompareTag("Player")) return;

        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) stats = other.transform.root.GetComponentInChildren<PlayerStats>();
        if (stats == null) return;

        isCollected = true;
        stats.CollectMaxEnergyUpgrade(bonusMaxEnergyOverride);

        if (pickupVfxPrefab != null)
        {
            GameObject vfx = Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, vfxLifetime);
        }

        Destroy(gameObject);
    }
}