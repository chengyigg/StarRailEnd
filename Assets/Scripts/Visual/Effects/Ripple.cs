using UnityEngine;
using UnityEngine.UI;

public class Ripple : MonoBehaviour
{
    private Image image;
    private float elapsed;
    private float duration;
    private float startSize;
    private float endSize;
    private Color baseColor;

    public static void Spawn(Transform parent, Vector2 anchoredPos,
        float startSize, float endSize, Color color, float duration)
    {
        GameObject obj = new GameObject("Ripple", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(startSize, startSize);
        rt.anchoredPosition = anchoredPos;

        Image img = obj.AddComponent<Image>();
        img.sprite = RippleSpriteGenerator.GetRing();
        img.color = color;
        img.raycastTarget = false;

        Ripple r = obj.AddComponent<Ripple>();
        r.image = img;
        r.startSize = startSize;
        r.endSize = endSize;
        r.baseColor = color;
        r.duration = duration;
    }
    public static void SpawnInverse(Transform parent, Vector2 anchoredPos,
     float startSize, float endSize, Color color, float duration)
    {
        GameObject obj = new GameObject("Ripple_Inverse", typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(startSize, startSize);
        rt.anchoredPosition = anchoredPos;

        Image img = obj.AddComponent<Image>();
        img.sprite = RippleSpriteGenerator.GetRing();
        img.color = color;
        img.raycastTarget = false;

        Ripple r = obj.AddComponent<Ripple>();
        r.image = img;
        r.startSize = startSize;
        r.endSize = endSize;
        r.baseColor = color;
        r.duration = duration;
    }
    void Update()
    {
        elapsed += Time.unscaledDeltaTime;
        float p = Mathf.Clamp01(elapsed / duration);
        float eased = 1f - Mathf.Pow(1f - p, 3f);   // Ease Out Cubic

        float size = Mathf.Lerp(startSize, endSize, eased);
        image.rectTransform.sizeDelta = new Vector2(size, size);

        Color c = baseColor;
        c.a = baseColor.a * (1f - p);               // µ­³ö
        image.color = c;

        if (p >= 1f) Destroy(gameObject);
    }
}