using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
[ExecuteAlways]
public class ButtonStyler : MonoBehaviour
{
    [Header("背景色")]
    public Color backgroundColor = new Color(0.106f, 0.149f, 0.231f, 0.85f);

    [Header("文字")]
    public Color textColor = new Color(0.961f, 0.941f, 0.902f, 1f);
    public float fontSize = 28f;
    public bool bold = true;
    public float characterSpacing = 4f;

    [Header("阴影（柔和）")]
    public bool useShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.3f);
    public Vector2 shadowDistance = new Vector2(0f, -2f);

    [Header("圆角（可选）")]
    public Sprite roundedSprite;

    private Image bgImage;
    private TextMeshProUGUI label;
    private Shadow shadow;

    void OnEnable() { Apply(); }
    void OnValidate() { Apply(); }

    [ContextMenu("立即应用样式")]
    public void Apply()
    {
        if (this == null) return;

        bgImage = GetComponent<Image>();
        if (bgImage != null)
        {
            bgImage.color = backgroundColor;
            if (roundedSprite != null)
            {
                bgImage.sprite = roundedSprite;
                bgImage.type = Image.Type.Sliced;
            }
        }

        label = GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            label.color = textColor;
            label.fontSize = fontSize;
            label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            label.characterSpacing = characterSpacing;
        }

        shadow = GetComponent<Shadow>();
        if (useShadow)
        {
            if (shadow == null) shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDistance;
            shadow.enabled = true;
        }
        else if (shadow != null) shadow.enabled = false;
    }
}