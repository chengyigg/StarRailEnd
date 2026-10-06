using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 打字机效果：把一段文字逐字显示到 TMP 文本上。
/// 只负责"显示文字"，不关心对话流程、选项、等待提示这些事。
/// </summary>
public class TypewriterEffect : MonoBehaviour
{
    [Header("目标文本")]
    public TextMeshProUGUI targetText;

    [Header("标点额外停顿")]
    public float punctuationPause = 0.2f;
    public float commaPause = 0.1f;
    public string punctuationChars = "。！？…";
    public string commaChars = "，、；：";

    /// <summary>当前是否正在打字。DialogueManager 用它判断"玩家点击是跳过打字还是下一句"。</summary>
    public bool IsTyping { get; private set; }

    /// <summary>
    /// 正常播放完打字（或被 Complete 提前结束时）触发一次。
    /// 注意：被 Stop() 中断不会触发。
    /// </summary>
    public event System.Action OnFinished;

    private Coroutine routine;
    private string currentFullText = "";

    /// <summary>
    /// 开始打字。
    /// prefix/suffix 是包在 body 前后的内容（比如姓名标签的富文本标签）。
    /// useTypewriter 为 false 时，直接一次性显示完整文本。
    /// </summary>
    public void Play(string prefix, string body, string suffix, bool useTypewriter)
    {
        Stop();

        currentFullText = prefix + body + suffix;

        if (targetText == null) return;

        if (!useTypewriter)
        {
            targetText.text = currentFullText;
            IsTyping = false;
            OnFinished?.Invoke();
            return;
        }

        routine = StartCoroutine(TypeRoutine(prefix, body, suffix));
    }

    /// <summary>立刻把整段文字显示完（玩家点击/按键跳过打字时用）。</summary>
    public void Complete()
    {
        if (!IsTyping) return;

        Stop();
        if (targetText != null) targetText.text = currentFullText;
        IsTyping = false;
        OnFinished?.Invoke();
    }

    /// <summary>中断打字，不触发 OnFinished。切行、跳转时用。</summary>
    public void Stop()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
        IsTyping = false;
    }

    /// <summary>清空文本并中断。用于流程重置。</summary>
    public void Clear()
    {
        Stop();
        currentFullText = "";
        if (targetText != null) targetText.text = "";
    }

    IEnumerator TypeRoutine(string prefix, string body, string suffix)
    {
        IsTyping = true;
        targetText.text = prefix + suffix;

        // 用 StringBuilder 避免每次 += 都分配新字符串
        StringBuilder sb = new StringBuilder(body.Length);

        foreach (char c in body)
        {
            sb.Append(c);
            targetText.text = prefix + sb.ToString() + suffix;

            float waitTime = GameSettings.typingSpeed;
            if (punctuationChars.IndexOf(c) >= 0) waitTime += punctuationPause;
            else if (commaChars.IndexOf(c) >= 0) waitTime += commaPause;

            yield return new WaitForSeconds(waitTime);
        }

        IsTyping = false;
        routine = null;
        OnFinished?.Invoke();
    }
}