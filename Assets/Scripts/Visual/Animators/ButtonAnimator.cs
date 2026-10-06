using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("缩放")]
    public float hoverScale = 1.06f;
    public float pressScale = 0.97f;

    [Header("悬停上浮距离")]
    public float hoverOffsetY = 5f;

    [Header("颜色")]
    public Color hoverColor = new Color(0.72f, 0.68f, 0.58f);
    public Color pressColor = new Color(0.6f, 0.56f, 0.48f);

    [Header("柔和速度")]
    public float smoothSpeed = 8f;

    [Header("受 LayoutGroup 管理时自动关闭位移")]
    public bool autoDisableOffsetInLayout = true;

    [Header("音效")]
    public string hoverSoundName = "button_hover";
    public string clickSoundName = "button_click";

    private RectTransform rect;
    private Image bgImage;
    private Vector3 baseScale;
    private Vector2 basePos;
    private Color baseColor;

    private Vector3 targetScale;
    private Vector2 targetPos;
    private Color targetColor;

    private bool allowOffset = true;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        bgImage = GetComponent<Image>();
        Recache();

        if (autoDisableOffsetInLayout && GetComponentInParent<LayoutGroup>() != null)
            allowOffset = false;
    }

    /// <summary>
    /// 重新记录基准值（位置、缩放、颜色）。
    /// 入场动画结束后调用一次，把当前状态作为新基准。
    /// </summary>
    public void Recache()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (bgImage == null) bgImage = GetComponent<Image>();

        baseScale = transform.localScale;
        basePos = rect.anchoredPosition;
        baseColor = bgImage != null ? bgImage.color : Color.white;

        targetScale = baseScale;
        targetPos = basePos;
        targetColor = baseColor;
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
        if (allowOffset)
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, t);
        if (bgImage != null)
            bgImage.color = Color.Lerp(bgImage.color, targetColor, t);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        targetScale = baseScale * hoverScale;
        if (allowOffset) targetPos = basePos + Vector2.up * hoverOffsetY;
        targetColor = hoverColor;
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(hoverSoundName))
            AudioManager.Instance.PlaySFX(hoverSoundName);
    }

    public void OnPointerExit(PointerEventData e)
    {
        targetScale = baseScale;
        if (allowOffset) targetPos = basePos;
        targetColor = baseColor;
    }

    public void OnPointerDown(PointerEventData e)
    {
        targetScale = baseScale * pressScale;
        if (allowOffset) targetPos = basePos;
        targetColor = pressColor;
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(clickSoundName))
            AudioManager.Instance.PlaySFX(clickSoundName);
    }

    public void OnPointerUp(PointerEventData e)
    {
        targetScale = baseScale * hoverScale;
        if (allowOffset) targetPos = basePos + Vector2.up * hoverOffsetY;
        targetColor = hoverColor;
    }
}