using System;
using System.Collections.Generic;

[Serializable]
public class SaveGameData
{
    public string saveDate;
    public string locationName;
    public float playTime;
    public string saveTime;

    public int currentCurrency;
    public List<string> unlockedGates = new List<string>();
}