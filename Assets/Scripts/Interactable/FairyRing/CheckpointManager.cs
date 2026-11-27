using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Default Spawn (Fallback before first FairyRing)")]
    [Tooltip("Optional initial spawn transform. If left empty, the Player's starting position is used.")]
    [SerializeField] private Transform defaultSpawnPoint;

    public string LastCheckpointId { get; private set; }
    public Vector3 LastRespawnPosition { get; private set; }
    public bool HasSavedCheckpoint => !string.IsNullOrEmpty(LastCheckpointId);

    private Vector3 initialSpawnPosition;
    private bool hasInitialSpawn;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (defaultSpawnPoint != null)
        {
            initialSpawnPosition = defaultSpawnPoint.position;
            hasInitialSpawn = true;
        }
        else
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                initialSpawnPosition = player.transform.position;
                hasInitialSpawn = true;
            }
        }
    }

    private void OnEnable() => FairyRingMenuUI.OnRestRequested += HandleCheckpointSaved;
    private void OnDisable() => FairyRingMenuUI.OnRestRequested -= HandleCheckpointSaved;

    public void SetCheckpoint(string checkpointId, Vector3 respawnPosition)
    {
        LastCheckpointId = checkpointId;
        LastRespawnPosition = respawnPosition;
    }

    public Vector3 GetRespawnPosition(Vector3 fallbackPosition)
    {
        if (HasSavedCheckpoint) return LastRespawnPosition;
        if (hasInitialSpawn) return initialSpawnPosition;
        return fallbackPosition;
    }

    private void HandleCheckpointSaved(GameObject player)
    {
        FairyRingActivation[] allRings = FindObjectsByType<FairyRingActivation>();
        if (allRings.Length == 0) return;

        FairyRingActivation closestRing = null;
        float minDistSqr = float.MaxValue;
        Vector3 playerPos = player != null ? player.transform.position : Vector3.zero;

        foreach (FairyRingActivation ring in allRings)
        {
            if (ring == null) continue;

            float distSqr = (ring.transform.position - playerPos).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                minDistSqr = distSqr;
                closestRing = ring;
            }
        }

        if (closestRing != null)
        {
            LastCheckpointId = closestRing.checkpointId;
            LastRespawnPosition = closestRing.transform.position;
            Debug.Log($"CheckpointManager: Saved checkpoint {LastCheckpointId}");
        }
    }
}