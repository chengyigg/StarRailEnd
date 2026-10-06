using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AffectionPopup : MonoBehaviour
{
    [Header("弹出位置的父物体（右上角空物体）")]
    public RectTransform anchor;

    [Header("字体（拖你的字体资源）")]
    public TMP_FontAsset font;

    [Header("样式")]
    public float fontSize = 28f;
    public Color upColor = new Color(1f, 0.6f, 0.7f, 1f);
    public Color downColor = new Color(0.6f, 0.65f, 0.75f, 1f);

    [Header("动画")]
    public float slideDuration = 0.3f;
    public float holdDuration = 2f;
    public float fadeOutDuration = 0.5f;
    public float verticalSpacing = 40f;

    void OnEnable() { AffectionManager.OnAffectionChanged += OnChanged; }
    void OnDisable() { AffectionManager.OnAffectionChanged -= OnChanged; }

    void OnChanged(string character, int oldValue, int newValue)
    {
        int delta = newValue - oldValue;
        if (delta == 0) return;
        StartCoroutine(SpawnPopup(character, delta));
    }

    IEnumerator SpawnPopup(string character, int delta)
    {
        if (anchor == null) yield break;

        GameObject obj = new GameObject("Popup", typeof(RectTransform));
        obj.transform.SetParent(anchor, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Right;
        text.raycastTarget = false;

        string sign = delta > 0 ? "+" : "";
        text.text = $"{character} {sign}{delta}";
        text.color = delta > 0 ? upColor : downColor;

        Vector2 startPos = new Vector2(200, 0);
        Vector2 endPos = Vector2.zero;
        rt.anchoredPosition = startPos;
        Color c = text.color; c.a = 0f; text.color = c;

        float t = 0f;
        while (t < slideDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / slideDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            rt.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
            c.a = p; text.color = c;
            yield return null;
        }
        rt.anchoredPosition = endPos;
        c.a = 1f; text.color = c;

        yield return new WaitForSecondsRealtime(holdDuration);

        t = 0f;
        Vector2 fadeStart = rt.anchoredPosition;
        Vector2 fadeEnd = fadeStart + new Vector2(0, verticalSpacing);
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            rt.anchoredPosition = Vector2.Lerp(fadeStart, fadeEnd, p);
            c.a = 1f - p; text.color = c;
            yield return null;
        }

        Destroy(obj);
    }
}