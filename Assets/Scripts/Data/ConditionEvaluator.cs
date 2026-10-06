/// <summary>
/// 对话系统条件判断的统一入口。
/// DialogueChoice.CheckCondition 和 ConditionalJump.Check 都调用它。
/// </summary>
public static class ConditionEvaluator
{
    /// <summary>根据条件枚举和参数判断条件是否满足。</summary>
    public static bool Evaluate(ChoiceCondition condition, string variableName, int variableValue)
    {
        switch (condition)
        {
            case ChoiceCondition.None:
                return true;
            case ChoiceCondition.VariableEquals:
                return GameVariables.GetInt(variableName) == variableValue;
            case ChoiceCondition.VariableGreaterOrEqual:
                return GameVariables.GetInt(variableName) >= variableValue;
            case ChoiceCondition.VariableLessOrEqual:
                return GameVariables.GetInt(variableName) <= variableValue;
            case ChoiceCondition.FlagSet:
                return GameVariables.GetBool(variableName);
            case ChoiceCondition.FlagNotSet:
                return !GameVariables.GetBool(variableName);
            case ChoiceCondition.AffectionGreaterOrEqual:
                return AffectionManager.Get(variableName) >= variableValue;
            case ChoiceCondition.AffectionLessOrEqual:
                return AffectionManager.Get(variableName) <= variableValue;
            case ChoiceCondition.AffectionEquals:
                return AffectionManager.Get(variableName) == variableValue;
        }
        return true;
    }
}