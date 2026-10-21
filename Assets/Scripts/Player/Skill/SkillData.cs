using UnityEngine;

public enum SkillType
{
    CombatSkill,
    KeySkill
}

public abstract class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public SkillType skillType = SkillType.CombatSkill;

    [TextArea(3, 5)]
    [Tooltip("Skill description to display in the Fairy Ring Skill Select UI.")]
    public string description;

    [Header("Combat Stats")]
    [Tooltip("Cooldown time in seconds before the skill can be used again.")]
    public float cooldownTime;
    [Tooltip("Mana cost required to execute this combat skill.")]
    public int mpCost = 20;

    [Header("UI & TechArt")]
    public Sprite skillIcon;
    public GameObject vfxPrefab;

    public abstract void ExecuteSkill(GameObject user);
}