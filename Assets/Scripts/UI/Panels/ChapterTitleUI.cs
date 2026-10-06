using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ChapterTitleUI : MonoBehaviour
{
    public static ChapterTitleUI Instance;

    [Header("字体")]
    public TMP_FontAsset font;

    [Header("时长")]
    public float fadeInDuration = 0.4f;
    public float textScaleDuration = 0.6f;
    public float holdDuration = 1.8f;
    public float fadeOutDuration = 0.6f;

    [Header("文字大小与起始缩放")]
    public float fontSize = 72f;
    public float startScale = 1.2f;

    [Header("黑屏最终不透明度（0~1）")]
    public float blackAlpha = 0.9f;

    public bool IsPlaying { get; private set; } = false;

    private Image blackOverlay;
    private TextMeshProUGUI titleText;
    private Coroutine playing;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        BuildUI();
        GameLog.Info($"[ChapterTitleUI] 初始化完成, font={(font == null ? "null" : font.name)}");
    }

    void BuildUI()
    {
        GameObject canvasObj = new GameObject("ChapterTitleCanvas", typeof(RectTransform));
        canvasObj.transform.SetParent(transform, false);

        Canvas cv = canvasObj.AddComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 9999;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject overlayObj = new GameObject("ChapterTitleOverlay", typeof(RectTransform));
        overlayObj.transform.SetParent(canvasObj.transform, false);
        blackOverlay = overlayObj.AddComponent<Image>();
        blackOverlay.color = new Color(0, 0, 0, 0);
        blackOverlay.raycastTarget = false;
        RectTransform ort = blackOverlay.rectTransform;
        ort.anchorMin = Vector2.zero;
        ort.anchorMax = Vector2.one;
        ort.offsetMin = Vector2.zero;
        ort.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("ChapterTitleText", typeof(RectTransform));
        textObj.transform.SetParent(overlayObj.transform, false);
        titleText = textObj.AddComponent<TextMeshProUGUI>();
        if (font != null) titleText.font = font;
        titleText.fontSize = fontSize;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1, 1, 1, 0);
        titleText.raycastTarget = false;
        titleText.text = "";
        RectTransform trt = titleText.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }

    public void Play(string chapterName)
    {
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(PlayRoutine(chapterName));
    }

    IEnumerator PlayRoutine(string chapterName)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("chapter_title");

        IsPlaying = true;
        if (blackOverlay != null) blackOverlay.raycastTarget = true;

        if (titleText != null)
        {
            titleText.text = chapterName;
            titleText.rectTransform.localScale = Vector3.one * startScale;
        }

        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeInDuration);
            SetAlpha(blackOverlay, p * blackAlpha);
            SetAlpha(titleText, p);
            yield return null;
        }
        SetAlpha(blackOverlay, blackAlpha);
        SetAlpha(titleText, 1f);

        t = 0f;
        while (t < textScaleDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / textScaleDuration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            if (titleText != null)
                titleText.rectTransform.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, eased);
            yield return null;
        }
        if (titleText != null) titleText.rectTransform.localScale = Vector3.one;

        yield return new WaitForSecondsRealtime(holdDuration);

        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / fadeOutDuration);
            SetAlpha(blackOverlay, blackAlpha * (1f - p));
            SetAlpha(titleText, 1f - p);
            yield return null;
        }
        SetAlpha(blackOverlay, 0f);
        SetAlpha(titleText, 0f);

        if (blackOverlay != null) blackOverlay.raycastTarget = false;
        IsPlaying = false;
    }

    void SetAlpha(Graphic g, float a)
    {
        if (g == null) return;
        Color c = g.color;
        c.a = a;
        g.color = c;
    }
}