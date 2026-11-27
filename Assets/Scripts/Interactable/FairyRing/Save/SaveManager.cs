using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Skill Registry")]
    [Tooltip("Drag all SkillData assets in the project here so SaveManager can restore them by ID.")]
    [SerializeField] private List<SkillData> skillDatabase = new List<SkillData>();

    public float CurrentPlayTime { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Time.timeScale > 0f)
        {
            CurrentPlayTime += Time.deltaTime;
        }
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
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveGameData>(json);
        }
        return null;
    }

    public void SaveGame(int slotIndex, GameObject player)
    {
        Debug.Log($"--- Starting SaveGame for Slot {slotIndex} ---");
        SaveGameData data = new SaveGameData();

        Scene activeScene = SceneManager.GetActiveScene();
        data.sceneName = activeScene.name;
        data.locationName = activeScene.name;
        data.saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        data.playTime = CurrentPlayTime;

        if (player != null)
        {
            data.hasSavedPosition = true;
            data.playerPosition = player.transform.position;

            CoreHealth health = player.GetComponentInChildren<CoreHealth>();
            PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
            if (stats != null || health != null)
            {
                data.hasSavedStats = true;
                if (health != null)
                {
                    data.currentHealth = health.currentHealth;
                }
                if (stats != null)
                {
                    data.bonusMaxEnergy = stats.bonusMaxEnergy;
                    data.currentEnergy = stats.currentEnergy;
                    data.currentPotions = stats.currentPotions;
                    data.currentCurrency = stats.currentCurrency;
                }
            }

            PlayerSkillController skillController = player.GetComponentInChildren<PlayerSkillController>();
            if (skillController != null)
            {
                data.hasPerish = skillController.hasPerish;
                data.hasDoubleJump = skillController.hasDoubleJump;
                data.hasSuperDash = skillController.hasSuperDash;
                data.activeWheelIndex = skillController.activeWheelIndex;

                data.unlockedSkillIds = new List<string>();
                foreach (SkillData skill in skillController.unlockedCombatSkills)
                {
                    string id = GetSkillId(skill);
                    if (!string.IsNullOrEmpty(id))
                    {
                        data.unlockedSkillIds.Add(id);
                    }
                }

                data.equippedSkillIds = new List<string>();
                for (int i = 0; i < skillController.equippedWheelSkills.Length; i++)
                {
                    data.equippedSkillIds.Add(GetSkillId(skillController.equippedWheelSkills[i]));
                }
            }
        }

        data.unlockedGates = new List<string>();
        BaseGate[] allGates = FindObjectsByType<BaseGate>(FindObjectsInactive.Include);
        foreach (BaseGate gate in allGates)
        {
            if (gate.isUnlocked && !string.IsNullOrEmpty(gate.gateID))
            {
                data.unlockedGates.Add(gate.gateID);
            }
        }

        data.activatedFairyRings = new List<string>();
        FairyRingActivation[] allRings = FindObjectsByType<FairyRingActivation>(FindObjectsInactive.Include);
        foreach (FairyRingActivation ring in allRings)
        {
            if (ring.isActivated && !string.IsNullOrEmpty(ring.checkpointId))
            {
                data.activatedFairyRings.Add(ring.checkpointId);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slotIndex), json);
        Debug.Log($"Game Saved to {GetSavePath(slotIndex)}");
    }

    public void LoadGame(int slotIndex, GameObject player)
    {
        Debug.Log("--- Starting LoadGame ---");

        SaveGameData data = GetSaveData(slotIndex);
        if (data == null)
        {
            Debug.LogError("Failed to load save file (Data is null). Execution stopped!");
            return;
        }

        CurrentPlayTime = data.playTime;

        if (player != null)
        {
            if (data.hasSavedPosition)
            {
                player.transform.position = data.playerPosition;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }

            CoreHealth health = player.GetComponentInChildren<CoreHealth>();
            PlayerStats stats = player.GetComponentInChildren<PlayerStats>();
            if (data.hasSavedStats)
            {
                if (health != null)
                {
                    health.SetHealth(data.currentHealth);
                }
                if (stats != null)
                {
                    stats.LoadSavedStats(
                        data.bonusMaxEnergy,
                        data.currentEnergy,
                        data.currentPotions,
                        data.currentCurrency
                    );
                }
            }
            else if (stats != null)
            {
                stats.AddCurrency(data.currentCurrency - stats.currentCurrency);
            }

            PlayerSkillController skillController = player.GetComponentInChildren<PlayerSkillController>();
            if (skillController != null)
            {
                List<SkillData> restoredUnlocked = new List<SkillData>();
                if (data.unlockedSkillIds != null)
                {
                    foreach (string id in data.unlockedSkillIds)
                    {
                        SkillData skill = FindSkillById(id);
                        if (skill != null) restoredUnlocked.Add(skill);
                    }
                }

                SkillData[] restoredEquipped = new SkillData[skillController.equippedWheelSkills.Length];
                if (data.equippedSkillIds != null)
                {
                    int count = Mathf.Min(restoredEquipped.Length, data.equippedSkillIds.Count);
                    for (int i = 0; i < count; i++)
                    {
                        restoredEquipped[i] = FindSkillById(data.equippedSkillIds[i]);
                    }
                }

                skillController.LoadSavedSkills(
                    data.hasPerish,
                    data.hasDoubleJump,
                    data.hasSuperDash,
                    restoredUnlocked,
                    restoredEquipped,
                    data.activeWheelIndex
                );
            }
        }

        RespawnManager respawnManager = FindAnyObjectByType<RespawnManager>();
        if (respawnManager != null)
        {
            respawnManager.RespawnAllEnemies();
        }

        BaseGate[] allGates = FindObjectsByType<BaseGate>(FindObjectsInactive.Include);
        foreach (BaseGate gate in allGates)
        {
            if (data.unlockedGates != null && !string.IsNullOrEmpty(gate.gateID) && data.unlockedGates.Contains(gate.gateID))
            {
                gate.UnlockImmediate();
            }
            else
            {
                gate.ResetToLocked();
            }
        }

        FairyRingActivation[] allRings = FindObjectsByType<FairyRingActivation>(FindObjectsInactive.Include);
        foreach (FairyRingActivation ring in allRings)
        {
            bool shouldActivate = data.activatedFairyRings != null
                && !string.IsNullOrEmpty(ring.checkpointId)
                && data.activatedFairyRings.Contains(ring.checkpointId);

            ring.isActivated = shouldActivate;

            FairyRingVisual visual = ring.GetComponent<FairyRingVisual>();
            if (visual != null && visual.activatedVFX != null)
            {
                visual.activatedVFX.SetActive(shouldActivate);
            }
        }

        Debug.Log($"--- LoadGame for Slot {slotIndex} completed successfully! ---");
    }

    public void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"Deleted save file at slot {slotIndex}");
        }
        else
        {
            Debug.LogWarning($"No save file found at slot {slotIndex} to delete.");
        }
    }

    private string GetSkillId(SkillData skill)
    {
        if (skill == null) return string.Empty;
        return !string.IsNullOrEmpty(skill.name) ? skill.name : skill.skillName;
    }

    private SkillData FindSkillById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < skillDatabase.Count; i++)
        {
            SkillData skill = skillDatabase[i];
            if (skill == null) continue;

            if (skill.name == id || skill.skillName == id)
            {
                return skill;
            }
        }

        Debug.LogWarning($"SaveManager: Skill ID '{id}' not found in skillDatabase.");
        return null;
    }
}