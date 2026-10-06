using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FallingParticles : MonoBehaviour
{
    [Header("生成")]
    public float spawnInterval = 0.4f;
    public int maxParticles = 25;

    [Header("大小")]
    public float minSize = 8f;
    public float maxSize = 22f;

    [Header("下落速度")]
    public float minSpeed = 20f;
    public float maxSpeed = 60f;

    [Header("左右飘动")]
    public float swayAmount = 25f;
    public float swaySpeed = 1.5f;

    [Header("颜色")]
    public Color particleColor = new Color(1f, 0.95f, 0.85f, 0.5f);

    [Header("精灵（留空则用代码生成的圆点）")]
    public Sprite particleSprite;

    private RectTransform rect;
    private float spawnTimer;
    private List<Particle> particles = new List<Particle>();
    private Sprite dotSprite;

    private class Particle
    {
        public RectTransform rt;
        public Image img;
        public float speed;
        public float swayPhase;
        public float swayFreq;
        public float startX;
    }

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        // 强制全屏锚点
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        if (particleSprite == null) dotSprite = GenerateDot(32);
    }

    void Update()
    {
        // 生成
        spawnTimer += Time.unscaledDeltaTime;
        if (spawnTimer >= spawnInterval && particles.Count < maxParticles)
        {
            spawnTimer = 0f;
            SpawnParticle();
        }

        // 移动
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            if (p.rt == null) { particles.RemoveAt(i); continue; }

            Vector2 pos = p.rt.anchoredPosition;
            pos.y -= p.speed * Time.unscaledDeltaTime;
            pos.x = p.startX + Mathf.Sin(Time.unscaledTime * p.swayFreq + p.swayPhase) * swayAmount;
            p.rt.anchoredPosition = pos;

            // 出界回收
            if (pos.y < rect.rect.yMin - 50f)
            {
                Destroy(p.rt.gameObject);
                particles.RemoveAt(i);
            }
        }
    }

    void SpawnParticle()
    {
        GameObject obj = new GameObject("Particle", typeof(RectTransform));
        obj.transform.SetParent(transform, false);

        Image img = obj.AddComponent<Image>();
        img.sprite = particleSprite != null ? particleSprite : dotSprite;
        img.color = particleColor;
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        float size = Random.Range(minSize, maxSize);
        rt.sizeDelta = new Vector2(size, size);

        float x = Random.Range(rect.rect.xMin, rect.rect.xMax);
        float y = rect.rect.yMax + 50f;
        rt.anchoredPosition = new Vector2(x, y);

        particles.Add(new Particle
        {
            rt = rt,
            img = img,
            speed = Random.Range(minSpeed, maxSpeed),
            swayPhase = Random.Range(0f, Mathf.PI * 2f),
            swayFreq = Random.Range(swaySpeed * 0.7f, swaySpeed * 1.3f),
            startX = x,
        });
    }

    Sprite GenerateDot(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[size * size];
        float half = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - half + 0.5f;
                float dy = y - half + 0.5f;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01((half - d) / (half * 0.5f));
                px[y * size + x] = new Color(1, 1, 1, a);
            }
        }
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}