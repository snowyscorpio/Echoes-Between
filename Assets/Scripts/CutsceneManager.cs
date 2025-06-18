using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneStep
    {
        public Sprite image;
        [TextArea(1, 3)]
        public List<string> sentences;
    }

    public Image cutsceneImage;
    public TMP_Text sentenceText;
    public CanvasGroup fadeGroup;
    public GameObject sentenceBackground;
    public GameObject skipText;

    public List<CutsceneStep> cutsceneSteps;

    public AudioMixer audioMixer;

    private int currentStepIndex = 0;
    private int currentSentenceIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private CanvasGroup skipCanvasGroup;
    private Coroutine blinkCoroutine;

    private bool waitingBetweenSteps = false;

    void Start()
    {
        ApplySavedVolume();

        fadeGroup.alpha = 1;
        sentenceText.text = "";
        if (sentenceBackground != null) sentenceBackground.SetActive(false);
        if (skipText != null)
        {
            skipText.SetActive(false);
            skipCanvasGroup = skipText.GetComponent<CanvasGroup>();
            if (skipCanvasGroup == null)
                skipCanvasGroup = skipText.AddComponent<CanvasGroup>();
        }

        Debug.Log($"Total steps loaded: {cutsceneSteps.Count}");
        StartCoroutine(FadeInAndShowFirst());
    }

    private void ApplySavedVolume()
    {
        var settings = DatabaseManager.Instance?.LoadSettings();
        if (settings.HasValue)
        {
            float volumeValue = Mathf.Clamp01(settings.Value.volume / 100f);
            float db = (volumeValue <= 0.0001f) ? -80f : Mathf.Log10(volumeValue) * 20f;
            audioMixer.SetFloat("Volume", db);
            Debug.Log("Applied volume in CutsceneManager: " + db + " dB");
        }
        else
        {
            Debug.LogWarning("No saved volume found for CutsceneManager.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (waitingBetweenSteps)
            {
                Debug.Log("Ignoring input during pause between steps");
                return;
            }

            if (currentStepIndex >= cutsceneSteps.Count)
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
                StopCoroutine(typingCoroutine);
                sentenceText.text = cutsceneSteps[currentStepIndex].sentences[currentSentenceIndex];
                isTyping = false;
            }
            else if (currentSentenceIndex < cutsceneSteps[currentStepIndex].sentences.Count - 1)
            {
                ShowNextSentence();
            }
            else
            {
                StartCoroutine(TransitionToNextStep());
            }
        }
    }

    IEnumerator FadeInAndShowFirst()
    {
        yield return new WaitForSeconds(1f);
        yield return Fade(1, 0, 1f);
        cutsceneImage.sprite = cutsceneSteps[currentStepIndex].image;
        yield return new WaitForSeconds(1f);
        ShowSentence();
    }

    void ShowSentence()
    {
        Debug.Log($"Showing step {currentStepIndex}, sentence {currentSentenceIndex}");

        if (sentenceBackground != null) sentenceBackground.SetActive(true);
        sentenceText.text = "";

        if (skipText != null)
        {
            skipText.SetActive(true);
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(BlinkSkipText());
        }

        string currentSentence = cutsceneSteps[currentStepIndex].sentences[currentSentenceIndex];
        if (string.IsNullOrEmpty(currentSentence))
        {
            Debug.LogWarning("Empty sentence!");
        }

        typingCoroutine = StartCoroutine(TypeSentence(currentSentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        sentenceText.text = "";
        for (int i = 0; i <= sentence.Length; i++)
        {
            sentenceText.text = sentence.Substring(0, i);
            yield return new WaitForSeconds(0.02f);
        }
        isTyping = false;
    }

    void ShowNextSentence()
    {
        currentSentenceIndex++;

        if (currentSentenceIndex < cutsceneSteps[currentStepIndex].sentences.Count)
        {
            ShowSentence();
        }
        else
        {
            StartCoroutine(TransitionToNextStep());
        }
    }

    IEnumerator TransitionToNextStep()
    {
        waitingBetweenSteps = true;

        yield return new WaitForSeconds(2f);
        if (sentenceBackground != null) sentenceBackground.SetActive(false);
        if (skipText != null) skipText.SetActive(false);
        if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);

        yield return Fade(0, 1, 1f);

        currentStepIndex++;
        currentSentenceIndex = 0;

        if (currentStepIndex < cutsceneSteps.Count)
        {
            cutsceneImage.sprite = cutsceneSteps[currentStepIndex].image;
            yield return new WaitForSeconds(1f);
            yield return Fade(1, 0, 1f);
            yield return new WaitForSeconds(1f);
            ShowSentence();
        }
        else
        {
            yield return new WaitForSeconds(1f);
            LoadingManager.SceneToLoad = "Level_1";
            SceneManager.LoadScene("Loading");
        }

        waitingBetweenSteps = false;
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            fadeGroup.alpha = Mathf.Lerp(from, to, timer / duration);
            timer += Time.deltaTime;
            yield return null;
        }
        fadeGroup.alpha = to;
    }

    IEnumerator BlinkSkipText()
    {
        while (true)
        {
            if (skipCanvasGroup != null)
            {
                skipCanvasGroup.alpha = 1f;
                yield return new WaitForSeconds(0.5f);
                skipCanvasGroup.alpha = 0.3f;
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                yield break;
            }
        }
    }
}