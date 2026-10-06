using UnityEngine;

/// <summary>
/// 自动创建的游戏设置落盘器。
/// 不需要手动挂载，游戏启动时自动创建并跨场景常驻。
/// 每隔一段时间检查 GameSettings 脏标记，有改动就写盘，退出时强制保存。
/// </summary>
public class GameSettingsSaver : MonoBehaviour
{
    [Tooltip("每隔多少秒检查一次脏标记（秒）")]
    public float checkInterval = 0.5f;

    private static GameSettingsSaver _instance;
    private float _timer;

    /// <summary>游戏启动时自动创建，无需任何场景挂载。</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;

        GameObject go = new GameObject("[GameSettingsSaver]");
        _instance = go.AddComponent<GameSettingsSaver>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!GameSettings.IsDirty) return;

        _timer += Time.unscaledDeltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            GameSettings.Save();
        }
    }

    void OnApplicationQuit()
    {
        GameSettings.Save();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) GameSettings.Save();
    }
}