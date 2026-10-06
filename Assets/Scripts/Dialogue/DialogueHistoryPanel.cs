using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// 历史记录面板：维护已读文本的列表，负责开关面板。
/// </summary>
public class DialogueHistoryPanel : MonoBehaviour
{
    [Header("引用")]
    public GameObject historyPanel;
    public TextMeshProUGUI historyText;

    private readonly List<string> log = new List<string>();

    public bool IsOpen => historyPanel != null && historyPanel.activeSelf;

    /// <summary>
    /// 确保 index 位置有一条记录。
    /// - 若 log 里 index 之后还有更旧的记录，裁掉（玩家从中间回去改历史时用）
    /// - 若 index 位置还没有，就加一条
    /// </summary>
    public void EnsureEntryAt(int index, string speaker, string text)
    {
        if (log.Count > index + 1)
            log.RemoveRange(index + 1, log.Count - index - 1);

        if (log.Count <= index)
        {
            string entry = string.IsNullOrEmpty(speaker) ? text : $"【{speaker}】{text}";
            log.Add(entry);
        }
    }

    public void Clear() => log.Clear();

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (historyPanel == null) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("history_open");

        if (PanelManager.Instance != null) PanelManager.Instance.OpenOnly(historyPanel);
        else historyPanel.SetActive(true);

        if (historyText != null) historyText.text = string.Join("\n", log);
    }

    public void Close()
    {
        if (historyPanel == null) return;
        if (PanelManager.Instance != null) PanelManager.Instance.Close(historyPanel);
        else historyPanel.SetActive(false);
    }

    /// <summary>静默隐藏（不走 PanelManager，用于 Start 时初始化）。</summary>
    public void ForceHide()
    {
        if (historyPanel != null) historyPanel.SetActive(false);
    }
}