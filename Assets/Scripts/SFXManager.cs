using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    public AudioSource audioSource;
    public AudioClip buttonClick;
    public AudioClip tokenEarned;
    public AudioClip tokenSpent;
    public AudioClip miniGameStart;
    public AudioClip miniGameEnd;
    public AudioClip error;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        if (!audioSource) audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    public void Play(AudioClip clip, float volume = 1f)
    {
        if (clip && audioSource) audioSource.PlayOneShot(clip, volume);
    }

    public void PlayButton() => Play(buttonClick);
    public void PlayTokenEarned() => Play(tokenEarned);
    public void PlayTokenSpent() => Play(tokenSpent);
    public void PlayMiniGameStart() => Play(miniGameStart);
    public void PlayMiniGameEnd() => Play(miniGameEnd);
    public void PlayError() => Play(error);
}