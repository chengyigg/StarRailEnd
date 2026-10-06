using UnityEngine;
using System.Collections.Generic;

public enum HeartState { Empty, Half, Full }

public static class HeartSpriteGenerator
{
    // 16x16 心形图案（0=透明, 1=描边, 2=填充, 3=高光）
    private static readonly int[,] Pattern = new int[,]
    {
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
        {0,0,1,1,1,0,0,0,0,0,0,1,1,1,0,0},
        {0,1,2,2,2,1,0,0,0,0,1,2,2,2,1,0},
        {0,1,2,3,3,2,1,1,1,1,2,2,2,2,1,0},
        {1,2,2,3,3,3,2,2,2,2,2,2,2,2,2,1},
        {1,2,2,3,3,3,2,2,2,2,2,2,2,2,2,1},
        {1,2,2,2,3,3,2,2,2,2,2,2,2,2,2,1},
        {1,2,2,2,2,2,2,2,2,2,2,2,2,2,2,1},
        {0,1,2,2,2,2,2,2,2,2,2,2,2,2,1,0},
        {0,0,1,2,2,2,2,2,2,2,2,2,2,1,0,0},
        {0,0,0,1,2,2,2,2,2,2,2,2,1,0,0,0},
        {0,0,0,0,1,2,2,2,2,2,2,1,0,0,0,0},
        {0,0,0,0,0,1,2,2,2,2,1,0,0,0,0,0},
        {0,0,0,0,0,0,1,2,2,1,0,0,0,0,0,0},
        {0,0,0,0,0,0,0,1,1,0,0,0,0,0,0,0},
    };

    private const int Size = 16;

    // 按 (状态 + 颜色) 缓存 Sprite，整个游戏生命周期只生成一次
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Generate(HeartState state, Color outline, Color fill, Color highlight, Color emptyOutline)
    {
        string key = $"{state}|{ColorKey(outline)}|{ColorKey(fill)}|{ColorKey(highlight)}|{ColorKey(emptyOutline)}";

        if (cache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        Sprite sprite = BuildSprite(state, outline, fill, highlight, emptyOutline);
        cache[key] = sprite;
        return sprite;
    }

    static Sprite BuildSprite(HeartState state, Color outline, Color fill, Color highlight, Color emptyOutline)
    {
        Texture2D tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0, 0, 0, 0);
        int halfX = Size / 2;

        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                int py = Size - 1 - y;
                int v = Pattern[py, x];
                Color c = clear;

                switch (v)
                {
                    case 0:
                        c = clear;
                        break;

                    case 1:
                        if (state == HeartState.Full) c = outline;
                        else if (state == HeartState.Half) c = (x < halfX) ? outline : emptyOutline;
                        else c = emptyOutline;
                        break;

                    case 2:
                        if (state == HeartState.Full) c = fill;
                        else if (state == HeartState.Half && x < halfX) c = fill;
                        break;

                    case 3:
                        if (state == HeartState.Full) c = highlight;
                        else if (state == HeartState.Half && x < halfX) c = highlight;
                        break;
                }

                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), 100f);
    }

    static string ColorKey(Color c)
    {
        return $"{c.r:F3}_{c.g:F3}_{c.b:F3}_{c.a:F3}";
    }
}