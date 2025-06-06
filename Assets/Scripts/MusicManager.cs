using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;

    private AudioClip menuMusic;
    private AudioClip sessionMusic;

    private string[] sessionScenes = { "Level_1", "Level_2", "Level_3", "LastLevel", "Cutscene", "Loading" };

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        menuMusic = Resources.Load<AudioClip>("Audio/MenuMusic");
        sessionMusic = Resources.Load<AudioClip>("Audio/SessionMusic");

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMenuMusic(); // מוזיקה בהתחלה
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (IsSessionScene(scene.name))
        {
            PlaySessionMusic();
        }
        else
        {
            PlayMenuMusic();
        }
    }

    bool IsSessionScene(string sceneName)
    {
        foreach (string name in sessionScenes)
        {
            if (sceneName.Contains(name))
                return true;
        }
        return false;
    }

    void PlayMenuMusic()
    {
        if (audioSource.clip != menuMusic)
        {
            audioSource.clip = menuMusic;
            audioSource.Play();
        }
    }

    void PlaySessionMusic()
    {
        if (audioSource.clip != sessionMusic)
        {
            audioSource.clip = sessionMusic;
            audioSource.Play();
        }
    }
}
