using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class PointerRipple : MonoBehaviour, IPointerClickHandler
{
    [Header("涟漪尺寸")]
    public float startSize = 20f;
    public float endSize = 200f;

    [Header("颜色（含透明度）")]
    public Color rippleColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("时长")]
    public float duration = 0.4f;

    public void OnPointerClick(PointerEventData e)
    {
        RectTransform rect = GetComponent<RectTransform>();

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            e.position,
            e.pressEventCamera,
            out localPos);

        Ripple.Spawn(rect, localPos, startSize, endSize, rippleColor, duration);
    }
}