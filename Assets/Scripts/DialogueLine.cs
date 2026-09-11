using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    [TextArea(2, 5)]
    public string sentence;

    [Tooltip("Check this if the Player is speaking (Left). Uncheck if the NPC is speaking (Right).")]
    public bool isPlayerSpeaking;
}