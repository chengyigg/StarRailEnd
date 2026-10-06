using UnityEngine;

public static class RippleSpriteGenerator
{
    private static Sprite _ring;

    public static Sprite GetRing(int size = 128, float ringWidthRatio = 0.18f)
    {
        if (_ring != null) return _ring;
        _ring = Generate(size, ringWidthRatio);
        return _ring;
    }

    private static Sprite Generate(int size, float ringWidthRatio)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        float half = size / 2f;
        float outer = half - 2f;                          // Íâ°ë¾¶
        float inner = outer * (1f - ringWidthRatio);      // ÄÚ°ë¾¶
        float aa = 1.5f;                                   // ¿¹¾â³Ý¿í¶È

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half + 0.5f;
                float dy = y - half + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);

                float a = 0f;
                if (d <= outer && d >= inner)
                {
                    a = 1f;
                    if (d > outer - aa) a *= (outer - d) / aa;
                    else if (d < inner + aa) a *= (d - inner) / aa;
                }

                pixels[y * size + x] = new Color(1, 1, 1, Mathf.Clamp01(a));
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}