using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleRipple : MonoBehaviour
{
    [Header("勾选时的涟漪")]
    public float startSize = 10f;
    public float endSize = 80f;
    public Color onColor = new Color(1f, 0.95f, 0.8f, 0.5f);
    public float onDuration = 0.3f;

    [Header("取消勾选时的涟漪（回缩）")]
    public Color offColor = new Color(0.6f, 0.6f, 0.65f, 0.3f);
    public float offDuration = 0.25f;

    private Toggle toggle;
    private RectTransform centerRect;

    void Awake()
    {
        toggle = GetComponent<Toggle>();

        // 优先找 Background 子物体作为中心，找不到就用自己
        Transform bg = transform.Find("Background");
        centerRect = bg != null ? bg.GetComponent<RectTransform>() : GetComponent<RectTransform>();

        toggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        if (centerRect == null) return;

        if (isOn)
        {
            Ripple.Spawn(centerRect, Vector2.zero, startSize, endSize, onColor, onDuration);
        }
        else
        {
            Ripple.SpawnInverse(centerRect, Vector2.zero, endSize, startSize, offColor, offDuration);
        }
    }
}