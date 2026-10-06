using UnityEngine;

public enum ChoiceCondition
{
    None,
    VariableEquals,
    VariableGreaterOrEqual,
    VariableLessOrEqual,
    FlagSet,
    FlagNotSet,
    AffectionGreaterOrEqual,
    AffectionLessOrEqual,
    AffectionEquals
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public string nextSequenceID;

    [Header("显示条件")]
    public ChoiceCondition condition = ChoiceCondition.None;
    public string variableName;
    public int variableValue;

    [Header("不满足条件时：勾选=隐藏，取消=置灰")]
    public bool hideWhenUnmet = true;

    [Header("好感度变化（选中时触发）")]
    public string affectionCharacter;
    public int affectionDelta;

    public bool CheckCondition()
    {
        return ConditionEvaluator.Evaluate(condition, variableName, variableValue);
    }

}