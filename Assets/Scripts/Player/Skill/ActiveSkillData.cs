using UnityEngine;

public abstract class ActiveSkillData : SkillData
{
    [Header("Active Combat Stats")]
    public float cooldownTime;
    public int mpCost = 20;
    public float castDuration;
}