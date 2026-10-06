using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ExecuteAlways]
public class DialoguePanelStyler : MonoBehaviour
{
    [Header("背景（半透明柔和）")]
    public Color backgroundColor = new Color(0.12f, 0.12f, 0.16f, 0.72f);

    [Header("说话人名字")]
    public TextMeshProUGUI speakerText;
    public Color speakerColor = new Color(0.95f, 0.88f, 0.72f, 1f);
    public float speakerFontSize = 30f;

    [Header("正文")]
    public TextMeshProUGUI dialogueText;
    public Color dialogueColor = new Color(0.92f, 0.92f, 0.9f, 1f);
    public float dialogueFontSize = 26f;

    [Header("正文阴影（柔和）")]
    public bool useShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.35f);
    public Vector2 shadowDistance = new Vector2(0f, -2f);

    void OnValidate() { Apply(); }
    void Awake() { Apply(); }

    public void Apply()
    {
        // 自动获取自己的 Image 当背景
        Image bg = GetComponent<Image>();
        if (bg != null) bg.color = backgroundColor;

        if (speakerText != null)
        {
            speakerText.color = speakerColor;
            speakerText.fontSize = speakerFontSize;
        }

        if (dialogueText != null)
        {
            dialogueText.color = dialogueColor;
            dialogueText.fontSize = dialogueFontSize;

            Shadow s = dialogueText.GetComponent<Shadow>();
            if (useShadow)
            {
                if (s == null) s = dialogueText.gameObject.AddComponent<Shadow>();
                s.effectColor = shadowColor;
                s.effectDistance = shadowDistance;
                s.enabled = true;
            }
            else if (s != null) s.enabled = false;
        }
    }
}