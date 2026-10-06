using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 负责立绘的显示 / 移动 / 淡入淡出。
/// </summary>
public class DialoguePortraitView : MonoBehaviour
{
    [Header("立绘组件")]
    public Image portraitImage;
    public CanvasGroup portraitCanvasGroup;
    public float portraitFadeDuration = 0.3f;

    [Header("立绘位置")]
    public float portraitLeftX = -450f;
    public float portraitRightX = 450f;
    public float portraitCenterX = 0f;
    public float portraitY = 0f;

    /// <summary>整段序列里只有一个角色带立绘时，强制居中。</summary>
    private bool singlePortraitMode = false;

    private Coroutine portraitFadeCoroutine;
    private Sprite currentPortraitSprite = null;

    void Awake()
    {
        if (portraitCanvasGroup != null)
            portraitCanvasGroup.alpha = 0f;
    }

    public void SetSingleMode(bool single)
    {
        singlePortraitMode = single;
    }

    /// <summary>显示立绘。sprite 为 null 或 pos 为 None 时会淡出。</summary>
    public void Show(Sprite sprite, PortraitPosition pos)
    {
        if (portraitImage == null || portraitCanvasGroup == null) return;

        if (singlePortraitMode && pos != PortraitPosition.None)
            pos = PortraitPosition.Center;

        if (sprite == null || pos == PortraitPosition.None)
        {
            if (portraitFadeCoroutine != null) StopCoroutine(portraitFadeCoroutine);
            portraitFadeCoroutine = StartCoroutine(FadeOut());
            currentPortraitSprite = null;
            return;
        }

        float targetX = portraitCenterX;
        switch (pos)
        {
            case PortraitPosition.Left: targetX = portraitLeftX; break;
            case PortraitPosition.Right: targetX = portraitRightX; break;
            case PortraitPosition.Center: targetX = portraitCenterX; break;
        }
        Vector2 targetPos = new Vector2(targetX, portraitY);

        bool sameSprite = (currentPortraitSprite == sprite);

        if (portraitFadeCoroutine != null) StopCoroutine(portraitFadeCoroutine);

        if (sameSprite)
            portraitFadeCoroutine = StartCoroutine(Move(targetPos));
        else
            portraitFadeCoroutine = StartCoroutine(FadeIn(sprite, targetPos));

        currentPortraitSprite = sprite;
    }

    IEnumerator Move(Vector2 targetPos)
    {
        RectTransform rt = portraitImage.rectTransform;
        Vector2 start = rt.anchoredPosition;
        float t = 0f;
        float dur = portraitFadeDuration;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            rt.anchoredPosition = Vector2.Lerp(start, targetPos, t / dur);
            yield return null;
        }
        rt.anchoredPosition = targetPos;
    }

    IEnumerator FadeOut()
    {
        float start = portraitCanvasGroup.alpha;
        float t = 0f;
        float dur = portraitFadeDuration * 0.5f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        portraitCanvasGroup.alpha = 0f;
    }

    IEnumerator FadeIn(Sprite newSprite, Vector2 targetPos)
    {
        float start = portraitCanvasGroup.alpha;
        float t = 0f;
        float dur = portraitFadeDuration * 0.5f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(start, 0f, t / dur);
            yield return null;
        }
        portraitCanvasGroup.alpha = 0f;

        portraitImage.sprite = newSprite;
        portraitImage.rectTransform.anchoredPosition = targetPos;

        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            portraitCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / dur);
            yield return null;
        }
        portraitCanvasGroup.alpha = 1f;
    }
}