using UnityEngine;

public class DanceNoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] notePrefabs; // Up, Down, Left, Right prefabs
    [SerializeField] private Transform[] spawnPoints;  // Where notes appear
    [SerializeField] private float songBpm = 120f; // Set this to your song's BPM
    [SerializeField] private float songOffset = 0f; // If your song has a delay before the first beat

    private float nextBeatTime = 0f;
    private float songTime = 0f;
    private AudioSource musicSource;

    private void Start()
    {
        nextBeatTime = songOffset;
        songTime = 0f;
    }

    private void Update()
    {
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
            nextBeatTime += beatInterval;
        }
    }

    private void SpawnRandomNote()
    {
        int rand = Random.Range(0, notePrefabs.Length);
        Instantiate(notePrefabs[rand], spawnPoints[rand].position, Quaternion.identity);
    }
}