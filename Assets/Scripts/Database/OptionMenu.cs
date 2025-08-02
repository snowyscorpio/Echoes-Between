using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the options/settings menu including resolution, graphics quality, volume, and navigation.
/// </summary>
public class OptionMenu : MonoBehaviour
{
    [Header("Resolution")]
    public TMP_Dropdown resolutionDropdown; // Dropdown for choosing resolution
    public Toggle fullscreenToggle; // Toggle for fullscreen mode

    [Header("Graphics")]
    public TMP_Dropdown graphicsDropdown; // Dropdown for graphics quality levels

    [Header("Volume")]
    public Slider volumeSlider; // Slider controlling master volume
    public AudioMixer audioMixer; // Audio mixer to apply volume change

    [Header("Navigation")]
    public Button backButton; // Button to go back to previous scene

    private List<Resolution> availableResolutions = new List<Resolution>(); // Cached unique resolutions

    /// <summary>
    /// Unity start lifecycle. Sets up UI components, loads settings, and binds navigation.
    /// </summary>
    private void Start()
    {
        Debug.Log("OptionMenu Start: checking UI bindings");

        Debug.Log("resolutionDropdown: " + (resolutionDropdown != null)); // log presence for debugging
        Debug.Log("fullscreenToggle: " + (fullscreenToggle != null));
        Debug.Log("graphicsDropdown: " + (graphicsDropdown != null));
        Debug.Log("volumeSlider: " + (volumeSlider != null));
        Debug.Log("audioMixer: " + (audioMixer != null));

        SetupResolutionDropdown(); // populate resolution options and hook listeners
        SetupGraphicsDropdown(); // populate graphics quality options
        SetupVolumeSlider(); // configure volume slider with current mixer state
        StartCoroutine(LoadSettingsDelayed()); // defer loading saved settings one frame

        if (backButton != null)
            backButton.onClick.AddListener(GoBackToPreviousScene); // wire back navigation
    }

    /// <summary>
    /// Waits one frame then loads saved settings from the database.
    /// </summary>
    private IEnumerator LoadSettingsDelayed()
    {
        yield return null; // wait a single frame to ensure UI is initialized
        LoadSettingsFromDB(); // attempt to fetch saved settings and apply them
    }

    /// <summary>
    /// Populates resolution dropdown with unique resolutions and hooks up change handlers.
    /// </summary>
    void SetupResolutionDropdown()
    {
        resolutionDropdown.ClearOptions(); // remove any placeholder options
        HashSet<string> unique = new HashSet<string>(); // ensure no duplicates
        List<string> options = new List<string>(); // string list for dropdown display

        foreach (Resolution res in Screen.resolutions)
        {
            string option = res.width + "x" + res.height; // format width x height
            if (unique.Add(option)) // add only if not already present
            {
                availableResolutions.Add(res); // store corresponding resolution struct
                options.Add(option); // add label to dropdown list
            }
        }

        resolutionDropdown.AddOptions(options); // set dropdown contents

        // Find currently active resolution in list
        int currentIndex = availableResolutions.FindIndex(r =>
            r.width == Screen.currentResolution.width &&
            r.height == Screen.currentResolution.height);

        // Select current resolution or fallback to first
        resolutionDropdown.value = currentIndex >= 0 ? currentIndex : 0;
        resolutionDropdown.RefreshShownValue(); // update UI display

        fullscreenToggle.isOn = Screen.fullScreen; // reflect current fullscreen state

        // Bind change events to handlers
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);
        fullscreenToggle.onValueChanged.AddListener(ChangeFullscreen);
    }

    /// <summary>
    /// Changes the screen resolution to the one selected by index and disables fullscreen if active.
    /// </summary>
    void ChangeResolution(int index)
    {
        Resolution res = availableResolutions[index]; // get selected resolution
        Screen.SetResolution(res.width, res.height, false); // apply resolution explicitly in windowed mode

        Debug.Log("Resolution changed to: " + res.width + "x" + res.height);

        if (fullscreenToggle.isOn) // if fullscreen toggle was on, turn it off because we forced windowed
            fullscreenToggle.isOn = false;
    }

    /// <summary>
    /// Toggles fullscreen mode and adjusts resolution if entering fullscreen.
    /// </summary>
    void ChangeFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen; // apply fullscreen flag
        Debug.Log("Fullscreen: " + isFullscreen);

        if (isFullscreen)
        {
            Resolution current = Screen.currentResolution; // get native fullscreen resolution
            Screen.SetResolution(current.width, current.height, true); // apply fullscreen with native size

            Debug.Log("Resolution set for fullscreen: " + current.width + "x" + current.height);

            // Try to reflect that resolution in the dropdown if present
            int currentIndex = availableResolutions.FindIndex(r =>
                r.width == current.width && r.height == current.height);

            if (currentIndex >= 0)
            {
                resolutionDropdown.value = currentIndex; // update dropdown selection
                resolutionDropdown.RefreshShownValue(); // refresh displayed text
            }
        }
    }

    /// <summary>
    /// Populates graphics quality dropdown and binds change handler.
    /// </summary>
    void SetupGraphicsDropdown()
    {
        graphicsDropdown.ClearOptions(); // clear any default
        string[] qualityLevels = QualitySettings.names; // get quality level names from Unity
        graphicsDropdown.AddOptions(new List<string>(qualityLevels)); // populate dropdown
        graphicsDropdown.value = QualitySettings.GetQualityLevel(); // set current quality as selected
        graphicsDropdown.RefreshShownValue(); // update label
        graphicsDropdown.onValueChanged.AddListener(ChangeGraphicsQuality); // hook change event
    }

    /// <summary>
    /// Applies selected graphics quality level.
    /// </summary>
    void ChangeGraphicsQuality(int index)
    {
        QualitySettings.SetQualityLevel(index); // set quality via Unity API
    }

    /// <summary>
    /// Configures the volume slider based on current audio mixer state and binds listener.
    /// </summary>
    void SetupVolumeSlider()
    {
        volumeSlider.minValue = 0.0001f; // avoid zero to keep log math valid
        volumeSlider.maxValue = 0.5f; // cap maximum

        if (audioMixer.GetFloat("Volume", out float volume)) // retrieve current dB volume
        {
            // Convert dB back to normalized linear value for slider
            float normalized = Mathf.Clamp(Mathf.Pow(10, volume / 20f), volumeSlider.minValue, volumeSlider.maxValue);
            volumeSlider.value = normalized; // set slider position
        }

        volumeSlider.onValueChanged.AddListener(SetVolume); // bind live adjustment
    }

    /// <summary>
    /// Sets the audio mixer's volume in decibels based on slider value.
    /// </summary>
    void SetVolume(float value)
    {
        // Convert linear slider value to decibels (logarithmic scaling)
        float db = Mathf.Log10(Mathf.Clamp(value, volumeSlider.minValue, volumeSlider.maxValue)) * 20;
        audioMixer.SetFloat("Volume", db); // apply to mixer
    }

    /// <summary>
    /// Saves the current settings (resolution, graphics, volume) to the database.
    /// </summary>
    public void SaveSettingsToDB()
    {
        // Build string representations for persistence
        string resolutionStr = availableResolutions[resolutionDropdown.value].width + "x" + availableResolutions[resolutionDropdown.value].height;
        string graphicsStr = QualitySettings.names[graphicsDropdown.value];
        int volumeInt = Mathf.RoundToInt(volumeSlider.value * 100); // convert to 0-100 scale

        DatabaseManager.Instance.SaveSettings(resolutionStr, graphicsStr, volumeInt); // delegate save
        Debug.Log("Settings saved to database."); // confirm log
    }

    /// <summary>
    /// Applies the currently selected UI settings to the system (resolution, fullscreen, graphics, volume).
    /// </summary>
    public void ApplyCurrentSettings()
    {
        ChangeResolution(resolutionDropdown.value); // apply resolution
        ChangeFullscreen(fullscreenToggle.isOn); // apply fullscreen preference
        ChangeGraphicsQuality(graphicsDropdown.value); // apply graphics quality
        SetVolume(volumeSlider.value); // apply volume
    }

    /// <summary>
    /// Loads saved settings from the database and updates UI and applied state accordingly.
    /// </summary>
    void LoadSettingsFromDB()
    {
        try
        {
            var settings = DatabaseManager.Instance.LoadSettings(); // fetch stored settings
            if (settings.HasValue)
            {
                Debug.Log("Settings loaded from DB: " + settings.Value);

                Debug.Log("Calling UpdateUIFromSettings...");
                UpdateUIFromSettings(settings.Value); // sync dropdowns/slider with loaded data

                Debug.Log("Calling ApplyCurrentSettings...");
                ApplyCurrentSettings(); // enforce the settings immediately

                Debug.Log("Settings loaded and applied.");
            }
            else
            {
                Debug.LogWarning("No saved settings found in DB."); // nothing found fallback
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Error in LoadSettingsFromDB: " + ex.Message); // log exceptions
            Debug.LogError("StackTrace: " + ex.StackTrace);
        }
    }

    /// <summary>
    /// Updates UI dropdowns and slider based on loaded settings tuple.
    /// </summary>
    public void UpdateUIFromSettings((string resolution, string graphics, int volume) settings)
    {
        if (string.IsNullOrEmpty(settings.resolution))
        {
            Debug.LogError("resolution from DB is null or empty");
            return; // early exit on invalid data
        }

        if (string.IsNullOrEmpty(settings.graphics))
        {
            Debug.LogError("graphics from DB is null or empty");
            return; // early exit on invalid data
        }

        // Parse resolution string like "1920x1080"
        string[] parts = settings.resolution.Split('x');
        if (parts.Length == 2 &&
            int.TryParse(parts[0], out int w) &&
            int.TryParse(parts[1], out int h))
        {
            // Find matching resolution in the previously populated list
            int index = availableResolutions.FindIndex(r => r.width == w && r.height == h);
            if (index >= 0)
            {
                resolutionDropdown.value = index; // set dropdown value
                resolutionDropdown.RefreshShownValue(); // update UI label
            }
        }

        // Map graphics name to index
        int graphicsIndex = System.Array.IndexOf(QualitySettings.names, settings.graphics);
        if (graphicsIndex >= 0)
        {
            graphicsDropdown.value = graphicsIndex; // update quality dropdown
            graphicsDropdown.RefreshShownValue();
        }

        // Normalize volume from 0-100 to 0-1 and clamp within slider bounds
        float volumeValue = Mathf.Clamp01(settings.volume / 100f);
        volumeSlider.value = Mathf.Clamp(volumeValue, volumeSlider.minValue, volumeSlider.maxValue); // set slider
    }

    /// <summary>
    /// Navigates back to the previous scene saved in GameManager, or falls back to main menu.
    /// </summary>
    void GoBackToPreviousScene()
    {
        string previous = GameManager.Instance?.LastSceneBeforeOptions; // retrieve stored previous scene
        if (!string.IsNullOrEmpty(previous))
        {
            SceneManager.LoadScene(previous); // go back to saved scene
        }
        else
        {
            SceneManager.LoadScene(0); // fallback to main menu if none saved
        }
    }
}
