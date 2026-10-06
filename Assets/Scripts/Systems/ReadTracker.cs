using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 记录玩家已读过的对话行。
/// 内存维护 HashSet，改动只打脏标记；由 ReadTrackerSaver 定期落盘。
/// </summary>
public static class ReadTracker
{
    private static HashSet<string> readKeys = new HashSet<string>();
    private static bool loaded = false;
    private static bool dirty = false;

    /// <summary>是否有未落盘的改动。ReadTrackerSaver 会周期性检查。</summary>
    public static bool IsDirty => dirty;

    private static string FilePath =>
        Path.Combine(Application.persistentDataPath, "read_data.json");

    [System.Serializable]
    private class Wrapper { public List<string> keys = new List<string>(); }

    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;
        if (!File.Exists(FilePath)) return;

        try
        {
            var w = JsonUtility.FromJson<Wrapper>(File.ReadAllText(FilePath));
            if (w != null && w.keys != null)
                readKeys = new HashSet<string>(w.keys);
        }
        catch (System.Exception e)
        {
            GameLog.Warning($"[ReadTracker] 读取失败，忽略并重置: {e.Message}");
        }
    }

    public static bool IsRead(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        EnsureLoaded();
        return readKeys.Contains(key);
    }

    public static void MarkRead(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        EnsureLoaded();
        if (readKeys.Add(key))
            dirty = true;   // 只打标记，不写盘
    }

    /// <summary>把内存里的已读数据落盘。没脏就啥也不干。</summary>
    public static void Save()
    {
        if (!dirty) return;
        dirty = false;

        try
        {
            var w = new Wrapper { keys = new List<string>(readKeys) };
            File.WriteAllText(FilePath, JsonUtility.ToJson(w));
        }
        catch (System.Exception e)
        {
            GameLog.Error($"[ReadTracker] 保存失败: {e.Message}");
            dirty = true;   // 保存失败，保留脏标记下次再试
        }
    }

    /// <summary>清空已读记录（新游戏时用）。</summary>
    public static void Clear()
    {
        readKeys.Clear();
        dirty = true;
    }
}