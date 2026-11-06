using UnityEngine;

public class RestService : MonoBehaviour
{
    private void OnEnable() => FairyRingActivation.OnFairyRingInteracted += HandleRest;
    private void OnDisable() => FairyRingActivation.OnFairyRingInteracted -= HandleRest;

    private void HandleRest(FairyRingActivation ring, GameObject player)
    {
        CoreHealth health = player.GetComponentInChildren<CoreHealth>();
        if (health != null)
        {
            health.Heal(health.maxHealth);
            Debug.Log("RestService: Player fully healed.");
        }
    }
}