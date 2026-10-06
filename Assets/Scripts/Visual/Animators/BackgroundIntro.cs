using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 主菜单背景出场动画：从稍微放大 + 淡入，缓缓缩到正常尺寸。
/// 挂在主菜单背景 Image 上。
/// </summary>
[RequireComponent(typeof(Image))]
public class BackgroundIntro : MonoBehaviour
{
    [Header("时机")]
    public float startDelay = 0.7f;   // 等 SceneFader 黑屏淡出

    [Header("动画参数")]
    public float duration = 1.2f;     // 比按钮略长，更有层次
    public float startScale = 1.08f;  // 从 1.08 倍开始缩回来

    private Image image;
    private RectTransform rect;

    void Awake()
    {
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        // 初始状态
        Color c = image.color; c.a = 0f; image.color = c;
        rect.localScale = Vector3.one * startScale;

        // 等待 SceneFader
        float t = 0f;
        while (t < startDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 淡入 + 缩小
        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);

            c.a = p;
            image.color = c;
            rect.localScale = Vector3.Lerp(Vector3.one * startScale, Vector3.one, eased);
            yield return null;
        }

        c.a = 1f;
        image.color = c;
        rect.localScale = Vector3.one;
    }
}