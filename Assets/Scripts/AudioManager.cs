using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource effectsSource;
    public AudioClip[] audioClips;

    public AudioSource musicSource;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void PlayEffect(int index)
    {
        effectsSource.clip = audioClips[index];
        effectsSource.Play();
    }

    public void PlayEffect(string effectName) {
        AudioClip clip = null;
        foreach (AudioClip audioClip in audioClips)
        {
            if (audioClip.name == effectName)
            {
                clip = audioClip;
                break;
            }
        }

        if (clip == null)
        {
            Debug.LogError("Audio clip not found: " + effectName);
            return;
        }

        effectsSource.clip = clip;
        effectsSource.Play();
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
}
