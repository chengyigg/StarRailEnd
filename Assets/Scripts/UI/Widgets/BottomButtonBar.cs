using UnityEngine;
using UnityEngine.UI;

public class BottomButtonBar : MonoBehaviour
{
    public static BottomButtonBar Instance;

    [Header("底栏容器")]
    public RectTransform barRect;

    [Header("触发区域高度")]
    public float triggerHeight = 120f;

    [Header("隐藏时往下偏移")]
    public float hideOffsetY = 120f;

    [Header("动画速度")]
    public float smoothSpeed = 12f;

    [Header("外部锁定")]
    public bool locked = false;

    private Canvas canvas;
    private Vector2 shownPos;
    private Vector2 hiddenPos;

    void Awake()
    {
        Instance = this;
        if (barRect == null) barRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        shownPos = barRect.anchoredPosition;
        hiddenPos = shownPos + Vector2.down * hideOffsetY;
        barRect.anchoredPosition = hiddenPos;
    }

    void Update()
    {
        if (canvas == null) return;

        bool inside = false;
        if (!locked)
        {
            Vector2 localMouse;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.GetComponent<RectTransform>(),
                    Input.mousePosition,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                    out localMouse))
            {
                Rect rect = canvas.GetComponent<RectTransform>().rect;
                float distanceFromBottom = localMouse.y - rect.yMin;
                inside = distanceFromBottom < triggerHeight;
            }
        }

        Vector2 target = inside ? shownPos : hiddenPos;
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        barRect.anchoredPosition = Vector2.Lerp(barRect.anchoredPosition, target, t);
    }

    public void SetLocked(bool value) { locked = value; }
}