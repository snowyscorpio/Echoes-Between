using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

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

    [Header("Navigation")]
    public Button backButton; // כפתור חזרה

    private List<Resolution> availableResolutions = new List<Resolution>();

    private void Start()
    {
        Debug.Log("OptionMenu Start: checking UI bindings");

        Debug.Log("resolutionDropdown: " + (resolutionDropdown != null));
        Debug.Log("fullscreenToggle: " + (fullscreenToggle != null));
        Debug.Log("graphicsDropdown: " + (graphicsDropdown != null));
        Debug.Log("volumeSlider: " + (volumeSlider != null));
        Debug.Log("audioMixer: " + (audioMixer != null));

        SetupResolutionDropdown();
        SetupGraphicsDropdown();
        SetupVolumeSlider();
        StartCoroutine(LoadSettingsDelayed());

        if (backButton != null)
            backButton.onClick.AddListener(GoBackToPreviousScene);
    }

    private IEnumerator LoadSettingsDelayed()
    {
        yield return null; // תחכי פריים אחד שכולם יאוחזרו
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
        Screen.SetResolution(res.width, res.height, false);

        Debug.Log("Resolution changed to: " + res.width + "x" + res.height);

        if (fullscreenToggle.isOn)
            fullscreenToggle.isOn = false;
    }

    void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log("Fullscreen: " + isFullscreen);

        if (isFullscreen)
        {
            Resolution current = Screen.currentResolution;
            Screen.SetResolution(current.width, current.height, true);

            Debug.Log("Resolution set for fullscreen: " + current.width + "x" + current.height);

            int currentIndex = availableResolutions.FindIndex(r =>
                r.width == current.width && r.height == current.height);

            if (currentIndex >= 0)
            {
                resolutionDropdown.value = currentIndex;
                resolutionDropdown.RefreshShownValue();
            }
        }
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
        volumeSlider.minValue = 0.0001f;
        volumeSlider.maxValue = 0.5f;

        if (audioMixer.GetFloat("Volume", out float volume))
        {
            float normalized = Mathf.Clamp(Mathf.Pow(10, volume / 20f), volumeSlider.minValue, volumeSlider.maxValue);
            volumeSlider.value = normalized;
        }

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    void SetVolume(float value)
    {
        float db = Mathf.Log10(Mathf.Clamp(value, volumeSlider.minValue, volumeSlider.maxValue)) * 20;
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

    public void ApplyCurrentSettings()
    {
        ChangeResolution(resolutionDropdown.value);
        ChangeFullscreen(fullscreenToggle.isOn);
        ChangeGraphicsQuality(graphicsDropdown.value);
        SetVolume(volumeSlider.value);
    }

    void LoadSettingsFromDB()
    {
        try
        {
            var settings = DatabaseManager.Instance.LoadSettings();
            if (settings.HasValue)
            {
                Debug.Log("Settings loaded from DB: " + settings.Value);

                Debug.Log("Calling UpdateUIFromSettings...");
                UpdateUIFromSettings(settings.Value);

                Debug.Log("Calling ApplyCurrentSettings...");
                ApplyCurrentSettings();

                Debug.Log("Settings loaded and applied.");
            }
            else
            {
                Debug.LogWarning("No saved settings found in DB.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in LoadSettingsFromDB: " + ex.Message);
            Debug.LogError("StackTrace: " + ex.StackTrace);
        }
    }



    public void UpdateUIFromSettings((string resolution, string graphics, int volume) settings)
    {
        if (string.IsNullOrEmpty(settings.resolution))
        {
            Debug.LogError("resolution from DB is null or empty");
            return;
        }

        if (string.IsNullOrEmpty(settings.graphics))
        {
            Debug.LogError("graphics from DB is null or empty");
            return;
        }

        string[] parts = settings.resolution.Split('x');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int w) &&
            int.TryParse(parts[1], out int h))
        {
            int index = availableResolutions.FindIndex(r => r.width == w && r.height == h);
            if (index >= 0)
            {
                resolutionDropdown.value = index;
                resolutionDropdown.RefreshShownValue();
            }
        }

        int graphicsIndex = System.Array.IndexOf(QualitySettings.names, settings.graphics);
        if (graphicsIndex >= 0)
        {
            graphicsDropdown.value = graphicsIndex;
            graphicsDropdown.RefreshShownValue();
        }

        float volumeValue = Mathf.Clamp01(settings.volume / 100f);
        volumeSlider.value = Mathf.Clamp(volumeValue, volumeSlider.minValue, volumeSlider.maxValue);
    }



    void GoBackToPreviousScene()
    {
        string previous = GameManager.Instance?.LastSceneBeforeOptions;
        if (!string.IsNullOrEmpty(previous))
        {
            SceneManager.LoadScene(previous);
        }
        else
        {
            SceneManager.LoadScene(0); // fallback
        }
    }
}
