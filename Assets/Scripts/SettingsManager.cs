using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

/// <summary>
/// This class manages game settings such as screen resolution, graphics quality, and volume.
/// It loads saved settings from the database when the game starts and applies them.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // Reference to the AudioMixer used to control volume in decibels
    public AudioMixer audioMixer;

    void Awake()
    {
        // Prevent this GameObject from being destroyed when loading a new scene
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Start loading settings after one frame delay, to ensure everything is initialized
        StartCoroutine(LoadSettingsDelayed());
    }

    /// <summary>
    /// Waits for one frame before loading and applying saved settings.
    /// This avoids potential issues with accessing uninitialized components.
    /// </summary>
    IEnumerator LoadSettingsDelayed()
    {
        yield return null; // Wait one frame
        ApplySavedSettings(); // Apply settings after delay
    }

    /// <summary>
    /// Loads saved settings from the database and applies them to the game.
    /// This includes:
    /// - Setting screen resolution
    /// - Setting graphics quality level
    /// - Setting master volume using the AudioMixer
    /// - Updating the settings UI if it exists in the scene
    /// </summary>
    public void ApplySavedSettings()
    {
        // Try to get the saved settings from the database via the DatabaseManager singleton
        var settings = DatabaseManager.Instance?.LoadSettings();

        // Check if we successfully retrieved saved settings
        if (settings.HasValue)
        {
            // ----- Apply screen resolution -----
            // Expected format: "1920x1080"
            string[] parts = settings.Value.resolution.Split('x');

            // Parse width and height from resolution string
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int w) &&
                int.TryParse(parts[1], out int h))
            {
                // Apply resolution with current fullscreen state
                Screen.SetResolution(w, h, Screen.fullScreen);
            }

            // ----- Apply graphics quality -----
            // Find the index of the quality setting name (e.g., "High", "Medium") in the predefined names
            int graphicsIndex = System.Array.IndexOf(QualitySettings.names, settings.Value.graphics);
            if (graphicsIndex >= 0)
                QualitySettings.SetQualityLevel(graphicsIndex); // Apply quality level if found

            // ----- Apply volume -----
            // Convert volume from 0-100 range to decibel scale (0 means mute)
            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f);
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f;

            // Set the calculated decibel value in the AudioMixer (usually affects master volume)
            audioMixer.SetFloat("Volume", db);

            Debug.Log("Settings loaded and applied on startup.");

            // ----- Update UI if exists -----
            // Look for an OptionMenu in the scene to update the UI with the loaded settings
            OptionMenu menu = Object.FindFirstObjectByType<OptionMenu>();
            if (menu != null)
            {
                menu.UpdateUIFromSettings(settings.Value);
                Debug.Log("OptionMenu UI updated from loaded settings.");
            }
        }
        else
        {
            // If no settings were found in the DB, show a warning in the console
            Debug.LogWarning("No settings found in DB.");
        }
    }
}
