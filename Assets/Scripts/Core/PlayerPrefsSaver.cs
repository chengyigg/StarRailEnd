using UnityEngine;

/// <summary>
/// 全局 PlayerPrefs 统一落盘。
/// 各处改动设置 / 数值时只 SetInt / SetFloat（改内存很快），
/// 由这个组件在退出 / 切后台时统一 Save()，避免频繁写磁盘。
/// 游戏启动时自动创建，无需场景挂载。
/// </summary>
public class PlayerPrefsSaver : MonoBehaviour
{
    private static PlayerPrefsSaver _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (_instance != null) return;
        GameObject go = new GameObject("[PlayerPrefsSaver]");
        _instance = go.AddComponent<PlayerPrefsSaver>();
        DontDestroyOnLoad(go);
    }

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit() { PlayerPrefs.Save(); }
    void OnApplicationPause(bool pause) { if (pause) PlayerPrefs.Save(); }
}