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

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slotIndex), json);
        Debug.Log($"Game Saved to Slot {slotIndex} at {GetSavePath(slotIndex)}");
    }

    public void LoadGame(int slotIndex, GameObject player)
    {
        SaveGameData data = GetSaveData(slotIndex);
        if (data == null) return;

        PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
        if (stats != null)
        {
            stats.AddCurrency(data.currentCurrency - stats.currentCurrency);
        }

        Debug.Log($"Game Loaded from Slot {slotIndex}");
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