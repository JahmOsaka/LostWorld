using System;
using System.Collections.Generic;

[Serializable]
public class SaveGameData
{
    public string saveTime;
    public string checkpointId;
    public int currentCurrency;

    public List<string> unlockedGates = new List<string>();
}