using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class DroppedCurrency : MonoBehaviour
{
    public int storedCurrency = 0;

    private void Reset()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                stats.RetrieveCurrency(storedCurrency);
                Debug.Log($"Retrieved {storedCurrency} currency!");


                Destroy(gameObject);
            }
        }
    }
}