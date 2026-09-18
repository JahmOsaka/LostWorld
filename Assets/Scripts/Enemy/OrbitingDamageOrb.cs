using UnityEngine;

public class OrbitingDamageOrb : MonoBehaviour
{
    private Transform center;
    private float radius;
    private float angularSpeed;
    private float currentAngle;
    private float duration;
    private int damage;
    private float hitCooldown;
    private const float HitInterval = 0.5f;

    public void Init(Transform centerTransform, float orbitRadius, float speedDeg, float startAngleDeg, float lifeDuration, int dmg)
    {
        center = centerTransform;
        radius = orbitRadius;
        angularSpeed = speedDeg;
        currentAngle = startAngleDeg;
        duration = lifeDuration;
        damage = dmg;
    }

    private void Update()
    {
        if (center == null) { Destroy(gameObject); return; }

        currentAngle += angularSpeed * Time.deltaTime;
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        transform.position = center.position + (Vector3)offset;

        duration -= Time.deltaTime;
        if (duration <= 0f) Destroy(gameObject);

        if (hitCooldown > 0f) hitCooldown -= Time.deltaTime;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && hitCooldown <= 0f)
        {
            other.GetComponent<IDamageable>()?.TakeDamage(damage);
            hitCooldown = HitInterval;
        }
    }
}