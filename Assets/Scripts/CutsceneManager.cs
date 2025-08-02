using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

/// <summary>
/// Manages the flow of a cutscene consisting of multiple steps (images + sentences).
/// Supports typing effect, skip hints, fade transitions, and loading the next scene.
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneStep
    {
        public Sprite image;                    // The image for this cutscene step
        [TextArea(1, 3)]
        public List<string> sentences;          // Sentences to display for this step
    }

    public Image cutsceneImage;                // UI element to display current image
    public TMP_Text sentenceText;              // UI element to display current sentence
    public CanvasGroup fadeGroup;              // UI group used for fade in/out transitions
    public GameObject sentenceBackground;      // Background panel behind text
    public GameObject skipText;                // Skip hint text object

    public List<CutsceneStep> cutsceneSteps;   // All cutscene steps to display

    public AudioMixer audioMixer;              // Reference to AudioMixer to apply saved volume

    private int currentStepIndex = 0;          // Index of the current cutscene step
    private int currentSentenceIndex = 0;      // Index of the current sentence within the step
    private bool isTyping = false;             // Whether typing animation is active
    private Coroutine typingCoroutine;         // Reference to the typing coroutine

    private CanvasGroup skipCanvasGroup;       // CanvasGroup for blinking skip hint
    private Coroutine blinkCoroutine;          // Reference to the blinking coroutine

    private bool waitingBetweenSteps = false;  // Prevent input during transition between steps

    /// <summary>
    /// Initializes volume, UI, and starts first cutscene step.
    /// </summary>
    void Start()
    {
        ApplySavedVolume(); // Load and apply saved volume from settings

        fadeGroup.alpha = 1; // Start with black screen (fully opaque)
        sentenceText.text = ""; // Clear any existing sentence text

        if (sentenceBackground != null)
            sentenceBackground.SetActive(false); // Hide background behind sentence

        if (skipText != null)
        {
            skipText.SetActive(false); // Hide "press to skip" hint
            skipCanvasGroup = skipText.GetComponent<CanvasGroup>(); // Try to get CanvasGroup

            if (skipCanvasGroup == null)
                skipCanvasGroup = skipText.AddComponent<CanvasGroup>(); // Add if missing
        }

        Debug.Log($"Total steps loaded: {cutsceneSteps.Count}"); // Debug: show number of steps
        StartCoroutine(FadeInAndShowFirst()); // Start cutscene sequence
    }

    /// <summary>
    /// Loads and applies saved volume settings from the database to the AudioMixer.
    /// </summary>
    private void ApplySavedVolume()
    {
        var settings = DatabaseManager.Instance?.LoadSettings(); // Get saved settings from DB

        if (settings.HasValue)
        {
            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f); // Normalize to 0–1
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f; // Convert to dB
            audioMixer.SetFloat("Volume", db); // Apply volume to mixer
            Debug.Log("Applied volume in CutsceneManager: " + db + " dB");
        }
        else
        {
            Debug.LogWarning("No saved volume found for CutsceneManager.");
        }
    }

    /// <summary>
    /// Handles user input to skip typing or move to next sentence/step.
    /// </summary>
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space)) // Listen for Enter or Space
        {
            if (waitingBetweenSteps) // Ignore input during step transitions
            {
                Debug.Log("Ignoring input during pause between steps");
                return;
            }

            if (currentStepIndex >= cutsceneSteps.Count) // Prevent out-of-bounds error
            {
                Debug.LogWarning("CurrentStepIndex is out of bounds!");
                return;
            }

            if (cutsceneSteps[currentStepIndex].sentences == null || cutsceneSteps[currentStepIndex].sentences.Count == 0)
            {
                Debug.LogWarning($"Step {currentStepIndex} has no sentences!");
                return;
            }

            if (isTyping)
            {
                StopCoroutine(typingCoroutine); // Skip typing animation
                sentenceText.text = cutsceneSteps[currentStepIndex].sentences[currentSentenceIndex]; // Show full sentence
                isTyping = false;
            }
            else if (currentSentenceIndex < cutsceneSteps[currentStepIndex].sentences.Count - 1)
            {
                ShowNextSentence(); // Show next sentence in step
            }
            else
            {
                StartCoroutine(TransitionToNextStep()); // Move to next step
            }
        }
    }

    /// <summary>
    /// Fades in and displays the first image and sentence of the cutscene.
    /// </summary>
    IEnumerator FadeInAndShowFirst()
    {
        yield return new WaitForSeconds(1f); // Optional delay before start
        yield return Fade(1, 0, 1f); // Fade from black to clear
        cutsceneImage.sprite = cutsceneSteps[currentStepIndex].image; // Show first image
        yield return new WaitForSeconds(1f); // Wait before showing sentence
        ShowSentence(); // Display sentence
    }

    /// <summary>
    /// Displays the current sentence with typing animation and UI setup.
    /// </summary>
    void ShowSentence()
    {
        Debug.Log($"Showing step {currentStepIndex}, sentence {currentSentenceIndex}");

        if (sentenceBackground != null)
            sentenceBackground.SetActive(true); // Show background behind text

        sentenceText.text = ""; // Clear text before typing

        if (skipText != null)
        {
            skipText.SetActive(true); // Show skip hint
            if (blinkCoroutine != null)
                StopCoroutine(blinkCoroutine); // Stop previous blinking if active
            blinkCoroutine = StartCoroutine(BlinkSkipText()); // Start blinking effect
        }

        string currentSentence = cutsceneSteps[currentStepIndex].sentences[currentSentenceIndex];

        if (string.IsNullOrEmpty(currentSentence))
        {
            Debug.LogWarning("Empty sentence!");
        }

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence)); // Begin typing effect
    }

    /// <summary>
    /// Types the sentence letter by letter with a short delay.
    /// </summary>
    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        sentenceText.text = "";

        for (int i = 0; i <= sentence.Length; i++)
        {
            sentenceText.text = sentence.Substring(0, i); // Show partial sentence
            yield return new WaitForSeconds(0.02f); // Delay between characters
        }

        isTyping = false;
    }

    /// <summary>
    /// Advances to the next sentence in the current cutscene step.
    /// </summary>
    void ShowNextSentence()
    {
        currentSentenceIndex++; // Move to next sentence

        if (currentSentenceIndex < cutsceneSteps[currentStepIndex].sentences.Count)
        {
            ShowSentence(); // Show it
        }
        else
        {
            StartCoroutine(TransitionToNextStep()); // Move to next step if no more sentences
        }
    }

    /// <summary>
    /// Fades out current content and moves to the next cutscene step or to gameplay.
    /// </summary>
    IEnumerator TransitionToNextStep()
    {
        waitingBetweenSteps = true; // Temporarily disable input

        yield return new WaitForSeconds(2f); // Optional pause
        if (sentenceBackground != null) sentenceBackground.SetActive(false);
        if (skipText != null) skipText.SetActive(false);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        yield return Fade(0, 1, 1f); // Fade to black

        currentStepIndex++; // Go to next step
        currentSentenceIndex = 0; // Reset sentence index

        if (currentStepIndex < cutsceneSteps.Count)
        {
            cutsceneImage.sprite = cutsceneSteps[currentStepIndex].image; // Show next image
            yield return new WaitForSeconds(1f);
            yield return Fade(1, 0, 1f); // Fade back in
            yield return new WaitForSeconds(1f);
            ShowSentence(); // Show first sentence of next step
        }
        else
        {
            yield return new WaitForSeconds(1f); // End of cutscene
            LoadingManager.SceneToLoad = "Level_1"; // Set next scene
            SceneManager.LoadScene("Loading"); // Load it
        }

        waitingBetweenSteps = false;
    }

    /// <summary>
    /// Fades the alpha of the cutscene canvas group over time.
    /// </summary>
    IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            fadeGroup.alpha = Mathf.Lerp(from, to, timer / duration); // Smooth fade
            timer += Time.deltaTime;
            yield return null;
        }

        fadeGroup.alpha = to; // Ensure final value
    }

    /// <summary>
    /// Continuously blinks the skip text by fading its alpha.
    /// </summary>
    IEnumerator BlinkSkipText()
    {
        while (true)
        {
            if (skipCanvasGroup != null)
            {
                skipCanvasGroup.alpha = 1f; // Fully visible
                yield return new WaitForSeconds(0.5f);
                skipCanvasGroup.alpha = 0.3f; // Dimmed
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield break; // Exit if CanvasGroup is missing
            }
        }
    }
}
