using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AutoSaveNotifier : MonoBehaviour
{
    [Header("弹出锚点（右上角空物体）")]
    public RectTransform anchor;

    [Header("字体")]
    public TMP_FontAsset font;
    public float fontSize = 24f;
    public Color textColor = new Color(0.85f, 0.9f, 0.8f, 1f);

    [Header("动画")]
    public float slideDuration = 0.25f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 0.5f;

    [Header("冷却（秒，避免连续弹）")]
    public float cooldown = 3f;

    private float lastShowTime = -999f;

    void OnEnable()
    {
        SaveManager.OnAutoSaveTriggered += OnAutoSave;
    }

    void OnDisable()
    {
        SaveManager.OnAutoSaveTriggered -= OnAutoSave;
    }

    void OnAutoSave()
    {
        if (Time.unscaledTime - lastShowTime < cooldown) return;
        lastShowTime = Time.unscaledTime;
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        if (anchor == null) yield break;

        GameObject obj = new GameObject("AutoSaveToast", typeof(RectTransform));
        obj.transform.SetParent(anchor, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(300, 36);
        rt.anchoredPosition = Vector2.zero;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.fontSize = fontSize;
        text.alignment = TextAlignmentOptions.Right;
        text.color = textColor;
        text.raycastTarget = false;
        text.text = "● 已自动存档";

        // 滑入
        Vector2 startPos = new Vector2(180, 0);
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

        // 淡出
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            c.a = 1f - p; text.color = c;
            yield return null;
        }

        Destroy(obj);
    }
}