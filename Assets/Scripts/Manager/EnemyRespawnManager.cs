using System.Collections.Generic;
using UnityEngine;

public class EnemyRespawnManager : MonoBehaviour
{
    private static List<RespawnableEnemy> allEnemies = new List<RespawnableEnemy>();

    [Header("Penalty Settings")]
    [Tooltip("Prefab Currency that drop before death")]
    public GameObject droppedCurrencyPrefab;

    private GameObject currentDroppedCurrency;

    private void OnEnable()
    {
        FairyRingMenuUI.OnRestRequested += HandleRest;
        CoreHealth.OnPlayerDeathGlobal += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        FairyRingMenuUI.OnRestRequested -= HandleRest;
        CoreHealth.OnPlayerDeathGlobal -= HandlePlayerDeath;
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
        Debug.Log("GameManager: Player Died! Applying penalties...");

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

        if (CheckpointManager.Instance != null && !string.IsNullOrEmpty(CheckpointManager.Instance.LastCheckpointId))
        {
            player.transform.position = CheckpointManager.Instance.LastRespawnPosition;
        }

        CoreHealth playerHealth = player.GetComponent<CoreHealth>();
        if (playerHealth != null)
        {
            playerHealth.Revive();
        }

        Player playerComp = player.GetComponent<Player>();
        if (playerComp != null)
        {
            playerComp.enabled = true;
            playerComp.DisableHitbox();
            playerComp.HideAllCombatVisuals();
            playerComp.StateMachine?.ChangeState(playerComp.IdleState);
        }

        PlayerInputHandler input = player.GetComponent<PlayerInputHandler>();
        if (input != null)
        {
            input.enabled = true;
        }

        RespawnAllEnemies();
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