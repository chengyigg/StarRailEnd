using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责对话背景图的切换淡入淡出，以及内心独白模式的色调滤镜。
/// </summary>
public class DialogueBackgroundView : MonoBehaviour
{
    [Header("两张背景图层（交叉淡入淡出）")]
    public Image backgroundImageA;
    public Image backgroundImageB;
    public float bgFadeDuration = 0.5f;

    [Header("内心独白滤镜")]
    public Color innerMonologueTint = new Color(0.55f, 0.45f, 0.75f);
    public float tintFadeDuration = 0.4f;

    private bool usingBackgroundA = true;
    private Sprite currentSprite = null;
    private Coroutine bgFadeCoroutine;
    private Coroutine tintCoroutine;
    private bool isTintActive = false;

    void Awake()
    {
        if (backgroundImageB != null)
        {
            Color c = backgroundImageB.color;
            c.a = 0f;
            backgroundImageB.color = c;
        }
    }

    /// <summary>切背景。若目标 sprite 与当前相同则跳过。</summary>
    public void ShowBackground(Sprite sprite)
    {
        if (sprite == null) return;
        if (backgroundImageA == null || backgroundImageB == null) return;
        if (currentSprite == sprite) return;

        currentSprite = sprite;

        if (bgFadeCoroutine != null) StopCoroutine(bgFadeCoroutine);
        bgFadeCoroutine = StartCoroutine(FadeRoutine(sprite));
    }

    /// <summary>切换内心独白色调。重复调用相同状态会被忽略。</summary>
    public void SetMonologueMode(bool on)
    {
        if (on == isTintActive) return;
        isTintActive = on;

        if (tintCoroutine != null) StopCoroutine(tintCoroutine);
        tintCoroutine = StartCoroutine(FadeTintRoutine(on ? innerMonologueTint : Color.white));
    }

    IEnumerator FadeRoutine(Sprite newBg)
    {
        Image fadeIn = usingBackgroundA ? backgroundImageB : backgroundImageA;
        Image fadeOut = usingBackgroundA ? backgroundImageA : backgroundImageB;

        fadeIn.sprite = newBg;
        fadeIn.rectTransform.localScale = Vector3.one * 1.05f;
        fadeOut.rectTransform.localScale = Vector3.one;

        Color cIn = fadeIn.color; cIn.a = 0f; fadeIn.color = cIn;
        Color cOut = fadeOut.color; cOut.a = 1f; fadeOut.color = cOut;

        float t = 0f;
        while (t < bgFadeDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / bgFadeDuration);
            float eased = p * p * (3f - 2f * p);
            Color ci = fadeIn.color; ci.a = eased; fadeIn.color = ci;
            Color co = fadeOut.color; co.a = 1f - eased; fadeOut.color = co;
            fadeIn.rectTransform.localScale = Vector3.Lerp(Vector3.one * 1.05f, Vector3.one, eased);
            yield return null;
        }

        Color finalIn = fadeIn.color; finalIn.a = 1f; fadeIn.color = finalIn;
        Color finalOut = fadeOut.color; finalOut.a = 0f; fadeOut.color = finalOut;
        fadeIn.rectTransform.localScale = Vector3.one;
        usingBackgroundA = !usingBackgroundA;
    }

    IEnumerator FadeTintRoutine(Color targetRGB)
    {
        Color startA = GetBgRGB(backgroundImageA);
        Color startB = GetBgRGB(backgroundImageB);

        float t = 0f;
        while (t < tintFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / tintFadeDuration);
            ApplyBgRGB(backgroundImageA, Color.Lerp(startA, targetRGB, p));
            ApplyBgRGB(backgroundImageB, Color.Lerp(startB, targetRGB, p));
            yield return null;
        }
        ApplyBgRGB(backgroundImageA, targetRGB);
        ApplyBgRGB(backgroundImageB, targetRGB);
    }

    Color GetBgRGB(Image img)
    {
        if (img == null) return Color.white;
        Color c = img.color;
        return new Color(c.r, c.g, c.b, 1f);
    }

    void ApplyBgRGB(Image img, Color rgb)
    {
        if (img == null) return;
        Color c = img.color;
        img.color = new Color(rgb.r, rgb.g, rgb.b, c.a);
    }
}