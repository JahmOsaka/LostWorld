using UnityEngine;

public class RestService : MonoBehaviour
{
    private void OnEnable() => FairyRingMenuUI.OnRestRequested += HandleRest;
    private void OnDisable() => FairyRingMenuUI.OnRestRequested -= HandleRest;

    private void HandleRest(GameObject player)
    {
        if (player == null) return;

        CoreHealth health = player.GetComponentInChildren<CoreHealth>();
        if (health != null)
        {
            health.Heal(health.maxHealth);
        }

        PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
        if (stats != null)
        {
            stats.RefillEnergy();
            stats.RefillPotions();
        }

        Debug.Log("RestService: Player HP, MP, and Life Gems fully refilled.");
    }
}