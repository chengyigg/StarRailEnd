using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NameInputPanel : MonoBehaviour
{
    public GameObject panel;
    public TMP_InputField inputField;
    public Button confirmButton;

    [Header("对话系统（用于确认名字后刷新当前行）")]
    public DialogueManager dialogueManager;

    void Start()
    {
        if (!PlayerPrefs.HasKey("PlayerName"))
            panel.SetActive(true);
        else
            panel.SetActive(false);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(Confirm);
    }

    void Update()
    {
        if (panel.activeSelf && Input.GetKeyDown(KeyCode.Return))
            Confirm();
    }

    public void Confirm()
    {
        string name = inputField != null ? inputField.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) name = "勇者";
        PlayerPrefs.SetString("PlayerName", name);
        panel.SetActive(false);
        GameLog.Info($"[NameInputPanel] 玩家名字设为: {name}");

        // 兜底：Inspector 里没挂时，全局找一次（少见的兜底，不常走）
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();

        if (dialogueManager != null)
            dialogueManager.RefreshCurrentLine();
    }
}