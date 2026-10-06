using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenu : MonoBehaviour
{
    [Header("按钮")]
    public Button settingsButton;
    public Button quitToMainButton;

    [Header("系统引用")]
    public DialogueManager dialogueManager;
    public SavePanel savePanel;

    [Header("设置面板")]
    public GameObject settingsPanel;

    [Header("场景跳转")]
    public Button prevSceneButton;
    public Button nextSceneButton;
    public string prevSceneName = "";
    public string nextSceneName = "";

    void Start()
    {
        GameSettings.ApplyAll();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (quitToMainButton != null)
            quitToMainButton.onClick.AddListener(() => LoadScene("SampleScene"));

        if (prevSceneButton != null)
            prevSceneButton.onClick.AddListener(() => LoadScene(prevSceneName));

        if (nextSceneButton != null)
            nextSceneButton.onClick.AddListener(() => LoadScene(nextSceneName));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                CloseSettings();
                return;
            }

            if (savePanel != null && savePanel.gameObject.activeSelf)
            {
                savePanel.Close();
                return;
            }
        }
    }

    void ToggleSettings()
    {
        if (settingsPanel == null) return;

        if (settingsPanel.activeSelf) CloseSettings();
        else
        {
            if (PanelManager.Instance != null) PanelManager.Instance.OpenOnly(settingsPanel);
            else settingsPanel.SetActive(true);
        }
    }

    void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;
        if (SceneFader.Instance != null) SceneFader.Instance.LoadScene(sceneName);
        else SceneManager.LoadScene(sceneName);
    }

    public void OpenSavePanel()
    {
        if (savePanel != null) savePanel.Open(SavePanelMode.Save);
    }

    public void OpenLoadPanel()
    {
        if (savePanel != null) savePanel.Open(SavePanelMode.Load);
    }

    public void CloseSettings()
    {
        if (settingsPanel == null) return;

        if (PanelManager.Instance != null) PanelManager.Instance.Close(settingsPanel);
        else settingsPanel.SetActive(false);
    }
}