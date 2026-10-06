using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ConditionalJump
{
    public ChoiceCondition condition = ChoiceCondition.None;
    public string variableName;
    public int variableValue;
    public string targetSequenceID;

    public bool Check()
    {
        return ConditionEvaluator.Evaluate(condition, variableName, variableValue);
    }
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "TextAdventure/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    public string sequenceID;
    public string nextSequenceID;
    public List<DialogueLine> lines = new List<DialogueLine>();

    [Header("完成本序列时的好感度变化")]
    public string affectionCharacter;
    public int affectionDelta;

    [Header("完成本序列时解锁的角色")]
    public string unlockCharacter;

    [Header("进入本序列时显示的章节标题")]
    public string chapterTitle;

    [Header("进入本序列时播放的 BGM（留空则不变）")]
    public string bgmName;

    [Header("完成本序列时设置变量（可选）")]
    public string setVariableName;
    public int setVariableValue;

    [Header("条件跳转")]
    public List<ConditionalJump> conditionalNexts = new List<ConditionalJump>();
}