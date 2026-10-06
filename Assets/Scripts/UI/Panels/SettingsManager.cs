using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    [Header("音量")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("打字机设置")]
    public Slider typingSpeedSlider;
    public Slider autoIntervalSlider;
    public Toggle typewriterToggle;
    public Toggle typingSoundToggle;

    [Header("显示")]
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;

    [Header("重置")]
    public Button resetButton;

    void Start()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            var options = new System.Collections.Generic.List<string>();
            foreach (var r in GameSettings.resolutions)
                options.Add($"{r.x} x {r.y}");
            resolutionDropdown.AddOptions(options);
        }

        LoadUI();
        BindEvents();
    }

    void LoadUI()
    {
        if (masterSlider) masterSlider.value = GameSettings.masterVolume;
        if (bgmSlider) bgmSlider.value = GameSettings.bgmVolume;
        if (sfxSlider) sfxSlider.value = GameSettings.sfxVolume;
        if (typingSpeedSlider) typingSpeedSlider.value = GameSettings.typingSpeed;
        if (autoIntervalSlider) autoIntervalSlider.value = GameSettings.autoInterval;
        if (typewriterToggle) typewriterToggle.isOn = GameSettings.useTypewriter;
        if (typingSoundToggle) typingSoundToggle.isOn = GameSettings.useTypingSound;
        if (fullscreenToggle) fullscreenToggle.isOn = GameSettings.isFullscreen;
        if (resolutionDropdown) resolutionDropdown.value = GameSettings.resolutionIndex;
    }

    void BindEvents()
    {
        if (masterSlider)
            masterSlider.onValueChanged.AddListener(v =>
            {
                GameSettings.masterVolume = v;
                GameSettings.ApplyVolume();
            });

        if (bgmSlider)
            bgmSlider.onValueChanged.AddListener(v => GameSettings.bgmVolume = v);

        if (sfxSlider)
            sfxSlider.onValueChanged.AddListener(v => GameSettings.sfxVolume = v);

        if (typingSpeedSlider)
            typingSpeedSlider.onValueChanged.AddListener(v => GameSettings.typingSpeed = v);

        if (autoIntervalSlider)
            autoIntervalSlider.onValueChanged.AddListener(v => GameSettings.autoInterval = v);

        if (typewriterToggle)
            typewriterToggle.onValueChanged.AddListener(v => GameSettings.useTypewriter = v);

        if (typingSoundToggle)
            typingSoundToggle.onValueChanged.AddListener(v => GameSettings.useTypingSound = v);

        if (fullscreenToggle)
            fullscreenToggle.onValueChanged.AddListener(v =>
            {
                GameSettings.isFullscreen = v;
                GameSettings.ApplyDisplay();
            });

        if (resolutionDropdown)
            resolutionDropdown.onValueChanged.AddListener(i =>
            {
                GameSettings.resolutionIndex = i;
                GameSettings.ApplyDisplay();
            });

        if (resetButton)
            resetButton.onClick.AddListener(() =>
            {
                GameSettings.ResetAll();
                LoadUI();
                GameSettings.ApplyAll();
            });
    }
}