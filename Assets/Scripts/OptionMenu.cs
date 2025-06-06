using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;

public class OptionMenu : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;

    [Header("Graphics")]
    public TMP_Dropdown graphicsDropdown;

    [Header("Volume")]
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    private List<Resolution> availableResolutions = new List<Resolution>();

    private void Start()
    {
        SetupResolutionDropdown();
        SetupGraphicsDropdown();
        SetupVolumeSlider();
        LoadSettingsFromDB();
    }

    void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        HashSet<string> unique = new HashSet<string>();
        List<string> options = new List<string>();

        foreach (Resolution res in Screen.resolutions)
        {
            string option = res.width + "x" + res.height;
            if (unique.Add(option))
            {
                availableResolutions.Add(res);
                options.Add(option);
            }
        }

        resolutionDropdown.AddOptions(options);

        int currentIndex = availableResolutions.FindIndex(r =>
            r.width == Screen.currentResolution.width &&
            r.height == Screen.currentResolution.height);

        resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = Screen.fullScreen;

        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
    }

    void ChangeResolution(int index)
    {
        Resolution res = availableResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);

    }

    void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Resolution res = availableResolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, isFullscreen);

    }

    void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions();
        string[] qualityLevels = QualitySettings.names;
        graphicsDropdown.AddOptions(new List<string>(qualityLevels));
        graphicsDropdown.value = QualitySettings.GetQualityLevel();
        graphicsDropdown.RefreshShownValue();
        graphicsDropdown.onValueChanged.AddListener(ChangeGraphicsQuality);
    }

    void ChangeGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index);

    }

    void SetupVolumeSlider()
    {
        if (audioMixer.GetFloat("Volume", out float volume))
        {
            volumeSlider.value = Mathf.Pow(10, volume / 20f);
        }

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        audioMixer.SetFloat("Volume", db);

    }

    public void SaveSettingsToDB()
    {
        string resolutionStr = availableResolutions[resolutionDropdown.value].width + "x" + availableResolutions[resolutionDropdown.value].height;
        string graphicsStr = QualitySettings.names[graphicsDropdown.value];
        int volumeInt = Mathf.RoundToInt(volumeSlider.value * 100);

        DatabaseManager.Instance.SaveSettings(resolutionStr, graphicsStr, volumeInt);
        Debug.Log("Settings saved to database.");
    }

    void LoadSettingsFromDB()
    {
        var settings = DatabaseManager.Instance.LoadSettings();
        if (settings.HasValue)
        {
            string[] parts = settings.Value.resolution.Split('x');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int w) &&
                int.TryParse(parts[1], out int h))
            {
                int index = availableResolutions.FindIndex(r => r.width == w && r.height == h);
                if (index >= 0)
                {
                    resolutionDropdown.value = index;
                    resolutionDropdown.RefreshShownValue();
                    Screen.SetResolution(w, h, fullscreenToggle.isOn);
                }
            }

            int graphicsIndex = System.Array.IndexOf(QualitySettings.names, settings.Value.graphics);
            if (graphicsIndex >= 0)
            {
                graphicsDropdown.value = graphicsIndex;
                graphicsDropdown.RefreshShownValue();
                QualitySettings.SetQualityLevel(graphicsIndex);
            }

            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f);
            volumeSlider.value = volumeValue;
            float db = Mathf.Log10(volumeValue) * 20f;
            audioMixer.SetFloat("Volume", db);
        }
    }
}
