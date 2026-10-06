using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class AffectionSlotUI : MonoBehaviour
{
    public Image avatarImage;
    public TextMeshProUGUI nameText;

    [Header("心形容器")]
    public Transform heartContainer;

    [Header("心形参数")]
    public int heartCount = 10;
    public float heartSize = 40f;

    [Header("心形颜色")]
    public Color outlineColor = new Color(0.35f, 0.08f, 0.12f);
    public Color fillColor = new Color(0.95f, 0.3f, 0.4f);
    public Color highlightColor = new Color(1f, 0.65f, 0.72f);
    public Color emptyOutlineColor = new Color(0.4f, 0.35f, 0.4f, 0.6f);

    [Header("动画")]
    public float popDuration = 0.25f;
    public float popScale = 1.6f;
    public float shakeDuration = 0.3f;
    public float shakeAmount = 6f;
    public float breatheAmplitude = 2f;
    public float breatheSpeed = 2f;

    private Image[] hearts;
    private Sprite emptySprite;
    private Sprite halfSprite;
    private Sprite fullSprite;
    private string characterName;
    private int lastValue = -1;

    public void Setup(CharacterData data)
    {
        if (data == null) return;

        characterName = data.characterName;
        if (avatarImage != null) avatarImage.sprite = data.avatar;
        if (nameText != null) nameText.text = data.characterName;

        BuildSprites();
        BuildHearts();
        Refresh(false);
    }

    void BuildSprites()
    {
        emptySprite = HeartSpriteGenerator.Generate(HeartState.Empty, outlineColor, fillColor, highlightColor, emptyOutlineColor);
        halfSprite = HeartSpriteGenerator.Generate(HeartState.Half, outlineColor, fillColor, highlightColor, emptyOutlineColor);
        fullSprite = HeartSpriteGenerator.Generate(HeartState.Full, outlineColor, fillColor, highlightColor, emptyOutlineColor);
    }

    void BuildHearts()
    {
        if (heartContainer == null)
        {
            GameLog.Error($"[AffectionSlotUI] {name} 的 heartContainer 未挂");
            return;
        }

        // 如果心形已存在且数量正确，只刷新 sprite 复用，不重建 GameObject
        if (hearts != null && hearts.Length == heartCount)
        {
            for (int i = 0; i < heartCount; i++)
            {
                if (hearts[i] == null) continue;
                hearts[i].sprite = emptySprite;
                hearts[i].rectTransform.sizeDelta = new Vector2(heartSize, heartSize);
                hearts[i].rectTransform.localScale = Vector3.one;
                hearts[i].rectTransform.anchoredPosition = new Vector2(hearts[i].rectTransform.anchoredPosition.x, 0f);
            }
            return;
        }

        // 首次创建
        foreach (Transform t in heartContainer)
            Destroy(t.gameObject);

        hearts = new Image[heartCount];
        for (int i = 0; i < heartCount; i++)
        {
            GameObject obj = new GameObject($"Heart_{i}", typeof(RectTransform));
            obj.transform.SetParent(heartContainer, false);

            Image img = obj.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = emptySprite;

            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(heartSize, heartSize);
            rt.localScale = Vector3.one;

            hearts[i] = img;
        }
    }

    void Update()
    {
        if (hearts == null) return;

        float offset = Mathf.Sin(Time.unscaledTime * breatheSpeed) * breatheAmplitude;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            RectTransform rt = hearts[i].rectTransform;
            Vector2 p = rt.anchoredPosition;
            p.y = (hearts[i].sprite == fullSprite) ? offset : 0f;
            rt.anchoredPosition = p;
        }
    }

    public void Refresh(bool animate = true)
    {
        if (hearts == null || string.IsNullOrEmpty(characterName)) return;

        int value = AffectionManager.Get(characterName);
        int perHeart = 100 / heartCount;

        bool valueChanged = (lastValue >= 0 && lastValue != value);
        int previousValue = lastValue;
        lastValue = value;

        for (int i = 0; i < heartCount; i++)
        {
            if (hearts[i] == null) continue;

            int heartValue = value - i * perHeart;
            Sprite newSprite;
            if (heartValue >= perHeart) newSprite = fullSprite;
            else if (heartValue >= perHeart / 2) newSprite = halfSprite;
            else newSprite = emptySprite;

            hearts[i].sprite = newSprite;

            if (animate && valueChanged)
            {
                int prevHeartValue = previousValue - i * perHeart;
                int prevState = prevHeartValue >= perHeart ? 2 : (prevHeartValue >= perHeart / 2 ? 1 : 0);
                int newState = heartValue >= perHeart ? 2 : (heartValue >= perHeart / 2 ? 1 : 0);

                if (newState > prevState) StartCoroutine(PopHeart(hearts[i]));
                else if (newState < prevState) StartCoroutine(ShakeHeart(hearts[i]));
            }
        }
    }

    IEnumerator PopHeart(Image heart)
    {
        RectTransform rt = heart.rectTransform;
        float t = 0f;
        while (t < popDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / popDuration);
            float scale;
            if (p < 0.5f) scale = Mathf.Lerp(1f, popScale, p / 0.5f);
            else scale = Mathf.Lerp(popScale, 1f, (p - 0.5f) / 0.5f);
            rt.localScale = Vector3.one * scale;
            yield return null;
        }
        rt.localScale = Vector3.one;
    }

    IEnumerator ShakeHeart(Image heart)
    {
        RectTransform rt = heart.rectTransform;
        Vector2 basePos = rt.anchoredPosition;
        float t = 0f;
        while (t < shakeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / shakeDuration);
            float x = Mathf.Sin(p * Mathf.PI * 8f) * shakeAmount * (1f - p);
            rt.anchoredPosition = basePos + new Vector2(x, 0f);
            yield return null;
        }
        rt.anchoredPosition = basePos;
    }
}