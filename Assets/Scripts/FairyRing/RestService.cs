using UnityEngine;

public class RestService : MonoBehaviour
{
    private void OnEnable() => FairyRingMenuUI.OnRestRequested += HandleRest;
    private void OnDisable() => FairyRingMenuUI.OnRestRequested -= HandleRest;

    private void HandleRest(GameObject player)
    {
        CoreHealth health = player.GetComponentInChildren<CoreHealth>();
        if (health != null)
        {
            health.Heal(health.maxHealth);
            Debug.Log("RestService: Player fully healed.");
        }
    }
}