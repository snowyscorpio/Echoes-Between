using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.Audio;

/// <summary>
/// Manages the loading screen process, including showing a tip, applying saved volume,
/// displaying progress bar and percentage, and loading the next scene asynchronously.
/// </summary>
public class LoadingManager : MonoBehaviour
{
    public static string SceneToLoad; // The name of the scene to be loaded

    public TMP_Text loadingLabelText;     // UI text for "LOADING..." label
    public TMP_Text loadingPercentText;   // UI text for displaying progress percentage
    public TMP_Text tipText;              // UI text for displaying gameplay tip
    public Slider progressBar;            // Slider to represent loading progress

    public AudioMixer audioMixer;         // Reference to AudioMixer for applying saved volume

    // Array of random tips to be displayed during loading screen
    private string[] tips = {
        "TIP: DON'T FORGET TO SAVE, BE RESPONSIBLE",
        "TIP: BE PATIENT, DON'T RUSH",
        "TIP: DON'T FORGET TO DRINK WATER (IN REAL LIFE)",
        "TIP: IF YOU GO BACK OR TO OPTIONS WITHOUT SAVING YOU WILL LOSE YOUR PROGRESS"
    };

    /// <summary>
    /// Called on loading screen start. Applies saved volume, shows a random tip, and starts loading the next scene.
    /// </summary>
    private void Start()
    {
        ApplySavedVolume();     // Apply volume based on saved user settings
        ShowRandomTip();        // Display a random tip to the user
        StartCoroutine(LoadSceneAsync()); // Begin asynchronous scene loading
    }

    /// <summary>
    /// Loads and applies saved volume settings from the database to the AudioMixer.
    /// </summary>
    private void ApplySavedVolume()
    {
        var settings = DatabaseManager.Instance?.LoadSettings();
        if (settings.HasValue)
        {
            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f); // Convert 0–100 to 0–1
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f; // Convert to dB scale
            audioMixer.SetFloat("Volume", db);
            Debug.Log("Applied volume in LoadingManager: " + db + " dB");
        }
        else
        {
            Debug.LogWarning("No saved volume found for LoadingManager.");
        }
    }

    /// <summary>
    /// Picks a random tip from the list and displays it in the loading screen.
    /// </summary>
    void ShowRandomTip()
    {
        int index = Random.Range(0, tips.Length);
        tipText.text = tips[index];
    }

    /// <summary>
    /// Coroutine that handles asynchronous scene loading,
    /// updates UI elements during progress, and transitions when ready.
    /// </summary>
    IEnumerator LoadSceneAsync()
    {
        yield return new WaitForSeconds(1f); // Optional delay before starting

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneToLoad);
        operation.allowSceneActivation = false; // Prevent immediate activation

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Normalize loading progress
            int percent = Mathf.RoundToInt(progress * 100);           // Convert to percentage

            if (loadingLabelText != null)
                loadingLabelText.text = "LOADING...";
            if (loadingPercentText != null)
                loadingPercentText.text = percent + "%";
            if (progressBar != null)
                progressBar.value = progress;

            // Allow scene activation after loading is nearly complete and enough time has passed
            if (operation.progress >= 0.9f && timer >= 6f)
            {
                loadingLabelText.text = "DONE!";
                loadingPercentText.text = "100%";
                yield return new WaitForSeconds(0.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
