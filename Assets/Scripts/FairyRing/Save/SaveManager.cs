using UnityEngine;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
    }

    public SaveGameData GetSaveData(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<SaveGameData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading save data from slot {slotIndex}: {e.Message}");
            }
        }
        return null;
    }

    public void SaveGame(int slotIndex, GameObject player)
    {
        SaveGameData data = new SaveGameData();

        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

        if (CheckpointManager.Instance != null)
            data.checkpointId = CheckpointManager.Instance.LastCheckpointId;

        PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
        if (stats != null)
            data.currentCurrency = stats.currentCurrency;

        ShortcutGate[] allGates = FindObjectsByType<ShortcutGate>(FindObjectsInactive.Include);
        foreach (ShortcutGate gate in allGates)
        {
            if (gate.isUnlocked && !string.IsNullOrEmpty(gate.gateID))
            {
                data.unlockedGates.Add(gate.gateID);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slotIndex), json);
        Debug.Log($"Game Saved to Slot {slotIndex} at {GetSavePath(slotIndex)}");
    }

    public void LoadGame(int slotIndex, GameObject player)
    {
        Debug.Log("--- 1. Starting LoadGame ---");

        SaveGameData data = GetSaveData(slotIndex);
        if (data == null)
        {
            Debug.LogError("❌ Failed to load save file (Data is null). Execution stopped!");
            return;
        }

        Debug.Log("--- 2. Save file loaded successfully. Managing currency... ---");
        PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
        if (stats != null)
        {
            stats.AddCurrency(data.currentCurrency - stats.currentCurrency);
        }

        Debug.Log("--- 3. Respawning enemies... ---");
        EnemyRespawnManager respawnManager = FindAnyObjectByType<EnemyRespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.RespawnAllEnemies();
        }
        else
        {
            Debug.LogWarning("⚠️ EnemyRespawnManager not found! (Execution continues)");
        }

        Debug.Log("--- 4. Searching for gates/obstacles... ---");
        ShortcutGate[] allGates = FindObjectsByType<ShortcutGate>(FindObjectsInactive.Include);
        Debug.Log($">> Total gates found: {allGates.Length}");

        foreach (ShortcutGate gate in allGates)
        {
            if (data.unlockedGates != null && data.unlockedGates.Contains(gate.gateID))
            {
                Debug.Log($"Gate: {gate.gateID} -> Action: Unlocked");
                gate.Interact();
            }
            else
            {
                Debug.Log($"Gate: {gate.gateID} -> Action: Reset to Locked");
                gate.ResetToLocked();
            }
        }

        Debug.Log($"--- 5. LoadGame for Slot {slotIndex} completed successfully! ---");
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save in Slot {slotIndex}");
        }
    }
}