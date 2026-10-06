using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameEndingUI : MonoBehaviour
{
    public static GameEndingUI Instance;

    [Header("字体")]
    public TMP_FontAsset font;

    [Header("副标题文字")]
    public string hintTextStr = "感谢游玩";
    public string subtitleTextStr = "《星轨尽头》";

    [Header("结局名样式")]
    public float endingFontSize = 110f;
    public float endingStartScale = 1.15f;
    public Color endingTopColor = new Color(1f, 0.95f, 0.75f);
    public Color endingBottomColor = new Color(0.85f, 0.65f, 0.4f);

    [Header("装饰线")]
    public Color lineColor = new Color(0.85f, 0.75f, 0.5f, 0.9f);
    public float lineMaxWidth = 520f;
    public float lineHeight = 2f;
    public float lineGapY = 90f;

    [Header("副标题样式")]
    public float hintFontSize = 30f;
    public Color hintColor = new Color(0.85f, 0.82f, 0.75f);
    public float subtitleFontSize = 22f;
    public Color subtitleColor = new Color(0.6f, 0.58f, 0.55f);

    [Header("时长")]
    public float blackFadeIn = 1.5f;
    public float titleFadeIn = 0.7f;
    public float titleScaleDuration = 1.0f;
    public float lineExpandDuration = 1.2f;
    public float hintFadeIn = 0.8f;
    public float holdDuration = 3.0f;
    public float fadeOutDuration = 2.0f;

    private Image blackOverlay;
    private TextMeshProUGUI endingText;
    private TextMeshProUGUI hintText;
    private TextMeshProUGUI subtitleText;
    private Image lineTop;
    private Image lineBottom;
    private Coroutine playing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
    }

    void BuildUI()
    {
        // 独立根 Canvas，不挂在 EndingManager 下，避免被父 Canvas 约束
        GameObject canvasObj = new GameObject("EndingCanvas", typeof(RectTransform));
        canvasObj.transform.SetParent(null, false);   // ★ 关键：脱离父级

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 强制铺满全屏
        RectTransform crt = canvasObj.GetComponent<RectTransform>();
        crt.anchorMin = Vector2.zero;
        crt.anchorMax = Vector2.one;
        crt.offsetMin = Vector2.zero;
        crt.offsetMax = Vector2.zero;
        crt.localPosition = Vector3.zero;
        crt.localScale = Vector3.one;

        // 纯黑背景
        GameObject overlayObj = new GameObject("BlackOverlay", typeof(RectTransform));
        overlayObj.transform.SetParent(canvasObj.transform, false);
        blackOverlay = overlayObj.AddComponent<Image>();
        blackOverlay.color = new Color(0, 0, 0, 0);
        blackOverlay.raycastTarget = false;   // ★ 初始不拦截
        RectTransform ort = blackOverlay.rectTransform;
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.offsetMin = Vector2.zero;
        ort.offsetMax = Vector2.zero;

        // 上方装饰线（居中，宽度从 0 → lineMaxWidth）
        lineTop = CreateLine(overlayObj.transform, "LineTop", lineGapY);
        lineBottom = CreateLine(overlayObj.transform, "LineBottom", -lineGapY);

        // 结局名
        GameObject endObj = new GameObject("EndingText", typeof(RectTransform));
        endObj.transform.SetParent(overlayObj.transform, false);
        endingText = endObj.AddComponent<TextMeshProUGUI>();
        if (font != null) endingText.font = font;
        endingText.fontSize = endingFontSize;
        endingText.fontStyle = FontStyles.Bold;
        endingText.alignment = TextAlignmentOptions.Center;
        endingText.raycastTarget = false;
        endingText.text = "";
        endingText.enableVertexGradient = true;
        endingText.colorGradient = new VertexGradient(endingTopColor, endingTopColor, endingBottomColor, endingBottomColor);
        RectTransform ert = endingText.rectTransform;
        ert.anchorMin = new Vector2(0, 0.5f);
        ert.anchorMax = new Vector2(1, 0.5f);
        ert.pivot = new Vector2(0.5f, 0.5f);
        ert.sizeDelta = new Vector2(0, 200);
        ert.anchoredPosition = Vector2.zero;

        // 副标题 "感谢游玩"
        GameObject hintObj = new GameObject("HintText", typeof(RectTransform));
        hintObj.transform.SetParent(overlayObj.transform, false);
        hintText = hintObj.AddComponent<TextMeshProUGUI>();
        if (font != null) hintText.font = font;
        hintText.fontSize = hintFontSize;
        hintText.alignment = TextAlignmentOptions.Center;
        hintText.color = new Color(hintColor.r, hintColor.g, hintColor.b, 0f);
        hintText.raycastTarget = false;
        hintText.text = hintTextStr;
        RectTransform hrt = hintText.rectTransform;
        hrt.anchorMin = new Vector2(0, 0.5f);
        hrt.anchorMax = new Vector2(1, 0.5f);
        hrt.pivot = new Vector2(0.5f, 0.5f);
        hrt.sizeDelta = new Vector2(0, 60);
        hrt.anchoredPosition = new Vector2(0, -160);

        // 游戏名
        GameObject subObj = new GameObject("SubtitleText", typeof(RectTransform));
        subObj.transform.SetParent(overlayObj.transform, false);
        subtitleText = subObj.AddComponent<TextMeshProUGUI>();
        if (font != null) subtitleText.font = font;
        subtitleText.fontSize = subtitleFontSize;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color = new Color(subtitleColor.r, subtitleColor.g, subtitleColor.b, 0f);
        subtitleText.raycastTarget = false;
        subtitleText.text = subtitleTextStr;
        RectTransform srt = subtitleText.rectTransform;
        srt.anchorMin = new Vector2(0, 0.5f);
        srt.anchorMax = new Vector2(1, 0.5f);
        srt.pivot = new Vector2(0.5f, 0.5f);
        srt.sizeDelta = new Vector2(0, 40);
        srt.anchoredPosition = new Vector2(0, -210);
    }

    Image CreateLine(Transform parent, string name, float y)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = new Color(lineColor.r, lineColor.g, lineColor.b, 0f);
        img.raycastTarget = false;
        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(0, lineHeight);
        rt.anchoredPosition = new Vector2(0, y);
        return img;
    }

    public void PlayEnding(string endingName)
    {
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(PlayRoutine(endingName));
    }

    IEnumerator PlayRoutine(string endingName)
    {
        // 初始化
        endingText.text = endingName;
        endingText.rectTransform.localScale = Vector3.one * endingStartScale;
        SetAlpha(endingText, 0f);

        SetAlpha(lineTop, 0f);
        SetAlpha(lineBottom, 0f);
        lineTop.rectTransform.sizeDelta = new Vector2(0, lineHeight);
        lineBottom.rectTransform.sizeDelta = new Vector2(0, lineHeight);

        SetAlpha(hintText, 0f);
        SetAlpha(subtitleText, 0f);

        // 1. 黑幕渐入
        float t = 0f;
        while (t < blackFadeIn)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(blackOverlay, Mathf.Clamp01(t / blackFadeIn));
            yield return null;
        }
        SetAlpha(blackOverlay, 1f);

        // 2. 结局名淡入
        t = 0f;
        while (t < titleFadeIn)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(endingText, Mathf.Clamp01(t / titleFadeIn));
            yield return null;
        }
        SetAlpha(endingText, 1f);

        // 3. 结局名缩放
        t = 0f;
        while (t < titleScaleDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / titleScaleDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            endingText.rectTransform.localScale = Vector3.Lerp(Vector3.one * endingStartScale, Vector3.one, eased);
            yield return null;
        }
        endingText.rectTransform.localScale = Vector3.one;

        // 4. 装饰线展开
        t = 0f;
        while (t < lineExpandDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / lineExpandDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            float w = Mathf.Lerp(0f, lineMaxWidth, eased);
            lineTop.rectTransform.sizeDelta = new Vector2(w, lineHeight);
            lineBottom.rectTransform.sizeDelta = new Vector2(w, lineHeight);
            SetAlpha(lineTop, p * lineColor.a);
            SetAlpha(lineBottom, p * lineColor.a);
            yield return null;
        }

        // 5. 副标题淡入
        Vector2 hintStart = hintText.rectTransform.anchoredPosition;
        Vector2 hintEnd = hintStart + new Vector2(0, 10);
        t = 0f;
        while (t < hintFadeIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / hintFadeIn);
            SetAlpha(hintText, p);
            hintText.rectTransform.anchoredPosition = Vector2.Lerp(hintStart, hintEnd, p);
            yield return null;
        }

        // 6. 游戏名淡入
        yield return new WaitForSecondsRealtime(0.4f);
        t = 0f;
        while (t < hintFadeIn)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / hintFadeIn);
            SetAlpha(subtitleText, p);
            yield return null;
        }

        // 7. 停留
        yield return new WaitForSecondsRealtime(holdDuration);

        // 8. 全部淡出
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = 1f - Mathf.Clamp01(t / fadeOutDuration);
            SetAlpha(endingText, p);
            SetAlpha(hintText, p);
            SetAlpha(subtitleText, p);
            SetAlpha(lineTop, p * lineColor.a);
            SetAlpha(lineBottom, p * lineColor.a);
            yield return null;
        }

        // 9. 回主菜单
        if (SceneFader.Instance != null)
            SceneFader.Instance.LoadScene("SampleScene");
        else
            SceneManager.LoadScene("SampleScene");
    }

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
}