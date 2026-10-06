using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;

    [Header("所有受管理的面板")]
    public GameObject[] panels;

    [Header("底部工具栏（打开面板时自动锁定）")]
    public BottomButtonBar bottomBar;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>只打开 target，其余面板全部关闭。常用于"打开存档面板"这种互斥弹窗。</summary>
    public void OpenOnly(GameObject target)
    {
        if (target == null) return;

        foreach (var p in panels)
        {
            if (p == null) continue;
            p.SetActive(p == target);
        }

        UpdateBottomBarLock();
    }

    /// <summary>关闭指定面板。target 为 null 时关闭全部。</summary>
    public void Close(GameObject target)
    {
        if (target == null)
        {
            CloseAll();
            return;
        }

        target.SetActive(false);
        UpdateBottomBarLock();
    }

    public void CloseAll()
    {
        foreach (var p in panels)
            if (p != null) p.SetActive(false);

        UpdateBottomBarLock();
    }

    /// <summary>当前是否有任何面板处于打开状态。</summary>
    public bool IsAnyPanelOpen()
    {
        if (panels == null) return false;
        foreach (var p in panels)
            if (p != null && p.activeSelf) return true;
        return false;
    }

    void UpdateBottomBarLock()
    {
        if (bottomBar != null)
            bottomBar.SetLocked(IsAnyPanelOpen());
    }
}