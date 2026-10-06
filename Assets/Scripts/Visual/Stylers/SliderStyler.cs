using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Slider))]
public class SliderStyler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("背景条")]
    public Image backgroundImage;
    public Color backgroundNormal = new Color(0.3f, 0.3f, 0.35f, 0.6f);
    public Color backgroundHover = new Color(0.35f, 0.35f, 0.42f, 0.7f);

    [Header("填充条")]
    public Image fillImage;
    public Color fillNormal = new Color(0.85f, 0.75f, 0.55f);
    public Color fillHover = new Color(0.95f, 0.85f, 0.65f);

    [Header("手柄")]
    public Image handleImage;
    public Color handleNormal = new Color(0.95f, 0.92f, 0.85f);
    public Color handleHover = new Color(1f, 0.98f, 0.9f);
    public float handleHoverScale = 1.15f;

    [Header("动画")]
    public float smoothSpeed = 10f;

    [Header("音效")]
    public string tickSoundName = "slider_tick";
    public float tickCooldown = 0.08f;

    private Slider slider;
    private bool isHover = false;
    private Vector3 handleBaseScale;
    private float lastTickTime = -1f;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (backgroundImage == null || fillImage == null || handleImage == null) AutoFindRefs();
        if (handleImage != null) handleBaseScale = handleImage.transform.localScale;

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void AutoFindRefs()
    {
        if (backgroundImage == null)
        {
            Transform t = transform.Find("Background");
            if (t != null) backgroundImage = t.GetComponent<Image>();
        }
        if (fillImage == null)
        {
            Transform t = transform.Find("Fill Area/Fill");
            if (t != null) fillImage = t.GetComponent<Image>();
        }
        if (handleImage == null)
        {
            Transform t = transform.Find("Handle Slide Area/Handle");
            if (t != null) handleImage = t.GetComponent<Image>();
        }
    }

    void OnValueChanged(float v)
    {
        if (Time.unscaledTime - lastTickTime < tickCooldown) return;
        lastTickTime = Time.unscaledTime;
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(tickSoundName))
            AudioManager.Instance.PlaySFXRandomPitch(tickSoundName, 0.9f, 1.1f);
    }

    void Update()
    {
        float t = 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime);

        if (backgroundImage != null)
            backgroundImage.color = Color.Lerp(backgroundImage.color, isHover ? backgroundHover : backgroundNormal, t);
        if (fillImage != null)
            fillImage.color = Color.Lerp(fillImage.color, isHover ? fillHover : fillNormal, t);
        if (handleImage != null)
        {
            handleImage.color = Color.Lerp(handleImage.color, isHover ? handleHover : handleNormal, t);
            Vector3 targetScale = isHover ? handleBaseScale * handleHoverScale : handleBaseScale;
            handleImage.transform.localScale = Vector3.Lerp(handleImage.transform.localScale, targetScale, t);
        }
    }

    public void OnPointerEnter(PointerEventData e) { isHover = true; }
    public void OnPointerExit(PointerEventData e) { isHover = false; }
}