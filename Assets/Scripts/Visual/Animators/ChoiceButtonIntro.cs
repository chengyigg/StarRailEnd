using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class ChoiceButtonIntro : MonoBehaviour
{
    [Header("时长")]
    public float duration = 0.25f;

    [Header("向下滑入距离（像素）")]
    public float slideFromY = -30f;

    [Header("起始缩放")]
    public float scaleFrom = 0.9f;

    private CanvasGroup cg;
    private RectTransform rect;
    private ButtonAnimator buttonAnimator;
    private Vector2 targetPos;
    private Vector3 targetScale;
    private bool parentHasLayout;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        buttonAnimator = GetComponent<ButtonAnimator>();
        parentHasLayout = GetComponentInParent<LayoutGroup>() != null;

        targetPos = rect.anchoredPosition;
        targetScale = transform.localScale;

        if (buttonAnimator != null) buttonAnimator.enabled = false;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(PlayIntro());
    }

    IEnumerator PlayIntro()
    {
        cg.alpha = 0f;
        transform.localScale = targetScale * scaleFrom;

        Vector2 startPos = parentHasLayout ? targetPos : targetPos + Vector2.up * slideFromY;
        if (!parentHasLayout) rect.anchoredPosition = startPos;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            cg.alpha = p;
            transform.localScale = Vector3.Lerp(targetScale * scaleFrom, targetScale, eased);
            if (!parentHasLayout)
                rect.anchoredPosition = Vector2.Lerp(startPos, targetPos, eased);
            yield return null;
        }

        cg.alpha = 1f;
        transform.localScale = targetScale;
        if (!parentHasLayout) rect.anchoredPosition = targetPos;

        if (buttonAnimator != null)
        {
            buttonAnimator.Recache();
            buttonAnimator.enabled = true;
        }
    }
}