using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "TextAdventure/Character Data")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite avatar;
    public int initialAffection = 0;

    [Header("是否一开始就解锁")]
    public bool unlockedByDefault = true;

    [Header("好感度到 0 时触发的对话")]
    public DialogueSequence zeroDialogue;
}