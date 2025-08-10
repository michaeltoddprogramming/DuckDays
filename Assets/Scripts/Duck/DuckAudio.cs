using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DuckAudio : MonoBehaviour
{
    public AudioClip[] quackSounds;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

        void Start()
    {
        // Preload by playing at 0 volume to warm up the audio
        if (quackSounds != null && quackSounds.Length > 0)
            audioSource.PlayOneShot(quackSounds[0], 0f); // Silent
    }


    public void PlayRandomQuack()
    {
        if (quackSounds == null || quackSounds.Length == 0) return;

        int index = Random.Range(0, quackSounds.Length);
        if (quackSounds[index])
        {
            audioSource.PlayOneShot(quackSounds[index]);
        }
    }
}
