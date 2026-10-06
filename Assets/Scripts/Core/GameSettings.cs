using UnityEngine;

/// <summary>
/// 游戏设置：音量 / 打字机 / 显示。
/// 内存缓存 + 脏标记，避免每次读写都摸 PlayerPrefs。
/// 定期通过 GameSettingsSaver 或手动调用 Save() 落盘。
/// </summary>
public static class GameSettings
{
    // ================== 分辨率选项（静态，不进 PlayerPrefs）==================
    public static readonly Vector2Int[] resolutions = new Vector2Int[]
    {
        new Vector2Int(1920, 1080),
        new Vector2Int(1600, 900),
        new Vector2Int(1366, 768),
        new Vector2Int(1280, 720)
    };

    // ================== PlayerPrefs 键名（集中管理，避免拼错）==================
    private const string KEY_MASTER = "GS_MasterVolume";
    private const string KEY_BGM = "GS_BGMVolume";
    private const string KEY_SFX = "GS_SFXVolume";
    private const string KEY_TYPING_SPEED = "GS_TypingSpeed";
    private const string KEY_AUTO_INTERVAL = "GS_AutoInterval";
    private const string KEY_USE_TYPEWRITER = "GS_UseTypewriter";
    private const string KEY_USE_TYPING_SOUND = "GS_UseTypingSound";
    private const string KEY_FULLSCREEN = "GS_Fullscreen";
    private const string KEY_RESOLUTION_INDEX = "GS_ResolutionIndex";

    // ================== 默认值 ==================
    private const float DEFAULT_MASTER = 1f;
    private const float DEFAULT_BGM = 0.7f;
    private const float DEFAULT_SFX = 0.8f;
    private const float DEFAULT_TYPING_SPEED = 0.05f;
    private const float DEFAULT_AUTO_INTERVAL = 2f;
    private const bool DEFAULT_USE_TYPEWRITER = true;
    private const bool DEFAULT_USE_TYPING_SOUND = true;
    private const bool DEFAULT_FULLSCREEN = true;
    private const int DEFAULT_RESOLUTION_INDEX = 0;

    // ================== 内存缓存 ==================
    private static bool _loaded = false;
    private static bool _dirty = false;

    private static float _masterVolume;
    private static float _bgmVolume;
    private static float _sfxVolume;
    private static float _typingSpeed;
    private static float _autoInterval;
    private static bool _useTypewriter;
    private static bool _useTypingSound;
    private static bool _isFullscreen;
    private static int _resolutionIndex;

    /// <summary>是否有未落盘的改动。GameSettingsSaver 会周期性检查它。</summary>
    public static bool IsDirty => _dirty;

    /// <summary>BGM 音量变化时触发。AudioManager 订阅它来实时更新播放中的 BGM。</summary>
    public static event System.Action OnBgmVolumeChanged;

    // ================== 对外属性（接口和以前完全一样）==================
    public static float masterVolume
    {
        get { EnsureLoaded(); return _masterVolume; }
        set { EnsureLoaded(); if (_masterVolume == value) return; _masterVolume = value; _dirty = true; }
    }

    public static float bgmVolume
    {
        get { EnsureLoaded(); return _bgmVolume; }
        set
        {
            EnsureLoaded();
            if (_bgmVolume == value) return;
            _bgmVolume = value;
            _dirty = true;
            OnBgmVolumeChanged?.Invoke();
        }
    }

    public static float sfxVolume
    {
        get { EnsureLoaded(); return _sfxVolume; }
        set { EnsureLoaded(); if (_sfxVolume == value) return; _sfxVolume = value; _dirty = true; }
    }

    public static float typingSpeed
    {
        get { EnsureLoaded(); return _typingSpeed; }
        set { EnsureLoaded(); if (_typingSpeed == value) return; _typingSpeed = value; _dirty = true; }
    }

    public static float autoInterval
    {
        get { EnsureLoaded(); return _autoInterval; }
        set { EnsureLoaded(); if (_autoInterval == value) return; _autoInterval = value; _dirty = true; }
    }

    public static bool useTypewriter
    {
        get { EnsureLoaded(); return _useTypewriter; }
        set { EnsureLoaded(); if (_useTypewriter == value) return; _useTypewriter = value; _dirty = true; }
    }

    public static bool useTypingSound
    {
        get { EnsureLoaded(); return _useTypingSound; }
        set { EnsureLoaded(); if (_useTypingSound == value) return; _useTypingSound = value; _dirty = true; }
    }

    public static bool isFullscreen
    {
        get { EnsureLoaded(); return _isFullscreen; }
        set { EnsureLoaded(); if (_isFullscreen == value) return; _isFullscreen = value; _dirty = true; }
    }

    public static int resolutionIndex
    {
        get { EnsureLoaded(); return _resolutionIndex; }
        set { EnsureLoaded(); if (_resolutionIndex == value) return; _resolutionIndex = value; _dirty = true; }
    }

    // ================== 加载 / 保存 ==================
    private static void EnsureLoaded()
    {
        if (_loaded) return;
        _loaded = true;

        _masterVolume = PlayerPrefs.GetFloat(KEY_MASTER, DEFAULT_MASTER);
        _bgmVolume = PlayerPrefs.GetFloat(KEY_BGM, DEFAULT_BGM);
        _sfxVolume = PlayerPrefs.GetFloat(KEY_SFX, DEFAULT_SFX);
        _typingSpeed = PlayerPrefs.GetFloat(KEY_TYPING_SPEED, DEFAULT_TYPING_SPEED);
        _autoInterval = PlayerPrefs.GetFloat(KEY_AUTO_INTERVAL, DEFAULT_AUTO_INTERVAL);
        _useTypewriter = PlayerPrefs.GetInt(KEY_USE_TYPEWRITER, DEFAULT_USE_TYPEWRITER ? 1 : 0) == 1;
        _useTypingSound = PlayerPrefs.GetInt(KEY_USE_TYPING_SOUND, DEFAULT_USE_TYPING_SOUND ? 1 : 0) == 1;
        _isFullscreen = PlayerPrefs.GetInt(KEY_FULLSCREEN, DEFAULT_FULLSCREEN ? 1 : 0) == 1;
        _resolutionIndex = PlayerPrefs.GetInt(KEY_RESOLUTION_INDEX, DEFAULT_RESOLUTION_INDEX);
    }

    /// <summary>把当前内存里的设置写回 PlayerPrefs。没脏就啥也不干。</summary>
    public static void Save()
    {
        if (!_dirty) return;
        _dirty = false;

        PlayerPrefs.SetFloat(KEY_MASTER, _masterVolume);
        PlayerPrefs.SetFloat(KEY_BGM, _bgmVolume);
        PlayerPrefs.SetFloat(KEY_SFX, _sfxVolume);
        PlayerPrefs.SetFloat(KEY_TYPING_SPEED, _typingSpeed);
        PlayerPrefs.SetFloat(KEY_AUTO_INTERVAL, _autoInterval);
        PlayerPrefs.SetInt(KEY_USE_TYPEWRITER, _useTypewriter ? 1 : 0);
        PlayerPrefs.SetInt(KEY_USE_TYPING_SOUND, _useTypingSound ? 1 : 0);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, _isFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KEY_RESOLUTION_INDEX, _resolutionIndex);
        PlayerPrefs.Save();
    }

    // ================== 应用 ==================
    public static void ApplyAll()
    {
        ApplyVolume();
        ApplyDisplay();
    }

    public static void ApplyVolume()
    {
        AudioListener.volume = masterVolume;
    }

    public static void ApplyDisplay()
    {
        Vector2Int res = resolutions[Mathf.Clamp(resolutionIndex, 0, resolutions.Length - 1)];
        Screen.SetResolution(res.x, res.y, isFullscreen);
    }

    // ================== 重置 ==================
    /// <summary>恢复到默认值，并从磁盘删除存档。</summary>
    public static void ResetAll()
    {
        // 内存恢复默认
        _masterVolume = DEFAULT_MASTER;
        _bgmVolume = DEFAULT_BGM;
        _sfxVolume = DEFAULT_SFX;
        _typingSpeed = DEFAULT_TYPING_SPEED;
        _autoInterval = DEFAULT_AUTO_INTERVAL;
        _useTypewriter = DEFAULT_USE_TYPEWRITER;
        _useTypingSound = DEFAULT_USE_TYPING_SOUND;
        _isFullscreen = DEFAULT_FULLSCREEN;
        _resolutionIndex = DEFAULT_RESOLUTION_INDEX;

        // 磁盘恢复默认
        PlayerPrefs.DeleteKey(KEY_MASTER);
        PlayerPrefs.DeleteKey(KEY_BGM);
        PlayerPrefs.DeleteKey(KEY_SFX);
        PlayerPrefs.DeleteKey(KEY_TYPING_SPEED);
        PlayerPrefs.DeleteKey(KEY_AUTO_INTERVAL);
        PlayerPrefs.DeleteKey(KEY_USE_TYPEWRITER);
        PlayerPrefs.DeleteKey(KEY_USE_TYPING_SOUND);
        PlayerPrefs.DeleteKey(KEY_FULLSCREEN);
        PlayerPrefs.DeleteKey(KEY_RESOLUTION_INDEX);
        PlayerPrefs.Save();

        _dirty = false;
    }
}