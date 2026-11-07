using System.Runtime.InteropServices;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    public string LastCheckpointId { get; private set; }
    public Vector3 LastRespawnPosition { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable() => FairyRingMenuUI.OnRestRequested += HandleCheckpointSaved;
    private void OnDisable() => FairyRingMenuUI.OnRestRequested -= HandleCheckpointSaved;

    private void HandleCheckpointSaved(GameObject player)
    {
        FairyRingActivation currentRing = FindAnyObjectByType<FairyRingActivation>();
        if (currentRing != null)
        {
            LastCheckpointId = currentRing.checkpointId;
            LastRespawnPosition = currentRing.transform.position;
            Debug.Log($"CheckpointManager: Saved checkpoint {LastCheckpointId}");
        }
    }
}