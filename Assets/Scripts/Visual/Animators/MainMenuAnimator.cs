using UnityEngine;
using System.Collections;

public class MainMenuAnimator : MonoBehaviour
{
    [Header("要动画的元素（按顺序出场）")]
    public RectTransform[] elements;

    [Header("动画参数")]
    public float startDelay = 0.7f;   // 等 SceneFader 淡入
    public float duration = 0.35f;    // 每个元素的动画时长
    public float eachDelay = 0.05f;   // 元素之间的间隔

    void Awake()
    {
        foreach (var e in elements)
        {
            if (e == null) continue;
            CanvasGroup cg = e.GetComponent<CanvasGroup>();
            if (cg == null) cg = e.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
        }
    }

    void Start()
    {
        StartCoroutine(PlayAll());
    }

    IEnumerator PlayAll()
    {
        // 等场景黑屏淡出
        float t = 0f;
        while (t < startDelay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 一个接一个播
        foreach (var element in elements)
        {
            if (element == null) continue;

            CanvasGroup cg = element.GetComponent<CanvasGroup>();
            if (cg == null) cg = element.gameObject.AddComponent<CanvasGroup>();

            float et = 0f;
            while (et < duration)
            {
                et += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(et / duration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                cg.alpha = p;
                yield return null;
            }
            cg.alpha = 1f;

            // 元素之间的间隔
            float gap = 0f;
            while (gap < eachDelay)
            {
                gap += Time.unscaledDeltaTime;
                yield return null;
            }
        }
    }
}