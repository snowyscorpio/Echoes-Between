using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(LoadSettingsDelayed());
    }

    IEnumerator LoadSettingsDelayed()
    {
        yield return null;

        ApplySavedSettings();
    }

    public void ApplySavedSettings()
    {
        var settings = DatabaseManager.Instance?.LoadSettings();
        if (settings.HasValue)
        {
            string[] parts = settings.Value.resolution.Split('x');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int w) &&
                int.TryParse(parts[1], out int h))
            {
                Screen.SetResolution(w, h, Screen.fullScreen);
            }

            int graphicsIndex = System.Array.IndexOf(QualitySettings.names, settings.Value.graphics);
            if (graphicsIndex >= 0)
                QualitySettings.SetQualityLevel(graphicsIndex);

            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f);
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f;
            audioMixer.SetFloat("Volume", db);

            Debug.Log("Settings loaded and applied on startup.");

            OptionMenu menu = Object.FindFirstObjectByType<OptionMenu>();
            if (menu != null)
            {
                menu.UpdateUIFromSettings(settings.Value);
                Debug.Log("OptionMenu UI updated from loaded settings.");
            }
        }
        else
        {
            Debug.LogWarning("No settings found in DB.");
        }
    }

}
