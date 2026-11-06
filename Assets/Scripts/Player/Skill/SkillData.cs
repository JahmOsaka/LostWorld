using UnityEngine;

public enum SkillCategory
{
    CombatActive,
    CombatPassive,
    KeyAbility,
    Ultimate
}

public abstract class SkillData : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public SkillCategory category = SkillCategory.CombatActive;

    [TextArea(3, 5)]
    public string description;

    [Header("UI & TechArt")]
    public Sprite skillIcon;
    public GameObject vfxPrefab;

    public virtual void ExecuteSkill(GameObject user) { }
}