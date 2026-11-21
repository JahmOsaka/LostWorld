using UnityEngine;

[CreateAssetMenu(fileName = "NewBombHeadSkill", menuName = "Data/Skills/Bomb Head")]
public class BombHeadSkillData : ActiveSkillData
{
    [Header("Throw Settings")]
    [Tooltip("Prefab containing BombHeadProjectile, Rigidbody2D, and a non-trigger Collider2D.")]
    public GameObject bombPrefab;
    [Tooltip("Initial throw velocity (X is mirrored by facing direction).")]
    public Vector2 throwVelocity = new Vector2(6f, 6f);
    [Tooltip("Spawn offset relative to the user (X is mirrored by facing direction).")]
    public Vector2 spawnOffset = new Vector2(0.6f, 0.4f);

    [Header("Explosion Settings")]
    [Tooltip("Fuse time in seconds before automatic detonation (GDD: 1 to 3 seconds).")]
    [Range(0.5f, 5f)] public float fuseDuration = 2f;
    [Tooltip("Radius of the explosion blast.")]
    public float explosionRadius = 2.8f;
    [Tooltip("Damage dealt to all enemies within the explosion radius.")]
    public int explosionDamage = 30;
    [Tooltip("Knockback force pushing enemies away from the blast center.")]
    public float knockbackForce = 9f;
    [Tooltip("Layers considered valid enemy targets for the explosion.")]
    public LayerMask enemyLayer;

    [Header("Screen Shake")]
    [Tooltip("Duration of camera shake on detonation.")]
    public float shakeDuration = 0.18f;
    [Tooltip("Magnitude of camera shake on detonation.")]
    public float shakeMagnitude = 0.25f;

    public override void ExecuteSkill(GameObject user)
    {
        if (bombPrefab == null || user == null) return;

        Player player = user.GetComponent<Player>();
        int dir = (player != null) ? player.FacingDirection : (user.transform.localScale.x < 0f ? -1 : 1);

        Vector3 spawnPos = user.transform.position + new Vector3(spawnOffset.x * dir, spawnOffset.y, 0f);
        GameObject obj = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        BombHeadProjectile bomb = obj.GetComponent<BombHeadProjectile>();
        if (bomb != null)
        {
            bomb.Initialize(this, dir);
        }
        user.GetComponent<PlayerCastFeedback>()?.TriggerCast(CastFeedbackType.Throw);
    }
}