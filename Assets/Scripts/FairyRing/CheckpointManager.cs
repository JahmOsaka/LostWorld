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

    private void OnEnable() => FairyRingActivation.OnFairyRingInteracted += HandleCheckpointSaved;
    private void OnDisable() => FairyRingActivation.OnFairyRingInteracted -= HandleCheckpointSaved;

    private void HandleCheckpointSaved(FairyRingActivation ring, GameObject player)
    {
        LastCheckpointId = ring.checkpointId;
        LastRespawnPosition = ring.transform.position;
        Debug.Log($"CheckpointManager: Saved checkpoint {LastCheckpointId}");
    }
}