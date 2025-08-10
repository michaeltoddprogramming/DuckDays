using UnityEngine;

public class DanceNoteSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] notePrefabs; // Up, Down, Left, Right prefabs
    [SerializeField] private Transform[] spawnPoints;  // Where notes appear
    [SerializeField] private float spawnInterval = 1f;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnRandomNote();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnRandomNote()
    {
        int rand = Random.Range(0, notePrefabs.Length);
        Instantiate(notePrefabs[rand], spawnPoints[rand].position, Quaternion.identity);
    }
}
