using UnityEngine;

[CreateAssetMenu(fileName = "NewOrbitSkill", menuName = "Data/Skills/Orbit")]
public class OrbitSkillData : ActiveSkillData
{
    [Header("Orbit Settings")]
    [Tooltip("Prefab containing the OrbitOrb script and a trigger Collider2D.")]
    public GameObject orbPrefab;
    [Tooltip("Number of orbs spawned around the player.")]
    public int orbCount = 2;
    [Tooltip("Distance from the player center to each orb.")]
    public float orbitRadius = 1.6f;
    [Tooltip("Rotation speed in degrees per second.")]
    public float orbitSpeed = 240f;
    [Tooltip("Total duration in seconds the orbs remain active.")]
    public float duration = 6f;

    [Header("Combat Settings")]
    [Tooltip("Damage dealt each time an orb strikes an enemy.")]
    public int damage = 12;
    [Tooltip("Cooldown in seconds before the same enemy can be hit by an orb again.")]
    public float hitCooldownPerEnemy = 0.35f;
    [Tooltip("Knockback force pushing enemies away from the player.")]
    public float knockbackForce = 4f;
    [Tooltip("Layers considered valid enemy targets.")]
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