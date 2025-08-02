using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/// <summary>
/// Manages background music transitions between menu and session scenes.
/// Loads music and volume settings from Resources and applies saved user volume settings.
/// Implements a singleton to persist across scene loads.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance; // Singleton instance for global access

    private AudioSource audioSource; // AudioSource for playing music

    private AudioClip menuMusic; // Music clip for menu scenes
    private AudioClip sessionMusic; // Music clip for session (gameplay) scenes

    // Array of scene names that are considered part of a gameplay session
    private string[] sessionScenes = { "Level_1", "Level_2", "Level_3", "Level_4", "Cutscene", "Loading" };

    private AudioMixer audioMixer; // Reference to the main AudioMixer used for volume control

    void Awake()
    {
        // Ensure only one instance exists (singleton pattern)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep this object alive across scene loads

        // Set up AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Load AudioMixer from Resources and assign "Master" group
        audioMixer = Resources.Load<AudioMixer>("MainAudioMixer");
        if (audioMixer != null)
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("Master");
            if (groups.Length > 0)
            {
                audioSource.outputAudioMixerGroup = groups[0];
            }
            else
            {
                Debug.LogWarning("No AudioMixerGroup named 'Master' found in MainAudioMixer.");
            }
        }
        else
        {
            Debug.LogWarning("AudioMixer 'MainAudioMixer' not found in Resources folder.");
        }

        // Load music clips from Resources
        menuMusic = Resources.Load<AudioClip>("Audio/MenuMusic");
        sessionMusic = Resources.Load<AudioClip>("Audio/SessionMusic");

        // Register scene load callback
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Start by playing menu music
        PlayMenuMusic();
    }

    /// <summary>
    /// Called whenever a new scene is loaded.
    /// Applies saved volume settings and switches music based on scene type.
    /// </summary>
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySavedVolume(); // Set volume according to saved user settings

        if (IsSessionScene(scene.name))
        {
            PlaySessionMusic(); // Switch to session music if in gameplay scene
        }
        else
        {
            PlayMenuMusic(); // Switch to menu music for other scenes
        }
    }

    /// <summary>
    /// Checks if the current scene is a gameplay session scene.
    /// </summary>
    bool IsSessionScene(string sceneName)
    {
        foreach (string name in sessionScenes)
        {
            if (sceneName.Contains(name))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Plays the menu music clip if not already playing.
    /// </summary>
    void PlayMenuMusic()
    {
        if (audioSource.clip != menuMusic)
        {
            audioSource.clip = menuMusic;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Plays the session music clip if not already playing.
    /// </summary>
    void PlaySessionMusic()
    {
        if (audioSource.clip != sessionMusic)
        {
            audioSource.clip = sessionMusic;
            audioSource.Play();
        }
    }

    /// <summary>
    /// Loads saved volume settings from the database and applies them to the AudioMixer.
    /// Converts linear volume (0–100) to decibels.
    /// </summary>
    void ApplySavedVolume()
    {
        var settings = DatabaseManager.Instance?.LoadSettings();
        if (settings.HasValue)
        {
            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f); // Convert to 0–1 range
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f; // Convert to dB scale
            audioMixer.SetFloat("Volume", db);
            Debug.Log("Applied volume from saved settings: " + db + " dB");
        }
        else
        {
            Debug.LogWarning("No saved volume found in DB to apply.");
        }
    }
}
