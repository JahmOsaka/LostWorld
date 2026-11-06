using UnityEngine;

public class SaveManager : MonoBehaviour
{
    private void OnEnable() => FairyRingActivation.OnFairyRingInteracted += SaveGame;
    private void OnDisable() => FairyRingActivation.OnFairyRingInteracted -= SaveGame;

    private void SaveGame(FairyRingActivation ring, GameObject player)
    {
        Debug.Log("SaveManager: Saving game data to disk...");
    }
}