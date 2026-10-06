using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [Header("淡入淡出时长")]
    public float fadeDuration = 0.5f;

    [Header("遮罩颜色")]
    public Color fadeColor = Color.black;

    private Image fadeImage;
    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        gameObject.AddComponent<GraphicRaycaster>();

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(transform, false);
        fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        fadeImage.raycastTarget = false;

        RectTransform rt = fadeImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartFadeIn();
    }

    void StartFadeIn()
    {
        StopFade();
        fadeCoroutine = StartCoroutine(FadeIn());
    }

    /// <summary>停止当前正在跑的淡入/淡出，不触发任何回调。</summary>
    void StopFade()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
    }

    IEnumerator FadeIn()
    {
        SetAlpha(1f);
        float t = fadeDuration;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        SetAlpha(0f);
        fadeCoroutine = null;
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        fadeCoroutine = null;
        SceneManager.LoadScene(sceneName);
    }

    void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
        fadeImage.raycastTarget = a > 0.01f;
    }

    /// <summary>淡出并加载场景。所有跳转都应该走这里。</summary>
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("scene_transition");

        StopFade();
        fadeCoroutine = StartCoroutine(FadeOutAndLoad(sceneName));
    }
}