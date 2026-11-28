using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyAbility", menuName = "Skills/Key Ability")]
public class KeyAbilitySkillData : SkillData
{
    private void OnEnable()
    {
        category = SkillCategory.KeyAbility;
    }
}