using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum PanelSlideDirection
{
    None,           // 只缩放/淡入
    FromBottom,     // 从下往上滑入
    FromTop,        // 从上往下滑入
    FromLeft,       // 从左往右滑入
    FromRight       // 从右往左滑入
}

[RequireComponent(typeof(RectTransform))]
public class PanelAnimator : MonoBehaviour
{
    [Header("缩放")]
    public float scaleFrom = 0.9f;
    public float duration = 0.25f;

    [Header("滑入方向")]
    public PanelSlideDirection direction = PanelSlideDirection.None;
    public float slideDistance = 200f;

    [Header("是否淡入")]
    public bool fadeIn = true;

    [Header("缓动曲线（Ease Out Cubic）")]
    public bool useEasing = true;

    private RectTransform rect;
    private CanvasGroup cg;
    private Vector3 targetScale;
    private Vector2 targetPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        targetScale = rect.localScale;
        targetPos = rect.anchoredPosition;

        cg = GetComponent<CanvasGroup>();
        if (cg == null && fadeIn) cg = gameObject.AddComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        // 起始状态
        rect.localScale = targetScale * scaleFrom;
        Vector2 startPos = targetPos + GetOffset();
        rect.anchoredPosition = startPos;

        if (cg != null) cg.alpha = 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = useEasing ? (1f - Mathf.Pow(1f - p, 3f)) : p;

            rect.localScale = Vector3.Lerp(targetScale * scaleFrom, targetScale, eased);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, eased);
            if (cg != null) cg.alpha = eased;

            yield return null;
        }

        rect.localScale = targetScale;
        rect.anchoredPosition = targetPos;
        if (cg != null) cg.alpha = 1f;
    }

    Vector2 GetOffset()
    {
        switch (direction)
        {
            case PanelSlideDirection.FromBottom: return new Vector2(0, -slideDistance);
            case PanelSlideDirection.FromTop: return new Vector2(0, slideDistance);
            case PanelSlideDirection.FromLeft: return new Vector2(-slideDistance, 0);
            case PanelSlideDirection.FromRight: return new Vector2(slideDistance, 0);
            default: return Vector2.zero;
        }
    }
}