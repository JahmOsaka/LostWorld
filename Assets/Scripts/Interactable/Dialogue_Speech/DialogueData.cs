using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueEntry
{
    [Tooltip("Leave empty for narration or system messages.")]
    public CharacterProfile speaker;

    [Tooltip("(Optional) Overrides the speaker's default portrait for expressions.")]
    public Sprite overridePortrait;

    [Tooltip("(Optional) Overrides the speaker's name if no profile is assigned.")]
    public string customSpeakerName;

    [TextArea(2, 5)]
    public string text;
}

[CreateAssetMenu(fileName = "NewDialogueData", menuName = "Data/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public List<DialogueEntry> lines = new List<DialogueEntry>();
}