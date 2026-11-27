using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveGameData
{
    public string sceneName;
    public string locationName;
    public string saveTime;
    public float playTime;

    public bool hasSavedPosition;
    public Vector3 playerPosition;

    public bool hasSavedStats;
    public int currentHealth;
    public float bonusMaxEnergy;
    public float currentEnergy;
    public int currentPotions;
    public int currentCurrency;

    public bool hasPerish;
    public bool hasDoubleJump;
    public bool hasSuperDash;

    public List<string> unlockedSkillIds = new List<string>();
    public List<string> equippedSkillIds = new List<string>();
    public int activeWheelIndex;

    public List<string> unlockedGates = new List<string>();
    public List<string> activatedFairyRings = new List<string>();
}