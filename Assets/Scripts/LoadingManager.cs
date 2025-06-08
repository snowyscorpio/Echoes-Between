using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public static string SceneToLoad;

    public TMP_Text loadingLabelText;     // Shows "Loading..."
    public TMP_Text loadingPercentText;   // Shows loading progress
    public TMP_Text tipText;              // Random tip
    public Slider progressBar;            // Loading progress bar

    private string[] tips = {
        "TIP: DON'T FORGET TO SAVE, BE RESPONSIBLE",
        "TIP: BE PATIENT, DON'T RUSH",
        "TIP: DON'T FORGET TO DRINK WATER (IN REAL LIFE)",
        "TIP: IF YOU GO BACK OR TO OPTIONS WITHOUT SAVING YOU WILL LOSE YOUR PROGRESS"
    };

    private void Start()
    {
        ShowRandomTip();
        StartCoroutine(LoadSceneAsync());
    }

    void ShowRandomTip()
    {
        int index = Random.Range(0, tips.Length);
        tipText.text = tips[index];
    }

    IEnumerator LoadSceneAsync()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneToLoad);
        operation.allowSceneActivation = false;

        float timer = 0f;

        while (!operation.isDone)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            int percent = Mathf.RoundToInt(progress * 100);

            if (loadingLabelText != null)
                loadingLabelText.text = "LOADING...";
            if (loadingPercentText != null)
                loadingPercentText.text = percent + "%";
            if (progressBar != null)
                progressBar.value = progress;

            if (operation.progress >= 0.9f && timer >= 8f)
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
