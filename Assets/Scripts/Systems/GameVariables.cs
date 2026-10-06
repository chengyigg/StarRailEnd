using UnityEngine;
using System.Collections.Generic;

public static class GameVariables
{
    private const string PREFIX = "Var_";

    // 用于 ResetAll 时删除所有键
    private static readonly List<string> RegisteredVars = new List<string>();

    public static int GetInt(string name, int def = 0)
    {
        if (string.IsNullOrEmpty(name)) return def;
        return PlayerPrefs.GetInt(PREFIX + name, def);
    }

    public static void SetInt(string name, int val)
    {
        if (string.IsNullOrEmpty(name)) return;
        PlayerPrefs.SetInt(PREFIX + name, val);
        Register(name);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("variable_set");
    }

    public static bool GetBool(string name, bool def = false)
    {
        if (string.IsNullOrEmpty(name)) return def;
        return PlayerPrefs.GetInt(PREFIX + name, def ? 1 : 0) == 1;
    }

    public static void SetBool(string name, bool val)
    {
        if (string.IsNullOrEmpty(name)) return;
        PlayerPrefs.SetInt(PREFIX + name, val ? 1 : 0);
        Register(name);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("variable_set");
    }

    public static string GetString(string name, string def = "")
    {
        if (string.IsNullOrEmpty(name)) return def;
        return PlayerPrefs.GetString(PREFIX + name, def);
    }

    public static void SetString(string name, string val)
    {
        if (string.IsNullOrEmpty(name)) return;
        PlayerPrefs.SetString(PREFIX + name, val);
        Register(name);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("variable_set");
    }

    private static void Register(string name)
    {
        if (!string.IsNullOrEmpty(name) && !RegisteredVars.Contains(name))
            RegisteredVars.Add(name);
    }

    /// <summary>清空所有剧情变量。</summary>
    public static void ResetAll()
    {
        foreach (var name in RegisteredVars)
            PlayerPrefs.DeleteKey(PREFIX + name);
        RegisteredVars.Clear();
        GameLog.Info("[变量] 已重置所有剧情变量");
    }
}