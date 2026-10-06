using UnityEngine;
using TMPro;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(TextMeshProUGUI))]
public class TextStyle : MonoBehaviour
{
    public enum Preset
    {
        标题,
        副标题,
        按钮文字,
        说明文字
    }

    [Header("预设（切换时自动应用一次）")]
    public Preset preset = Preset.按钮文字;

    [Header("颜色")]
    public Color color = new Color(0.961f, 0.941f, 0.902f);
    public bool useGradient = false;
    public Color colorTop = new Color(0.961f, 0.902f, 0.722f);      // #F5E6B8 浅金
    public Color colorBottom = new Color(0.788f, 0.588f, 0.239f);   // #C9963D 暗金

    [Header("字体")]
    public float fontSize = 28f;
    public FontStyles fontStyle = FontStyles.Bold;
    public float characterSpacing = 0f;

    [Header("阴影")]
    public bool useShadow = false;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public Vector2 shadowDistance = new Vector2(0f, -2f);

    [SerializeField, HideInInspector]
    private int lastPresetIndex = -1;

    private TextMeshProUGUI text;

    void OnEnable() { Apply(); }

    void OnValidate()
    {
        if (lastPresetIndex != (int)preset)
        {
            lastPresetIndex = (int)preset;
            ApplyPresetValues();
        }
        Apply();
    }

    [ContextMenu("立即应用样式")]
    public void Apply()
    {
        text = GetComponent<TextMeshProUGUI>();
        if (text == null) return;

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.characterSpacing = characterSpacing;

        if (useGradient)
        {
            text.enableVertexGradient = true;
            text.colorGradient = new VertexGradient(colorTop, colorTop, colorBottom, colorBottom);
        }
        else
        {
            text.enableVertexGradient = false;
            text.color = color;
        }

        var shadow = text.GetComponent<Shadow>();
        if (useShadow)
        {
            if (shadow == null) shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDistance;
            shadow.enabled = true;
        }
        else if (shadow != null) shadow.enabled = false;
    }

    void ApplyPresetValues()
    {
        switch (preset)
        {
            case Preset.标题:
                color = new Color(0.961f, 0.941f, 0.902f);            // 米白（渐变关闭时用）
                useGradient = true;
                colorTop = new Color(0.961f, 0.902f, 0.722f);         // #F5E6B8 浅金
                colorBottom = new Color(0.788f, 0.588f, 0.239f);      // #C9963D 暗金
                fontSize = 96f;                                        // 主标题大字号
                fontStyle = FontStyles.Bold;
                characterSpacing = 20f;                                // 字距拉大 30%
                useShadow = true;
                shadowColor = new Color(0.6f, 0.45f, 0.2f, 0.5f);
                shadowDistance = new Vector2(0f, -4f);
                break;

            case Preset.副标题:
                color = new Color(0.557f, 0.792f, 0.902f);            // #8ECAE6 淡青
                useGradient = false;
                fontSize = 24f;                                        // 主标题 1/4
                fontStyle = FontStyles.Normal;
                characterSpacing = 8f;
                useShadow = false;
                break;

            case Preset.按钮文字:
                color = new Color(0.35f, 0.32f, 0.3f);
                useGradient = false;
                fontSize = 28f;
                fontStyle = FontStyles.Bold;
                characterSpacing = 0f;
                useShadow = false;
                break;

            case Preset.说明文字:
                color = new Color(0.7f, 0.68f, 0.65f);
                useGradient = false;
                fontSize = 20f;
                fontStyle = FontStyles.Italic;
                characterSpacing = 0f;
                useShadow = false;
                break;
        }
    }
}