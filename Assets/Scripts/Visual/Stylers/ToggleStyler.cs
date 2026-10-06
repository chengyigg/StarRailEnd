using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Toggle))]
public class ToggleStyler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("背景框")]
    public Image backgroundImage;
    public Color backgroundNormal = new Color(0.3f, 0.3f, 0.35f, 0.55f);
    public Color backgroundHover = new Color(0.4f, 0.4f, 0.46f, 0.7f);
    public Color backgroundOn = new Color(0.85f, 0.72f, 0.45f, 0.95f);

    [Header("勾选标记")]
    public Image checkmarkImage;
    public Color checkmarkColor = new Color(0.25f, 0.2f, 0.15f);
    public float checkPopScale = 1.6f;
    public float checkStartScale = 0f;

    [Header("文字")]
    public TextMeshProUGUI label;
    public Color labelNormal = new Color(0.85f, 0.85f, 0.85f);
    public Color labelHover = new Color(1f, 0.98f, 0.9f);
    public Color labelOn = new Color(1f, 0.95f, 0.75f);

    [Header("动画")]
    public float smoothSpeed = 12f;
    public float checkPopDuration = 0.25f;

    [Header("音效")]
    public string soundOn = "toggle_on";
    public string soundOff = "toggle_off";

    private Toggle toggle;
    private bool isHover = false;
    private float checkAlpha;
    private Vector3 checkBaseScale;
    private float checkPopTimer = 0f;
    private bool isPopping = false;
    private RectTransform checkRect;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        if (backgroundImage == null || checkmarkImage == null) AutoFindRefs();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();

        if (checkmarkImage != null)
        {
            checkRect = checkmarkImage.rectTransform;
            checkBaseScale = checkRect.localScale;
        }

        toggle.onValueChanged.AddListener(OnToggleChanged);
        checkAlpha = toggle.isOn ? 1f : 0f;
        ApplyInstant();
    }

    void AutoFindRefs()
    {
        if (backgroundImage == null)
        {
            Transform t = transform.Find("Background");
            if (t != null) backgroundImage = t.GetComponent<Image>();
        }
        if (checkmarkImage == null)
        {
            Transform t = transform.Find("Background/Checkmark");
            if (t != null) checkmarkImage = t.GetComponent<Image>();
        }
    }

    void OnToggleChanged(bool on)
    {
        checkAlpha = on ? 1f : 0f;
        if (on && checkmarkImage != null)
        {
            isPopping = true;
            checkPopTimer = 0f;
            if (checkRect != null) checkRect.localScale = checkBaseScale * checkStartScale;
            checkRect.localRotation = Quaternion.Euler(0, 0, -180f);
        }
        else if (!on && checkRect != null)
        {
            checkRect.localScale = checkBaseScale;
            checkRect.localRotation = Quaternion.identity;
            isPopping = false;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(on ? soundOn : soundOff);
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);

        if (backgroundImage != null)
        {
            Color target;
            if (toggle.isOn) target = backgroundOn;
            else target = isHover ? backgroundHover : backgroundNormal;
            backgroundImage.color = Color.Lerp(backgroundImage.color, target, t);
        }

        if (checkmarkImage != null)
        {
            Color c = checkmarkImage.color;
            Color targetCheck = checkmarkColor;
            targetCheck.a = checkAlpha;
            checkmarkImage.color = Color.Lerp(c, targetCheck, t);
        }

        if (isPopping && checkRect != null)
        {
            checkPopTimer += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(checkPopTimer / checkPopDuration);

            float scale;
            if (p < 0.6f) scale = Mathf.Lerp(0f, checkPopScale, p / 0.6f);
            else scale = Mathf.Lerp(checkPopScale, 1f, (p - 0.6f) / 0.4f);
            checkRect.localScale = checkBaseScale * scale;

            float angle = Mathf.Lerp(-180f, 0f, p);
            checkRect.localRotation = Quaternion.Euler(0, 0, angle);

            if (p >= 1f)
            {
                isPopping = false;
                checkRect.localScale = checkBaseScale;
                checkRect.localRotation = Quaternion.identity;
            }
        }

        if (label != null)
        {
            Color targetLabel;
            if (toggle.isOn) targetLabel = labelOn;
            else targetLabel = isHover ? labelHover : labelNormal;
            label.color = Color.Lerp(label.color, targetLabel, t);
        }
    }

    void ApplyInstant()
    {
        if (backgroundImage != null)
            backgroundImage.color = toggle.isOn ? backgroundOn : backgroundNormal;
        if (checkmarkImage != null)
        {
            Color c = checkmarkColor;
            c.a = checkAlpha;
            checkmarkImage.color = c;
        }
        if (label != null)
            label.color = toggle.isOn ? labelOn : labelNormal;
    }

    public void OnPointerEnter(PointerEventData e) { isHover = true; }
    public void OnPointerExit(PointerEventData e) { isHover = false; }
}