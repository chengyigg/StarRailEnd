using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 存档截图加载器：按文件名读取 PNG 并缓存成 Sprite。
/// SlotUI（小缩略图）和 SavePanel（大预览）共用。
/// </summary>
public static class SaveScreenshotLoader
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Load(string fileName)
    {
        if (string.IsNullOrEmpty(fileName)) return null;

        if (cache.TryGetValue(fileName, out var cached) && cached != null)
            return cached;

        string path = SaveManager.GetScreenshotPath(fileName);
        if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path)) return null;

        try
        {
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            tex.LoadImage(bytes);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
            cache[fileName] = sprite;
            return sprite;
        }
        catch (System.Exception e)
        {
            GameLog.Warning($"[SaveScreenshotLoader] 加载失败 {fileName}: {e.Message}");
            return null;
        }
    }

    /// <summary>清空缓存（存档后调用，让新截图立刻显示）。</summary>
    public static void Clear()
    {
        foreach (var s in cache.Values)
            if (s != null) Object.Destroy(s.texture);
        cache.Clear();
    }
}