using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RespawnManager : MonoBehaviour
{
    private static List<RespawnableEnemy> allEnemies = new List<RespawnableEnemy>();

    [Header("Penalty Settings")]
    [Tooltip("Prefab for the currency dropped at the player's death location.")]
    public GameObject droppedCurrencyPrefab;

    [Header("Respawn Timing")]
    [Tooltip("Delay (seconds) after player death before warping to checkpoint and reviving.")]
    [Range(0f, 5f)][SerializeField] private float respawnDelay = 1.2f;

    [Header("Optional UI / Fade Events")]
    [Tooltip("Triggered immediately when the player dies (e.g., start screen fade-out or death banner).")]
    public UnityEvent OnDeathSequenceStart;
    [Tooltip("Triggered right after the player warps and revives (e.g., start screen fade-in).")]
    public UnityEvent OnRespawnComplete;

    private GameObject currentDroppedCurrency;
    private Coroutine respawnCoroutine;
    private bool isRespawning;

    private void OnEnable()
    {
        FairyRingMenuUI.OnRestRequested += HandleRest;
        CoreHealth.OnPlayerDeathGlobal += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        FairyRingMenuUI.OnRestRequested -= HandleRest;
        CoreHealth.OnPlayerDeathGlobal -= HandlePlayerDeath;

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
        isRespawning = false;
    }

    public static void RegisterEnemy(RespawnableEnemy enemy)
    {
        if (!allEnemies.Contains(enemy)) allEnemies.Add(enemy);
    }

    public static void UnregisterEnemy(RespawnableEnemy enemy)
    {
        if (allEnemies.Contains(enemy)) allEnemies.Remove(enemy);
    }

    private void HandleRest(GameObject player)
    {
        RespawnAllEnemies();
    }

    private void HandlePlayerDeath(GameObject player)
    {
        if (isRespawning || player == null) return;
        respawnCoroutine = StartCoroutine(RespawnSequenceRoutine(player));
    }

    private IEnumerator RespawnSequenceRoutine(GameObject player)
    {
        isRespawning = true;
        Debug.Log("GameManager: Player Died! Applying penalties...");

        Player playerComp = player.GetComponent<Player>();
        if (playerComp != null)
        {
            playerComp.DisableHitbox();
            playerComp.HideAllCombatVisuals();
        }

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null && droppedCurrencyPrefab != null)
        {
            if (currentDroppedCurrency != null) Destroy(currentDroppedCurrency);

            int droppedAmount = stats.DropAllCurrency();
            if (droppedAmount > 0)
            {
                currentDroppedCurrency = Instantiate(droppedCurrencyPrefab, player.transform.position, Quaternion.identity);
                currentDroppedCurrency.GetComponent<DroppedCurrency>().storedCurrency = droppedAmount;
            }
        }

        OnDeathSequenceStart?.Invoke();

        if (respawnDelay > 0f)
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        if (player == null)
        {
            isRespawning = false;
            respawnCoroutine = null;
            yield break;
        }

        if (CheckpointManager.Instance != null)
        {
            player.transform.position = CheckpointManager.Instance.GetRespawnPosition(player.transform.position);
            Physics2D.SyncTransforms();
            FindAnyObjectByType<CameraController>()?.SnapToPlayerImmediate();
        }

        CoreHealth playerHealth = player.GetComponent<CoreHealth>();
        if (playerHealth != null)
        {
            playerHealth.Revive();
        }

        if (playerComp != null)
        {
            playerComp.enabled = true;
            playerComp.StateMachine?.ChangeState(playerComp.IdleState);
        }

        PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
        if (input != null)
        {
            input.enabled = true;
        }

        RespawnAllEnemies();
        OnRespawnComplete?.Invoke();

        isRespawning = false;
        respawnCoroutine = null;
    }

    public void RespawnAllEnemies()
    {
        RespawnableEnemy[] enemies = FindObjectsByType<RespawnableEnemy>(FindObjectsInactive.Include);

        foreach (RespawnableEnemy enemy in enemies)
        {
            enemy.Respawn();
        }

        Debug.Log("All normal enemies respawned.");
    }
}