using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("设置面板")]
    public GameObject settingsPanel;
    public Slider volumeSlider;

    [Header("存档面板")]
    public SavePanel savePanel;

    [Header("继续游戏按钮")]
    public Button continueButton;

    void Start()
    {
        GameSettings.ApplyAll();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBGM("bgm_main_menu");

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        // 主菜单快速音量滑块：走 GameSettings，不用 AudioListener
        if (volumeSlider != null)
        {
            volumeSlider.value = GameSettings.masterVolume;
            volumeSlider.onValueChanged.AddListener(v =>
            {
                GameSettings.masterVolume = v;
                GameSettings.ApplyVolume();
            });
        }

        if (continueButton != null)
            continueButton.interactable = SaveManager.HasAutoSave();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && settingsPanel != null && settingsPanel.activeSelf)
            settingsPanel.SetActive(false);
    }

    public void StartGame()
    {
        AffectionManager.ResetAll();
        GameVariables.ResetAll();

        PlayerPrefs.DeleteKey("PendingLoadIndex");
        PlayerPrefs.DeleteKey("PendingLoadSeqID");
        PlayerPrefs.DeleteKey("PlayerName");

        if (SceneFader.Instance != null) SceneFader.Instance.LoadScene("Game");
        else SceneManager.LoadScene("Game");
    }

    public void ContinueGame()
    {
        SaveData data = SaveManager.LoadAuto();
        if (data == null) return;

        if (!string.IsNullOrEmpty(data.playerName))
        {
            PlayerPrefs.SetString("PlayerName", data.playerName);
        }

        AffectionManager.LoadFromSaveData(data);

        PlayerPrefs.SetInt("PendingLoadIndex", data.lineIndex);
        PlayerPrefs.SetString("PendingLoadSeqID", data.sequenceID);

        if (SceneFader.Instance != null) SceneFader.Instance.LoadScene(data.sceneName);
        else SceneManager.LoadScene(data.sceneName);
    }

    public void OpenLoadPanel()
    {
        if (savePanel != null) savePanel.Open(SavePanelMode.Load);
    }

    public void OpenSettings() { if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void CloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); }
    public void QuitGame() { Application.Quit(); }
}