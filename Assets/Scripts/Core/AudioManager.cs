using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("音效增益（1=原始，5=增强）")]
    [Range(1f, 20f)] public float sfxGain = 5.0f;

    [Header("SFX 声源池大小")]
    public int sfxPoolSize = 8;

    [Header("BGM 淡入淡出时长")]
    public float bgmFadeDuration = 1.0f;

    private AudioSource bgmSource;
    private List<AudioSource> sfxSources = new List<AudioSource>();
    private int sfxIndex = 0;
    private Dictionary<string, AudioClip> sfxCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmCache = new Dictionary<string, AudioClip>();
    private string currentBgmName = "";
    private Coroutine bgmFadeCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource s = gameObject.AddComponent<AudioSource>();
            s.playOnAwake = false;
            sfxSources.Add(s);
        }

        GameSettings.OnBgmVolumeChanged += ApplyBgmVolume;

        PreloadAll();
    }

    void OnDestroy()
    {
        GameSettings.OnBgmVolumeChanged -= ApplyBgmVolume;
    }

    /// <summary>BGM 音量设置变化时被 GameSettings 事件回调。</summary>
    void ApplyBgmVolume()
    {
        if (bgmSource == null) return;
        if (bgmFadeCoroutine != null) return;   // fade 中让协程自己处理
        if (!bgmSource.isPlaying) return;

        bgmSource.volume = GameSettings.bgmVolume;
    }

    void PreloadAll()
    {
        AudioClip[] sfxs = Resources.LoadAll<AudioClip>("Audio/SFX");
        foreach (var c in sfxs) RegisterClip(sfxCache, c.name, c);

        AudioClip[] bgms = Resources.LoadAll<AudioClip>("Audio/BGM");
        foreach (var c in bgms) RegisterClip(bgmCache, c.name, c);

        GameLog.Info($"[AudioManager] 已加载 {sfxCache.Count} 个音效，{bgmCache.Count} 首 BGM");
    }

    /// <summary>
    /// 注册 clip 到缓存。
    /// 原名 "bgm_emotional（情感对话）" 也会注册短名 "bgm_emotional"。
    /// </summary>
    void RegisterClip(Dictionary<string, AudioClip> cache, string originalName, AudioClip clip)
    {
        cache[originalName] = clip;

        int idx = originalName.IndexOf('（');
        if (idx < 0) idx = originalName.IndexOf('(');
        if (idx > 0)
        {
            string shortName = originalName.Substring(0, idx).Trim();
            if (!cache.ContainsKey(shortName))
                cache[shortName] = clip;
        }
    }

    /// <summary>播放一次性音效。</summary>
    public void PlaySFX(string name)
    {
        PlaySFXInternal(name, 1f);
    }

    /// <summary>播放一次性音效，带随机音调（避免连续播放同一音效听起来机械）。</summary>
    public void PlaySFXRandomPitch(string name, float minPitch = 0.95f, float maxPitch = 1.05f)
    {
        PlaySFXInternal(name, Random.Range(minPitch, maxPitch));
    }

    void PlaySFXInternal(string name, float pitch)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (sfxSources.Count == 0) return;

        if (!sfxCache.TryGetValue(name, out var clip))
        {
            GameLog.Warning($"[AudioManager] 找不到音效: {name}");
            return;
        }

        AudioSource s = sfxSources[sfxIndex];
        sfxIndex = (sfxIndex + 1) % sfxSources.Count;

        s.pitch = pitch;
        s.PlayOneShot(clip, GameSettings.sfxVolume * sfxGain);
    }

    /// <summary>播放 BGM（带淡入淡出）。若已在播放同名 BGM 则什么都不做。</summary>
    public void PlayBGM(string name)
    {
        if (string.IsNullOrEmpty(name)) return;
        if (currentBgmName == name && bgmSource.isPlaying) return;

        if (!bgmCache.TryGetValue(name, out var clip))
        {
            GameLog.Warning($"[AudioManager] 找不到 BGM: {name}");
            return;
        }

        currentBgmName = name;
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeBgm(clip));
    }

    /// <summary>停止 BGM（带淡出）。</summary>
    public void StopBGM()
    {
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeOutBgm());
    }

    IEnumerator FadeBgm(AudioClip newClip)
    {
        // 先淡出旧 BGM
        if (bgmSource.isPlaying)
        {
            float startVol = bgmSource.volume;
            float t = 0f;
            float half = bgmFadeDuration * 0.5f;
            while (t < half)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(startVol, 0f, t / half);
                yield return null;
            }
            bgmSource.Stop();
        }

        // 换 clip 再淡入
        bgmSource.clip = newClip;
        bgmSource.volume = 0f;
        bgmSource.Play();

        float t2 = 0f;
        float targetVol = GameSettings.bgmVolume;
        float half2 = bgmFadeDuration * 0.5f;
        while (t2 < half2)
        {
            t2 += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(0f, targetVol, t2 / half2);
            yield return null;
        }
        bgmSource.volume = targetVol;

        bgmFadeCoroutine = null;
    }

    IEnumerator FadeOutBgm()
    {
        float startVol = bgmSource.volume;
        float t = 0f;
        while (t < bgmFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / bgmFadeDuration);
            yield return null;
        }
        bgmSource.Stop();
        currentBgmName = "";

        bgmFadeCoroutine = null;
    }
}