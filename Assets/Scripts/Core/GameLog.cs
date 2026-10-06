using UnityEngine;
using ULog = UnityEngine.Debug;

/// <summary>
/// 统一日志入口。
/// 编辑器 / Development Build 下输出，Release 构建下自动屏蔽（编译期移除）。
/// 使用 ULog 别名，避免被全局 Debug.Log 替换误伤。
/// </summary>
public static class GameLog
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Info(object msg) => ULog.Log(msg);

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Info(object msg, Object context) => ULog.Log(msg, context);

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(object msg) => ULog.LogWarning(msg);

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [System.Diagnostics.Conditional("DEVELOPMENT_BUILD")]
    public static void Warning(object msg, Object context) => ULog.LogWarning(msg, context);

    // Error 永远输出
    public static void Error(object msg) => ULog.LogError(msg);

    public static void Error(object msg, Object context) => ULog.LogError(msg, context);
}