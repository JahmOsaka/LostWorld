using UnityEngine;

[CreateAssetMenu(fileName = "NewOrbitSkill", menuName = "Data/Skills/Orbit")]
public class OrbitSkillData : ActiveSkillData
{
    [Header("Orbit Shape & Motion")]
    [Tooltip("Prefab containing the OrbitOrb script and a trigger Collider2D.")]
    public GameObject orbPrefab;
    [Tooltip("Number of orbs spawned around the player.")]
    public int orbCount = 2;
    [Tooltip("Offset from the player pivot so orbs rotate around the torso instead of clipping the ground.")]
    public Vector2 centerOffset = new Vector2(0f, 0.7f);
    [Tooltip("Horizontal (X) and vertical (Y) orbit radii for an elliptical 2.5D feel.")]
    public Vector2 orbitRadius = new Vector2(1.75f, 1.15f);
    [Tooltip("Base rotation speed in degrees per second.")]
    public float orbitSpeed = 250f;
    [Tooltip("Total duration in seconds the orbs remain active.")]
    public float duration = 6f;

    [Header("Spawn & Despawn Animation")]
    [Tooltip("Duration in seconds for orbs to expand outward when cast.")]
    public float spawnDuration = 0.22f;
    [Tooltip("Duration in seconds for orbs to shrink inward before disappearing.")]
    public float despawnDuration = 0.28f;
    [Tooltip("Extra spin speed multiplier applied briefly during the spawn animation.")]
    public float spawnSpinBoost = 1.8f;

    [Header("Combat & Impact Feel")]
    [Tooltip("Damage dealt each time an orb strikes an enemy.")]
    public int damage = 12;
    [Tooltip("Cooldown in seconds before the same enemy can be hit by an orb again.")]
    public float hitCooldownPerEnemy = 0.35f;
    [Tooltip("Knockback force pushing enemies away from the player.")]
    public float knockbackForce = 4f;
    [Tooltip("Hit flash duration applied to struck enemies.")]
    public float enemyFlashDuration = 0.08f;
    [Tooltip("Subtle camera shake duration on orb impact.")]
    public float hitShakeDuration = 0.05f;
    [Tooltip("Subtle camera shake magnitude on orb impact.")]
    public float hitShakeMagnitude = 0.05f;
    [Tooltip("Layers considered valid enemy targets (e.g., EnemyHitbox).")]
    public LayerMask enemyLayer;

    public override void ExecuteSkill(GameObject user)
    {
        if (orbPrefab == null || user == null) return;

        OrbitSkillController existing = FindActiveControllerFor(user.transform);
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        GameObject holder = new GameObject("OrbitSkillController");
        OrbitSkillController controller = holder.AddComponent<OrbitSkillController>();
        controller.Initialize(user.transform, this);

        user.GetComponent<PlayerCastFeedback>()?.TriggerCast(CastFeedbackType.Pulse);
    }

    private OrbitSkillController FindActiveControllerFor(Transform owner)
    {
        OrbitSkillController[] controllers = FindObjectsByType<OrbitSkillController>();
        foreach (OrbitSkillController c in controllers)
        {
            if (c.Owner == owner) return c;
        }
        return null;
    }
}