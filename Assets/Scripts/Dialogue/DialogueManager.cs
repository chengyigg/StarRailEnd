using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("当前对话序列")]
    public DialogueSequence dialogueData;

    [Header("所有对话序列")]
    public List<DialogueSequence> allSequences = new List<DialogueSequence>();

    [Header("UI 引用")]
    public TextMeshProUGUI speakerText;
    public TextMeshProUGUI dialogueText;
    public Image avatarImage;

    [Header("子组件（留空会自动找）")]
    [SerializeField] private TypewriterEffect typewriter;
    [SerializeField] private DialogueChoiceView choiceView;
    [SerializeField] private DialogueHistoryPanel historyView;
    [SerializeField] private DialogueBackgroundView backgroundView;
    [SerializeField] private DialoguePortraitView portraitView;
    [SerializeField] private DialogueKeyboard keyboard;

    [Header("等待指示器")]
    public GameObject waitIndicator;
    public float waitIndicatorBobSpeed = 2f;
    public float waitIndicatorBobRange = 5f;

    [Header("对话框面板")]
    public GameObject dialoguePanel;

    [Header("点击感应器（选项出现时禁用）")]
    public GameObject clickSensor;

    [Header("音效")]
    public bool useTypingSound = true;

    private int currentLineIndex = 0;
    private bool isAutoPlaying = false;
    private bool isFastForwarding = false;
    private float autoTimer = 0f;
    private Vector2 waitIndicatorBasePos;
    private bool sequenceBonusApplied = false;

    // 结局锁
    private bool isEndingPlaying = false;

    // === 给 DialogueKeyboard 用的公开接口 ===
    public bool IsTyping => typewriter != null && typewriter.IsTyping;
    public bool IsHistoryOpen => historyView != null && historyView.IsOpen;
    public void SkipTyping() { if (typewriter != null) typewriter.Complete(); }
    public bool IsCurrentLineRead() { return ReadTracker.IsRead(GetLineKey()); }

    void Awake()
    {
        // 打字机
        if (typewriter == null) typewriter = GetComponent<TypewriterEffect>();
        if (typewriter == null) typewriter = gameObject.AddComponent<TypewriterEffect>();
        typewriter.targetText = dialogueText;
        typewriter.OnFinished -= OnTypewriterFinished;
        typewriter.OnFinished += OnTypewriterFinished;

        // 选项视图
        if (choiceView == null) choiceView = GetComponent<DialogueChoiceView>();
        if (choiceView == null) choiceView = gameObject.AddComponent<DialogueChoiceView>();
        choiceView.Initialize(ReplacePlaceholders);
        choiceView.OnChoiceClicked -= HandleChoiceClicked;
        choiceView.OnChoiceClicked += HandleChoiceClicked;

        // 历史记录面板
        if (historyView == null) historyView = GetComponent<DialogueHistoryPanel>();
        if (historyView == null) historyView = gameObject.AddComponent<DialogueHistoryPanel>();

        // 背景视图
        if (backgroundView == null) backgroundView = GetComponent<DialogueBackgroundView>();
        if (backgroundView == null)
            GameLog.Error("[DialogueManager] 缺少 DialogueBackgroundView 组件，请手动添加并填引用");

        // 立绘视图
        if (portraitView == null) portraitView = GetComponent<DialoguePortraitView>();
        if (portraitView == null)
            GameLog.Error("[DialogueManager] 缺少 DialoguePortraitView 组件，请手动添加并填引用");

        // 键盘（无引用，可自动加）
        if (keyboard == null) keyboard = GetComponent<DialogueKeyboard>();
        if (keyboard == null) keyboard = gameObject.AddComponent<DialogueKeyboard>();
    }

    void OnDestroy()
    {
        if (typewriter != null)
            typewriter.OnFinished -= OnTypewriterFinished;
        if (choiceView != null)
            choiceView.OnChoiceClicked -= HandleChoiceClicked;
    }

    void OnTypewriterFinished()
    {
        if (waitIndicator != null && !HasChoices())
            waitIndicator.SetActive(true);
    }

    void HandleChoiceClicked(DialogueChoice choice)
    {
        if (choice == null) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("choice_click");

        if (!string.IsNullOrEmpty(choice.affectionCharacter))
            AffectionManager.Add(choice.affectionCharacter, choice.affectionDelta);

        JumpToSequence(choice.nextSequenceID);
    }

    void Start()
    {
        if (PlayerPrefs.HasKey("PendingLoadIndex"))
        {
            currentLineIndex = PlayerPrefs.GetInt("PendingLoadIndex");
            string pendingSeqID = PlayerPrefs.GetString("PendingLoadSeqID", "");
            PlayerPrefs.DeleteKey("PendingLoadIndex");
            PlayerPrefs.DeleteKey("PendingLoadSeqID");
            if (!string.IsNullOrEmpty(pendingSeqID))
            {
                DialogueSequence target = allSequences.Find(s => s.sequenceID == pendingSeqID);
                if (target != null) dialogueData = target;
            }
        }

        if (waitIndicator != null)
        {
            waitIndicatorBasePos = waitIndicator.GetComponent<RectTransform>().anchoredPosition;
            waitIndicator.SetActive(false);
        }

        sequenceBonusApplied = false;
        isEndingPlaying = false;

        NameInputPanel nameInput = FindObjectOfType<NameInputPanel>();
        bool needsNameInput = nameInput != null && nameInput.panel != null && nameInput.panel.activeSelf;

        if (needsNameInput)
        {
            GameLog.Info("[DialogueManager] 等待玩家输入名字...");
            if (historyView != null) historyView.ForceHide();
            return;
        }

        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM("bgm_daily");

        OnSequenceEnter(dialogueData);
        ShowCurrentLine();
        if (historyView != null) historyView.ForceHide();
    }

    public void StartDialogueAfterNameInput()
    {
        isEndingPlaying = false;
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBGM("bgm_daily");
        OnSequenceEnter(dialogueData);
        ShowCurrentLine();
    }

    void OnSequenceEnter(DialogueSequence seq)
    {
        if (seq == null) return;

        if (!string.IsNullOrEmpty(seq.bgmName) && AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM(seq.bgmName);

        // 统计本序列里有几个角色带立绘
        HashSet<string> portraitChars = new HashSet<string>();
        foreach (var line in seq.lines)
        {
            if (line.speakerPortrait != null && line.portraitPos != PortraitPosition.None)
            {
                if (!string.IsNullOrEmpty(line.speakerName))
                    portraitChars.Add(line.speakerName);
            }
        }
        if (portraitView != null)
            portraitView.SetSingleMode(portraitChars.Count == 1);

        if (!string.IsNullOrEmpty(seq.chapterTitle) && ChapterTitleUI.Instance != null)
            ChapterTitleUI.Instance.Play(seq.chapterTitle);

        if (!string.IsNullOrEmpty(seq.unlockCharacter))
            AffectionManager.Unlock(seq.unlockCharacter);
    }

    void Update()
    {
        if (isAutoPlaying && !HasChoices() && !IsTyping)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= GameSettings.autoInterval)
            {
                autoTimer = 0f;
                NextLine();
            }
        }

        if (waitIndicator != null && waitIndicator.activeSelf)
        {
            RectTransform rt = waitIndicator.GetComponent<RectTransform>();
            float offset = Mathf.Sin(Time.unscaledTime * waitIndicatorBobSpeed) * waitIndicatorBobRange;
            rt.anchoredPosition = waitIndicatorBasePos + Vector2.up * offset;
        }
    }

    string GetLineKey()
    {
        string seqID = dialogueData != null ? dialogueData.sequenceID : "unknown";
        return $"{seqID}_{currentLineIndex}";
    }

    string ReplacePlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        string playerName = PlayerPrefs.GetString("PlayerName", "勇者");
        return text.Replace("{playerName}", playerName);
    }

    public string GetCurrentChapterName()
    {
        return dialogueData != null ? dialogueData.chapterTitle : "";
    }

    public bool HasChoices()
    {
        if (dialogueData == null || currentLineIndex >= dialogueData.lines.Count) return false;
        var line = dialogueData.lines[currentLineIndex];
        if (line.choices == null) return false;
        foreach (var c in line.choices)
            if (c.CheckCondition()) return true;
        return false;
    }

    void ShowCurrentLine()
    {
        if (clickSensor != null) clickSensor.SetActive(true);

        choiceView.Hide();

        if (dialogueData != null && currentLineIndex < dialogueData.lines.Count)
        {
            DialogueLine line = dialogueData.lines[currentLineIndex];

            string speaker = ReplacePlaceholders(line.speakerName);
            speakerText.text = string.IsNullOrEmpty(speaker) ? "" : $"【{speaker}】";

            historyView.EnsureEntryAt(currentLineIndex, speaker, ReplacePlaceholders(line.textContent));

            ReadTracker.MarkRead(GetLineKey());

            if (backgroundView != null)
                backgroundView.ShowBackground(line.background);

            if (line.speakerAvatar != null)
            {
                avatarImage.sprite = line.speakerAvatar;
                avatarImage.gameObject.SetActive(true);
            }
            else avatarImage.gameObject.SetActive(false);

            if (portraitView != null)
                portraitView.Show(line.speakerPortrait, line.portraitPos);

            ApplyTextStyleForSpeaker(line.speakerName);

            if (backgroundView != null)
                backgroundView.SetMonologueMode(line.speakerName == "（内心）");

            string body = ReplacePlaceholders(line.textContent);
            bool isDialogue = !string.IsNullOrEmpty(speaker) && speaker != "（内心）";
            string prefix = isDialogue ? "<b>「</b>" : "";
            string suffix = isDialogue ? "<b>」</b>" : "";

            if (waitIndicator != null) waitIndicator.SetActive(false);

            typewriter.Play(prefix, body, suffix, GameSettings.useTypewriter);

            if (line.choices != null && line.choices.Count > 0)
            {
                isAutoPlaying = false;
                isFastForwarding = false;

                if (clickSensor != null) clickSensor.SetActive(false);

                choiceView.Show(line.choices);

                if (choiceView.HasVisibleChoices && waitIndicator != null)
                    waitIndicator.SetActive(false);
            }
        }
        else
        {
         

            if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.unlockCharacter))
                AffectionManager.Unlock(dialogueData.unlockCharacter);

            bool zeroTriggered = false;
            if (dialogueData != null && !sequenceBonusApplied
                && !string.IsNullOrEmpty(dialogueData.affectionCharacter)
                && dialogueData.affectionDelta != 0)
            {
                sequenceBonusApplied = true;
                string ch = dialogueData.affectionCharacter;
                int before = AffectionManager.Get(ch);
                AffectionManager.Add(ch, dialogueData.affectionDelta);
                int after = AffectionManager.Get(ch);

                if (after <= 0 && before > 0)
                {
                    var cd = AffectionManager.GetCharacterData(ch);
                    if (cd != null && cd.zeroDialogue != null && !AffectionManager.IsZeroTriggered(ch))
                    {
                        AffectionManager.MarkZeroTriggered(ch);
                        zeroTriggered = true;
                        dialogueData = cd.zeroDialogue;
                        currentLineIndex = 0;
                        historyView.Clear();
                        sequenceBonusApplied = false;
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlaySFX("zero_trigger");
                        ShowCurrentLine();
                    }
                }
            }
            if (zeroTriggered) return;

            if (dialogueData != null && !string.IsNullOrEmpty(dialogueData.setVariableName))
            {
                GameVariables.SetInt(dialogueData.setVariableName, dialogueData.setVariableValue);
            }

            string jumpTarget = null;
            if (dialogueData != null && dialogueData.conditionalNexts != null && dialogueData.conditionalNexts.Count > 0)
            {
                foreach (var cj in dialogueData.conditionalNexts)
                {
                    if (cj.Check())
                    {
                        jumpTarget = cj.targetSequenceID;
                        break;
                    }
                }
            }
            if (string.IsNullOrEmpty(jumpTarget))
                jumpTarget = dialogueData != null ? dialogueData.nextSequenceID : null;

            speakerText.text = "";
            typewriter.Stop();
            typewriter.Clear();
            isAutoPlaying = false;
            isFastForwarding = false;
            if (waitIndicator != null) waitIndicator.SetActive(false);

            if (!string.IsNullOrEmpty(jumpTarget))
            {
                JumpToSequence(jumpTarget);
            }
            else
            {
                if (isEndingPlaying)
                {
                    GameLog.Info("[结局] 已在播放中，忽略重复触发");
                    return;
                }
                string endingName = dialogueData != null ? dialogueData.sequenceID : "结局";
                isEndingPlaying = true;
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayBGM("bgm_ending");
                if (GameEndingUI.Instance != null)
                    GameEndingUI.Instance.PlayEnding(endingName);
                else
                    GameLog.Warning("[结局] GameEndingUI.Instance 为空");
            }
        }
    }

    void JumpToSequence(string sequenceID)
    {
        if (string.IsNullOrEmpty(sequenceID)) return;
        DialogueSequence target = allSequences.Find(s => s.sequenceID == sequenceID);
        if (target == null) { GameLog.Warning($"找不到 sequenceID: {sequenceID}"); return; }

        isEndingPlaying = false;

        dialogueData = target;
        currentLineIndex = 0;
        historyView.Clear();
        sequenceBonusApplied = false;

        OnSequenceEnter(target);
        ShowCurrentLine();

        SaveData auto = new SaveData
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            sequenceID = dialogueData.sequenceID,
            lineIndex = currentLineIndex,
            chapterName = ""
        };
        SaveManager.SaveAuto(auto);
    }

    public void TriggerSpecialSequence(DialogueSequence seq)
    {
        if (seq == null) return;
        dialogueData = seq;
        currentLineIndex = 0;
        historyView.Clear();
        sequenceBonusApplied = false;

        if (dialoguePanel != null && !dialoguePanel.activeSelf)
            dialoguePanel.SetActive(true);

        ShowCurrentLine();
    }

    public void NextLine()
    {
        if (IsTyping) { SkipTyping(); return; }

        do { currentLineIndex++; }
        while (isFastForwarding
               && currentLineIndex < dialogueData.lines.Count - 1
               && (dialogueData.lines[currentLineIndex].choices == null
                   || dialogueData.lines[currentLineIndex].choices.Count == 0));

        ShowCurrentLine();
    }

    public void RefreshCurrentLine() { ShowCurrentLine(); }

    public void ToggleAuto()
    {
        isAutoPlaying = !isAutoPlaying;
        autoTimer = 0f;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(isAutoPlaying ? "auto_on" : "auto_off");
    }

    public void GoBack() { if (currentLineIndex > 0) { currentLineIndex--; ShowCurrentLine(); } }

    public void ToggleFastForward()
    {
        isFastForwarding = !isFastForwarding;
        if (AudioManager.Instance != null && isFastForwarding)
            AudioManager.Instance.PlaySFX("fast_forward");
    }

    void ApplyTextStyleForSpeaker(string speaker)
    {
        if (dialogueText == null) return;

        dialogueText.color = new Color(0.92f, 0.92f, 0.9f);
        dialogueText.fontStyle = FontStyles.Normal;
        dialogueText.fontSize = 28f;

        if (string.IsNullOrEmpty(speaker))
        {
            dialogueText.color = new Color(0.75f, 0.75f, 0.72f);
            dialogueText.fontStyle = FontStyles.Italic;
            dialogueText.fontSize = 26f;
        }
        else if (speaker == "（内心）")
        {
            dialogueText.color = new Color(0.85f, 0.8f, 0.95f);
            dialogueText.fontStyle = FontStyles.Italic;
            dialogueText.fontSize = 26f;
        }
    }

    public void ToggleHistory() { historyView.Toggle(); }
    public void CloseHistory() { historyView.Close(); }

    public void OnSensorClicked()
    {
        if (isEndingPlaying) return;

        if (ChapterTitleUI.Instance != null && ChapterTitleUI.Instance.IsPlaying)
            return;

        if (dialoguePanel != null)
        {
            if (!dialoguePanel.activeSelf) { dialoguePanel.SetActive(true); ShowCurrentLine(); }
            else
            {
                if (HasChoices()) return;
                NextLine();
            }
        }
    }

    public int GetCurrentLineIndex() { return currentLineIndex; }
    public string GetCurrentSequenceID() { return dialogueData != null ? dialogueData.sequenceID : ""; }

    public void LoadFromSave(string sequenceID, int lineIndex)
    {
        if (!string.IsNullOrEmpty(sequenceID))
        {
            DialogueSequence target = allSequences.Find(s => s.sequenceID == sequenceID);
            if (target != null) dialogueData = target;
        }
        currentLineIndex = lineIndex;
        historyView.Clear();
        sequenceBonusApplied = false;
        isEndingPlaying = false;
        ShowCurrentLine();
    }

    public void SetCurrentLineIndex(int index) { currentLineIndex = index; ShowCurrentLine(); }
}