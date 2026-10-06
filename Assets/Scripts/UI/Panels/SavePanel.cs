using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum SavePanelMode { Save, Load }

public class SavePanel : MonoBehaviour
{
    [Header("标题与关闭")]
    public TextMeshProUGUI titleText;
    public Button closeButton;

    [Header("9 个存档槽")]
    public SlotUI[] slots;

    [Header("底部按钮")]
    public Button saveButton;
    public Button loadButton;
    public Button deleteButton;      // 删除选中
    public Button deleteAllButton;   // 删除所有

    [Header("确认弹窗")]
    public GameObject confirmDialog;
    public TextMeshProUGUI confirmText;
    public Button confirmYesButton;
    public Button confirmNoButton;

    [Header("Toast")]
    public GameObject toastRoot;
    public TextMeshProUGUI toastLabel;

    [Header("大预览（选中槽位后显示）")]
    public Image previewImage;
    public GameObject previewEmptyHint;
    public TextMeshProUGUI previewTitleText;
    public TextMeshProUGUI previewTimeText;

    [Header("对话系统（用于保存当前进度）")]
    public DialogueManager dialogueManager;

    private enum PendingAction { None, Save, DeleteOne, DeleteAll }
    private PendingAction pendingAction = PendingAction.None;

    private SavePanelMode currentMode = SavePanelMode.Load;
    private int selectedSlot = -1;
    private float previousTimeScale = 1f;
    private float toastTimer = 0f;

    void Start()
    {
        if (closeButton) closeButton.onClick.AddListener(Close);
        if (saveButton) saveButton.onClick.AddListener(OnSaveClicked);
        if (loadButton) loadButton.onClick.AddListener(OnLoadClicked);
        if (deleteButton) deleteButton.onClick.AddListener(OnDeleteClicked);
        if (deleteAllButton) deleteAllButton.onClick.AddListener(OnDeleteAllClicked);
        if (confirmYesButton) confirmYesButton.onClick.AddListener(OnConfirmYes);
        if (confirmNoButton) confirmNoButton.onClick.AddListener(OnConfirmNo);
        if (confirmDialog) confirmDialog.SetActive(false);
        if (toastRoot) toastRoot.SetActive(false);

        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                slots[i].slotIndex = i;
                slots[i].Init(this);
            }
        }

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (confirmDialog != null && confirmDialog.activeSelf)
                CloseConfirmDialog();
            else if (gameObject.activeSelf)
                Close();
        }

        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            if (toastTimer <= 0f && toastRoot != null)
                toastRoot.SetActive(false);
        }
    }

    // ================== 开关 ==================
    public void Open(SavePanelMode mode)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("panel_open");

        currentMode = mode;
        selectedSlot = -1;

        if (titleText != null)
            titleText.text = mode == SavePanelMode.Save ? "存档" : "读取";

        if (saveButton != null)
            saveButton.gameObject.SetActive(mode == SavePanelMode.Save);

        if (PanelManager.Instance != null) PanelManager.Instance.OpenOnly(gameObject);
        else gameObject.SetActive(true);

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        RefreshAllSlots();
        RefreshPreview(null);
        UpdateButtonStates();
    }

    public void Close()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("panel_close");

        if (PanelManager.Instance != null) PanelManager.Instance.Close(gameObject);
        else gameObject.SetActive(false);

        Time.timeScale = previousTimeScale;
    }

    // ================== 槽位刷新 ==================
    void RefreshAllSlots()
    {
        if (slots == null) return;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            SaveData data = SaveManager.LoadFromSlot(i);
            slots[i].SetData(data);
            slots[i].SetSelected(false);
        }
    }

    public void OnSlotClicked(int index)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("save_slot_select");

        selectedSlot = index;
        if (slots != null)
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i] != null) slots[i].SetSelected(i == index);
        }
        UpdateButtonStates();

        SaveData data = SaveManager.LoadFromSlot(index);
        RefreshPreview(data);
    }

    void UpdateButtonStates()
    {
        bool hasSelection = selectedSlot >= 0;
        bool slotHasData = hasSelection && SaveManager.HasSlotData(selectedSlot);

        if (saveButton != null)
            saveButton.interactable = hasSelection && currentMode == SavePanelMode.Save;

        if (loadButton != null)
            loadButton.interactable = hasSelection && slotHasData;

        if (deleteButton != null)
            deleteButton.interactable = hasSelection && slotHasData;

        if (deleteAllButton != null)
        {
            // 有任何槽位有数据时才可点
            bool anyData = false;
            for (int i = 0; i < SaveManager.SLOT_COUNT; i++)
            {
                if (SaveManager.HasSlotData(i)) { anyData = true; break; }
            }
            deleteAllButton.interactable = anyData;
        }
    }

    // ================== 保存 ==================
    void OnSaveClicked()
    {
        if (selectedSlot < 0) return;

        if (SaveManager.HasSlotData(selectedSlot))
        {
            ShowConfirmDialog($"存档 {selectedSlot + 1} 已有数据，确认覆盖？", PendingAction.Save);
        }
        else
        {
            DoSave();
        }
    }

    void DoSave()
    {
        StartCoroutine(DoSaveRoutine());
    }

    System.Collections.IEnumerator DoSaveRoutine()
    {
        SaveData data = new SaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            chapterName = "",
            playerName = PlayerPrefs.GetString("PlayerName", "勇者")
        };

        if (dialogueManager != null)
        {
            data.sequenceID = dialogueManager.GetCurrentSequenceID();
            data.lineIndex = dialogueManager.GetCurrentLineIndex();
            data.chapterName = dialogueManager.GetCurrentChapterName();
        }

        AffectionManager.FillSaveData(data);

        // 截图
        CanvasGroup selfCg = GetComponent<CanvasGroup>();
        if (selfCg == null) selfCg = gameObject.AddComponent<CanvasGroup>();
        float oldAlpha = selfCg.alpha;
        selfCg.alpha = 0f;

        GameObject bottomBarGo = PanelManager.Instance != null && PanelManager.Instance.bottomBar != null
            ? PanelManager.Instance.bottomBar.gameObject : null;
        bool bottomBarWasActive = bottomBarGo != null && bottomBarGo.activeSelf;
        if (bottomBarWasActive) bottomBarGo.SetActive(false);

        GameObject dialogPanelGo = dialogueManager != null ? dialogueManager.dialoguePanel : null;
        bool dialogWasActive = dialogPanelGo != null && dialogPanelGo.activeSelf;
        if (dialogWasActive) dialogPanelGo.SetActive(false);

        yield return new WaitForEndOfFrame();

        Texture2D screenshot = null;
        try { screenshot = ScreenCapture.CaptureScreenshotAsTexture(); }
        catch (System.Exception e) { GameLog.Warning($"[SavePanel] 截图失败: {e.Message}"); }

        selfCg.alpha = oldAlpha;
        if (bottomBarWasActive) bottomBarGo.SetActive(true);
        if (dialogWasActive) dialogPanelGo.SetActive(true);

        if (screenshot != null)
        {
            try
            {
                string fileName = $"slot_{selectedSlot}.png";
                string folder = SaveManager.EnsureScreenshotFolder();
                string path = System.IO.Path.Combine(folder, fileName);

                byte[] png = screenshot.EncodeToPNG();
                Destroy(screenshot);

                System.IO.File.WriteAllBytes(path, png);
                data.screenshotFile = fileName;
            }
            catch (System.Exception e)
            {
                GameLog.Warning($"[SavePanel] 保存截图失败: {e.Message}");
            }
        }

        SaveManager.SaveToSlot(selectedSlot, data);

        SaveScreenshotLoader.Clear();
        RefreshAllSlots();
        if (slots != null && selectedSlot < slots.Length && slots[selectedSlot] != null)
            slots[selectedSlot].SetSelected(true);

        RefreshPreview(SaveManager.LoadFromSlot(selectedSlot));

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("save_success");

        ShowToast("保存成功");
        UpdateButtonStates();
    }

    // ================== 读取 ==================
    void OnLoadClicked()
    {
        if (selectedSlot < 0) return;

        SaveData data = SaveManager.LoadFromSlot(selectedSlot);
        if (data == null) return;

        if (!string.IsNullOrEmpty(data.playerName))
            PlayerPrefs.SetString("PlayerName", data.playerName);

        AffectionManager.LoadFromSaveData(data);

        string currentScene = SceneManager.GetActiveScene().name;

        if (data.sceneName == currentScene && dialogueManager != null)
        {
            dialogueManager.LoadFromSave(data.sequenceID, data.lineIndex);
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("load_success");
            Close();
        }
        else
        {
            PlayerPrefs.SetInt("PendingLoadIndex", data.lineIndex);
            PlayerPrefs.SetString("PendingLoadSeqID", data.sequenceID);
            Time.timeScale = previousTimeScale;

            if (SceneFader.Instance != null) SceneFader.Instance.LoadScene(data.sceneName);
            else SceneManager.LoadScene(data.sceneName);
        }
    }

    // ================== 删除 ==================
    void OnDeleteClicked()
    {
        if (selectedSlot < 0) return;
        if (!SaveManager.HasSlotData(selectedSlot)) return;

        ShowConfirmDialog($"确认删除存档 {selectedSlot + 1} ？此操作不可撤销。", PendingAction.DeleteOne);
    }

    void OnDeleteAllClicked()
    {
        ShowConfirmDialog("确认删除所有存档？此操作不可撤销。", PendingAction.DeleteAll);
    }

    void DoDeleteSelected()
    {
        SaveManager.DeleteSlot(selectedSlot);

        selectedSlot = -1;
        SaveScreenshotLoader.Clear();
        RefreshAllSlots();
        RefreshPreview(null);
        UpdateButtonStates();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("button_cancel");

        ShowToast("已删除");
    }

    void DoDeleteAll()
    {
        SaveManager.DeleteAllSlots();

        selectedSlot = -1;
        SaveScreenshotLoader.Clear();
        RefreshAllSlots();
        RefreshPreview(null);
        UpdateButtonStates();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("button_cancel");

        ShowToast("已删除所有存档");
    }

    // ================== 确认框 ==================
    void ShowConfirmDialog(string message, PendingAction action)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("confirm_dialog");

        pendingAction = action;

        if (confirmText != null) confirmText.text = message;
        if (confirmDialog != null) confirmDialog.SetActive(true);
    }

    void CloseConfirmDialog()
    {
        pendingAction = PendingAction.None;
        if (confirmDialog != null) confirmDialog.SetActive(false);
    }

    void OnConfirmYes()
    {
        PendingAction action = pendingAction;
        CloseConfirmDialog();

        switch (action)
        {
            case PendingAction.Save: DoSave(); break;
            case PendingAction.DeleteOne: DoDeleteSelected(); break;
            case PendingAction.DeleteAll: DoDeleteAll(); break;
        }
    }

    void OnConfirmNo()
    {
        CloseConfirmDialog();
    }

    // ================== 大预览 ==================
    void RefreshPreview(SaveData data)
    {
        if (data == null)
        {
            if (previewImage != null)
            {
                previewImage.sprite = null;
                previewImage.gameObject.SetActive(false);   // 完全隐藏
            }
            if (previewEmptyHint != null) previewEmptyHint.SetActive(true);
            if (previewTitleText != null) previewTitleText.text = "";
            if (previewTimeText != null) previewTimeText.text = "";
            return;
        }

        if (previewEmptyHint != null) previewEmptyHint.SetActive(false);

        if (previewImage != null)
        {
            Sprite sprite = SaveScreenshotLoader.Load(data.screenshotFile);
            if (sprite != null)
            {
                previewImage.gameObject.SetActive(true);   // 有图才显示
                previewImage.sprite = sprite;
                previewImage.color = Color.white;
                previewImage.preserveAspect = true;
            }
            else
            {
                previewImage.sprite = null;
                previewImage.gameObject.SetActive(false);   // 没图也隐藏
            }
        }

        string title = data.chapterName;
        if (string.IsNullOrEmpty(title)) title = data.sequenceID;
        if (string.IsNullOrEmpty(title)) title = "未知";

        if (previewTitleText != null) previewTitleText.text = title;

        if (previewTimeText != null)
        {
            string name = string.IsNullOrEmpty(data.playerName) ? "" : $" · {data.playerName}";
            previewTimeText.text = data.saveTime + name;
        }
    }

    // ================== Toast ==================
    void ShowToast(string msg)
    {
        if (toastRoot == null) return;
        toastRoot.SetActive(true);
        if (toastLabel != null) toastLabel.text = msg;
        toastTimer = 1.5f;

        if (selectedSlot >= 0 && slots != null && selectedSlot < slots.Length && slots[selectedSlot] != null)
        {
            StartCoroutine(FlyToSlot(slots[selectedSlot].GetComponent<RectTransform>()));
        }
    }

    System.Collections.IEnumerator FlyToSlot(RectTransform targetSlot)
    {
        if (targetSlot == null) yield break;

        GameObject flyObj = new GameObject("FlyIcon", typeof(RectTransform));
        flyObj.transform.SetParent(transform, false);

        Image img = flyObj.AddComponent<Image>();
        img.sprite = RippleSpriteGenerator.GetRing();
        img.color = new Color(1f, 0.85f, 0.5f, 1f);
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(40f, 40f);
        rt.anchoredPosition = Vector2.zero;

        RectTransform self = GetComponent<RectTransform>();
        Vector2 worldPos = targetSlot.TransformPoint(targetSlot.rect.center);
        Vector2 endPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            self,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out endPos);

        float duration = 0.6f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = p * p * (3f - 2f * p);

            rt.anchoredPosition = Vector2.Lerp(Vector2.zero, endPos, eased);
            rt.localScale = Vector3.one * Mathf.Lerp(1f, 0.4f, eased);
            yield return null;
        }

        Image flash = targetSlot.GetComponent<Image>();
        if (flash != null)
        {
            Color orig = flash.color;
            float ft = 0f;
            while (ft < 0.3f)
            {
                ft += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(ft / 0.3f);
                flash.color = Color.Lerp(new Color(1f, 0.9f, 0.5f, 1f), orig, p);
                yield return null;
            }
            flash.color = orig;
        }

        Destroy(flyObj);
    }
}