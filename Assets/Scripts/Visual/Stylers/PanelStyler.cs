using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class PanelStyler : MonoBehaviour
{
    [Header("背景")]
    public Image backgroundImage;
    public Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.85f);

    [Header("圆角（需要 Sprite）")]
    public Sprite roundedSprite;
    public Image.Type imageType = Image.Type.Sliced;

    [Header("阴影")]
    public bool useShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public Vector2 shadowDistance = new Vector2(4f, -4f);

    [Header("边框（可选）")]
    public bool useOutline = false;
    public Color outlineColor = new Color(0.6f, 0.55f, 0.45f, 0.5f);
    public Vector2 outlineDistance = new Vector2(2f, -2f);

    void OnValidate() { Apply(); }
    void Awake() { Apply(); }

    [ContextMenu("立即应用样式")]
    public void Apply()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // 背景
        if (backgroundImage != null)
        {
            backgroundImage.color = backgroundColor;
            if (roundedSprite != null)
            {
                backgroundImage.sprite = roundedSprite;
                backgroundImage.type = imageType;
            }
        }

        // 阴影
        if (backgroundImage != null)
        {
            Shadow sh = backgroundImage.GetComponent<Shadow>();
            if (useShadow)
            {
                if (sh == null) sh = backgroundImage.gameObject.AddComponent<Shadow>();
                sh.effectColor = shadowColor;
                sh.effectDistance = shadowDistance;
                sh.enabled = true;
            }
            else if (sh != null) sh.enabled = false;
        }

        // 边框
        if (backgroundImage != null)
        {
            Outline ol = backgroundImage.GetComponent<Outline>();
            if (useOutline)
            {
                if (ol == null) ol = backgroundImage.gameObject.AddComponent<Outline>();
                ol.effectColor = outlineColor;
                ol.effectDistance = outlineDistance;
                ol.enabled = true;
            }
            else if (ol != null) ol.enabled = false;
        }
    }
}