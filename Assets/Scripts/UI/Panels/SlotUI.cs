using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public int slotIndex;
    public Image thumbnail;
    public Image selectionBorder;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI chapterText;
    public Button button;

    [Header("选中效果")]
    public float selectedScale = 1.06f;
    public float smoothSpeed = 12f;

    private SavePanel parentPanel;
    private RectTransform rect;
    private Vector3 baseScale;
    private Vector3 targetScale;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        baseScale = rect.localScale;
        targetScale = baseScale;
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);
        rect.localScale = Vector3.Lerp(rect.localScale, targetScale, t);
    }

    public void Init(SavePanel panel)
    {
        parentPanel = panel;
        if (button == null)
        {
            GameLog.Error($"[SlotUI] Slot_{slotIndex} 的 button 引用为空！");
            return;
        }
        button.onClick.AddListener(() =>
        {
            if (parentPanel != null)
                parentPanel.OnSlotClicked(slotIndex);
        });
    }

    public void SetData(SaveData data)
    {
        if (data == null)
        {
            if (timeText != null) timeText.text = "空存档";
            if (chapterText != null) chapterText.text = "";
            if (thumbnail != null)
            {
                thumbnail.sprite = null;
                thumbnail.color = new Color(1, 1, 1, 0.2f);
            }
        }
        else
        {
            if (timeText != null)
            {
                string name = string.IsNullOrEmpty(data.playerName) ? "" : $" · {data.playerName}";
                timeText.text = data.saveTime + name;
            }

            if (chapterText != null)
            {
                string title = data.chapterName;
                if (string.IsNullOrEmpty(title)) title = data.sequenceID;
                if (string.IsNullOrEmpty(title)) title = "未知";
                chapterText.text = title;
            }

            if (thumbnail != null)
            {
                Sprite sprite = SaveScreenshotLoader.Load(data.screenshotFile);
                if (sprite != null)
                {
                    thumbnail.sprite = sprite;
                    thumbnail.color = Color.white;
                    thumbnail.preserveAspect = true;
                }
                else
                {
                    thumbnail.sprite = null;
                    thumbnail.color = new Color(1, 1, 1, 0.3f);
                }
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected;

        targetScale = selected ? baseScale * selectedScale : baseScale;
    }
}