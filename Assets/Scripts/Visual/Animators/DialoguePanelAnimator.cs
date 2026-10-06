using UnityEngine;
using System.Collections;

public class DialoguePanelAnimator : MonoBehaviour
{
    [Header("滑入距离")]
    public float slideDistance = 150f;

    [Header("起始缩放")]
    public float scaleFrom = 0.95f;

    [Header("时长")]
    public float duration = 0.3f;

    private RectTransform rect;
    private Vector2 targetPos;
    private Vector2 hiddenPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        targetPos = rect.anchoredPosition;
        hiddenPos = targetPos + Vector2.down * slideDistance;
        rect.anchoredPosition = hiddenPos;
    }

    void OnEnable()
    {
        StopAllCoroutines();
        rect.anchoredPosition = hiddenPos;
        rect.localScale = Vector3.one * scaleFrom;
        StartCoroutine(SlideIn());
    }

    IEnumerator SlideIn()
    {
        float t = 0f;
        Vector2 startPos = rect.anchoredPosition;
        Vector3 startScale = rect.localScale;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - p, 3f);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, eased);
            rect.localScale = Vector3.Lerp(startScale, Vector3.one, eased);
            yield return null;
        }
        rect.anchoredPosition = targetPos;
        rect.localScale = Vector3.one;
    }
}