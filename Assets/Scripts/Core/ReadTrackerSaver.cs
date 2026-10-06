using UnityEngine;

/// <summary>
/// 定期把 ReadTracker 的脏数据落盘。
/// 游戏启动时自动创建，无需场景挂载。
/// </summary>
public class ReadTrackerSaver : MonoBehaviour
{
    [Tooltip("每隔多少秒检查一次脏标记")]
    public float checkInterval = 10f;

    private static ReadTrackerSaver _instance;
    private float _timer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;
        GameObject go = new GameObject("[ReadTrackerSaver]");
        _instance = go.AddComponent<ReadTrackerSaver>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!ReadTracker.IsDirty) return;

        _timer += Time.unscaledDeltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            ReadTracker.Save();
        }
    }

    void OnApplicationQuit() { ReadTracker.Save(); }
    void OnApplicationPause(bool pause) { if (pause) ReadTracker.Save(); }
}