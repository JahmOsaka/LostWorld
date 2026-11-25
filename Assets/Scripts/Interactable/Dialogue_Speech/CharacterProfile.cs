using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterProfile", menuName = "Data/Dialogue/Character Profile")]
public class CharacterProfile : ScriptableObject
{
    public string characterName;
    public Sprite defaultPortrait;
    public Color nameColor = new Color(1f, 0.85f, 0.35f, 1f);
}