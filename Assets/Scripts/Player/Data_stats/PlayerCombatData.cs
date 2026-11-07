using UnityEngine;

[System.Serializable]
public class PlayerCombatData
{
    public int ComboCounter { get; private set; }
    public float LastAttackTime { get; set; } = -999f;
    public float DodgeCooldownTimer { get; set; }

    public float DamageMultiplier { get; set; } = 1f;

    public void RecordAttack()
    {
        ComboCounter++;
        if (ComboCounter > 3) ComboCounter = 1;
    }

    public void ResetCombo()
    {
        ComboCounter = 0;
    }

    public void TriggerCombatState()
    {
        LastAttackTime = Time.time;
    }

    public void TickDodgeCooldown(float deltaTime)
    {
        if (DodgeCooldownTimer > 0) DodgeCooldownTimer -= deltaTime;
    }
}