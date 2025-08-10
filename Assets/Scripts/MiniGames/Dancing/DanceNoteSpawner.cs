using UnityEngine;

public class DanceNoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] notePrefabs; // Up, Down, Left, Right prefabs
    [SerializeField] private Transform[] spawnPoints;  // Where notes appear
    [SerializeField] private float songBpm = 120f; // Set this to your song's BPM
    [SerializeField] private float songOffset = 0f; // If your song has a delay before the first beat

    [Header("Vibe Effects")]
    [SerializeField] private Camera mainCamera; // Assign your main camera in Inspector
    [SerializeField] private Color beatColor = new Color(0.2f, 0.2f, 0.5f);
    [SerializeField] private Color baseColor = Color.black;

    private float nextBeatTime = 0f;
    private float songTime = 0f;
    private AudioSource musicSource;
    private DanceGameManager danceGameManager;

    private void Start()
    {
        nextBeatTime = songOffset;
        songTime = 0f;
        danceGameManager = FindObjectOfType<DanceGameManager>();
        // Remove or comment out the following lines to prevent movement:
        // Vector3 baseScale = transform.localScale;
        // LeanTween.scale(gameObject, baseScale * 1.25f, 0.8f)
        //     .setEaseInOutSine()
        //     .setLoopPingPong();
    }

    private void Update()
    {
        if (danceGameManager != null && !danceGameManager.IsPlaying)
            return;

        // Find the musicSource if not set
        if (musicSource == null)
        {
            var manager = FindObjectOfType<DanceGameManager>();
            if (manager != null)
                musicSource = manager.MusicSource;
            if (musicSource == null)
                return;
        }

        if (!musicSource.isPlaying)
            return;

        songTime = musicSource.time;

        float beatInterval = 60f / songBpm;
            while (songTime >= nextBeatTime)
    {
        SpawnRandomNote();

        // Camera + background pulse
        if (mainCamera != null)
        {
            LeanTween.cancel(mainCamera.gameObject);
            LeanTween.value(mainCamera.gameObject, baseColor, beatColor, 0.1f)
                .setOnUpdate((Color c) => mainCamera.backgroundColor = c)
                .setEaseInOutSine()
                .setOnComplete(() =>
                    LeanTween.value(mainCamera.gameObject, beatColor, baseColor, 0.25f)
                        .setOnUpdate((Color c) => mainCamera.backgroundColor = c)
                        .setEaseInOutSine()
                );
        }

        nextBeatTime += beatInterval;
    }
    }

    private void SpawnRandomNote()
    {
        int rand = Random.Range(0, notePrefabs.Length);
        GameObject note = Instantiate(notePrefabs[rand], spawnPoints[rand].position, Quaternion.identity);
        float arrowScale = 0.25f;
        note.transform.localScale = Vector3.one * arrowScale * 0.7f;
        LeanTween.scale(note, Vector3.one * arrowScale, 0.18f).setEaseOutBack();
    }

    // --- Example hooks for other effects ---
    // Call this from your score system when score increases
    public void PunchScore(TMPro.TextMeshProUGUI scoreText)
    {
        if (scoreText != null)
            LeanTween.scale(scoreText.rectTransform, Vector3.one * 1.2f, 0.15f).setEasePunch();
    }

    // Call this from DanceNote or HitZone on perfect hit
    public void CameraShakeOnPerfect()
    {
        if (mainCamera != null)
            LeanTween.rotateZ(mainCamera.gameObject, Random.Range(-5f, 5f), 0.1f).setEasePunch();
    }

    private void PlayHitSfx()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        AudioClip hitSfx = null; // Assign your AudioClip in the Inspector or load it dynamically

        if (audioSource != null && hitSfx != null)
        {
            float pitch = 1f;
            if (danceGameManager != null)
            {
                // Example: pitch goes from 1.0 to 1.5 as score goes from 0 to 5000
                pitch = 1f + Mathf.Clamp01(danceGameManager.Score / 5000f) * 0.5f;
            }
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(hitSfx, 0.7f);
        }
    }
}