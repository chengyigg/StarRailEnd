using UnityEngine;

/// <summary>
/// 处理对话系统的键盘输入：Ctrl 快进、Escape 关历史。
/// 只管"按键 → 通知 DialogueManager"，不关心剧情逻辑。
/// </summary>
[RequireComponent(typeof(DialogueManager))]
public class DialogueKeyboard : MonoBehaviour
{
    [Header("Ctrl 跳过已读间隔（秒）")]
    public float ctrlSkipDelay = 0.05f;

    private DialogueManager dm;
    private float ctrlSkipTimer = 0f;

    void Awake()
    {
        dm = GetComponent<DialogueManager>();
    }

    void Update()
    {
        if (dm == null) return;

        HandleCtrlSkip();
        HandleEscape();
    }

    void HandleCtrlSkip()
    {
        bool ctrlHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (!ctrlHeld)
        {
            ctrlSkipTimer = 0f;
            return;
        }

        if (dm.IsTyping)
        {
            dm.SkipTyping();
            return;
        }

        if (dm.IsCurrentLineRead() && !dm.HasChoices())
        {
            ctrlSkipTimer += Time.unscaledDeltaTime;
            if (ctrlSkipTimer >= ctrlSkipDelay)
            {
                ctrlSkipTimer = 0f;
                dm.NextLine();
            }
        }
    }

    void HandleEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && dm.IsHistoryOpen)
            dm.CloseHistory();
    }
}