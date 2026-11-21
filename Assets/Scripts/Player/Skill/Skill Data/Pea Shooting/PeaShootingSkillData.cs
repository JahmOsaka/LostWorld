using UnityEngine;

[CreateAssetMenu(fileName = "NewPeaShootingSkill", menuName = "Data/Skills/Pea Shooting")]
public class PeaShootingSkillData : ActiveSkillData
{
    [Header("Pea Shooting Settings")]
    [Tooltip("Prefab containing the PeaProjectile script, Rigidbody2D, and trigger Collider2D.")]
    public GameObject projectilePrefab;
    [Tooltip("Optional particle prefab spawned at the launch point when fired.")]
    public GameObject muzzleVfxPrefab;
    [Tooltip("Damage dealt on impact.")]
    public int damage = 15;
    [Tooltip("Initial launch velocity (X is mirrored by facing direction, Y creates the arc).")]
    public Vector2 launchVelocity = new Vector2(7f, 10f);
    [Tooltip("Gravity scale applied to the projectile to control arc drop speed.")]
    public float gravityScale = 2.5f;
    [Tooltip("Maximum lifetime in seconds before the projectile auto-destroys.")]
    public float lifetime = 3f;
    [Tooltip("Spawn offset relative to the user (X is mirrored by facing direction).")]
    public Vector2 spawnOffset = new Vector2(0.6f, 0.3f);

    [Header("Impact Feel")]
    [Tooltip("Horizontal knockback force applied to hit enemies.")]
    public float knockbackForce = 4f;
    [Tooltip("Hit flash duration applied to struck enemies.")]
    public float enemyFlashDuration = 0.08f;
    [Tooltip("Subtle camera shake duration on enemy impact.")]
    public float hitShakeDuration = 0.05f;
    [Tooltip("Subtle camera shake magnitude on enemy impact.")]
    public float hitShakeMagnitude = 0.05f;

    [Header("Collision Layers")]
    [Tooltip("Layers considered valid enemy targets (e.g., EnemyHitbox).")]
    public LayerMask enemyLayer;
    [Tooltip("Layers that destroy the projectile on contact (e.g., Ground, Walls).")]
    public LayerMask groundLayer;

    public override void ExecuteSkill(GameObject user)
    {
        if (projectilePrefab == null || user == null) return;

        Player player = user.GetComponent<Player>();
        int dir = (player != null) ? player.FacingDirection : (user.transform.localScale.x < 0f ? -1 : 1);

        Vector3 spawnPos = user.transform.position + new Vector3(spawnOffset.x * dir, spawnOffset.y, 0f);
        GameObject obj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        PeaProjectile projectile = obj.GetComponent<PeaProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(this, dir);
        }

        if (muzzleVfxPrefab != null)
        {
            float yRot = (dir < 0) ? 180f : 0f;
            GameObject muzzleFx = Instantiate(muzzleVfxPrefab, spawnPos, Quaternion.Euler(0f, yRot, 0f));
            Destroy(muzzleFx, 0.5f);
        }

        user.GetComponent<PlayerCastFeedback>()?.TriggerCast(CastFeedbackType.Throw);
    }
}