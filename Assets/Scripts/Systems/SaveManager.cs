using UnityEngine;
using System.IO;
using System;

public static class SaveManager
{
    private const string AUTOSAVE_FILE = "autosave.json";
    private const string SLOT_PREFIX = "slot_";
    private const string SLOT_SUFFIX = ".json";
    public const int SLOT_COUNT = 9;
    public static event System.Action OnAutoSaveTriggered;

    private static string SaveFolder
    {
        get
        {
            string path = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    private static string GetSlotPath(int index)
        => Path.Combine(SaveFolder, $"{SLOT_PREFIX}{index}{SLOT_SUFFIX}");

    private static string GetAutoSavePath()
        => Path.Combine(SaveFolder, AUTOSAVE_FILE);

    public static void SaveToSlot(int index, SaveData data)
    {
        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSlotPath(index), json);
        GameLog.Info($"[SaveManager] 槽位 {index} 已保存 (场景: {data.sceneName}, 进度: {data.sequenceID})");
    }

    public static SaveData LoadFromSlot(int index)
    {
        string path = GetSlotPath(index);
        if (!File.Exists(path)) return null;
        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch { return null; }
    }

    public static bool HasSlotData(int index) => File.Exists(GetSlotPath(index));

    public static void SaveAuto(SaveData data)
    {
        data.saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetAutoSavePath(), json);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("auto_save");
        OnAutoSaveTriggered?.Invoke();
    }

    public static SaveData LoadAuto()
    {
        string path = GetAutoSavePath();
        if (!File.Exists(path)) return null;
        try { return JsonUtility.FromJson<SaveData>(File.ReadAllText(path)); }
        catch { return null; }
    }

    public static bool HasAutoSave() => File.Exists(GetAutoSavePath());
    /// <summary>根据截图文件名返回完整路径。</summary>
    public static string GetScreenshotPath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;
        return Path.Combine(SaveFolder, "screenshots", fileName);
    }

    /// <summary>确保截图文件夹存在并返回路径。</summary>
    public static string EnsureScreenshotFolder()
    {
        string path = Path.Combine(SaveFolder, "screenshots");
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }
    /// <summary>删除指定槽位的存档（JSON + 截图）。</summary>
    public static void DeleteSlot(int index)
    {
        string path = GetSlotPath(index);
        if (File.Exists(path))
        {
            // 先读出 SaveData，好知道截图文件名
            try
            {
                SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data != null && !string.IsNullOrEmpty(data.screenshotFile))
                {
                    string shot = GetScreenshotPath(data.screenshotFile);
                    if (!string.IsNullOrEmpty(shot) && File.Exists(shot))
                        File.Delete(shot);
                }
            }
            catch { }

            File.Delete(path);
        }
    }

    /// <summary>删除所有槽位存档（JSON + 截图），不影响自动存档。</summary>
    public static void DeleteAllSlots()
    {
        for (int i = 0; i < SLOT_COUNT; i++)
            DeleteSlot(i);
    }
    public static string GetSaveFolderPath() => SaveFolder;
}