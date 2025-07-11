using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

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

        // Connect to AudioMixer
        AudioMixer mixer = Resources.Load<AudioMixer>("MainAudioMixer");
        if (mixer != null)
        {
            AudioMixerGroup[] groups = mixer.FindMatchingGroups("Master");
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

        menuMusic = Resources.Load<AudioClip>("Audio/MenuMusic");
        sessionMusic = Resources.Load<AudioClip>("Audio/SessionMusic");

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMenuMusic(); 
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
