using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName;
    public float cooldownTime;

    [Header("TechArt")]
    public GameObject vfxPrefab;

    public abstract void ExecuteSkill(GameObject user);
}
