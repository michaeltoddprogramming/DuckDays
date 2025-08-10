using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip mainClip; // Use one music file

    public DuckController duck;

    void Start()
    {
        if (!duck) duck = FindFirstObjectByType<DuckController>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        audioSource.clip = mainClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        UpdateMusicMood();
    }

    void UpdateMusicMood()
    {
        if (!duck || !audioSource) return;

        float avg = (duck.hunger.value + duck.happiness.value + duck.energy.value + duck.health.value) / 4f;

        // Default values
        float targetPitch = 1f;
        float targetVolume = 0.7f;

        if (avg > 70f)
        {
            targetPitch = 1.1f; // Happy: slightly faster
            targetVolume = 1f;
        }
        else if (avg > 30f)
        {
            targetPitch = 1f; // Neutral: normal
            targetVolume = 0.7f;
        }
        else
        {
            targetPitch = 0.85f; // Sad: slower, softer
            targetVolume = 0.5f;
        }

        // Smoothly interpolate for a nice effect
        audioSource.pitch = Mathf.Lerp(audioSource.pitch, targetPitch, Time.deltaTime * 2f);
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, Time.deltaTime * 2f);
    }
}