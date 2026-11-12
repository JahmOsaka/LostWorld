using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyerHead : MonoBehaviour
{
    [SerializeField] private GameObject burstEffect;

    private int damage;
    private float radius;
    private float fuseTime;
    private bool hasLanded;

    public void Init(int damage, float radius, float fuse)
    {
        this.damage = damage;
        this.radius = radius;
        this.fuseTime = fuse; 

        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(Random.Range(-1.5f, 1.5f), 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!hasLanded)
        {
            hasLanded = true;
            Invoke(nameof(Explode), fuseTime);
        }
    }

    private void Explode()
    {
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, radius))
        {
            if (!hit.CompareTag("Player")) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damage);
                break;
            }
        }

        if (burstEffect != null)
            Instantiate(burstEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, radius > 0f ? radius : 2f);
    }
}