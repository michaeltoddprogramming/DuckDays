using UnityEngine;

public class HitZone : MonoBehaviour
{
    [SerializeField] private KeyCode inputKey; 
    [SerializeField] private float perfectWindow = 0.1f; 
    [SerializeField] private float goodWindow = 0.25f; 

    private DanceNote noteInZone;

    void Update()
    {
        if (noteInZone != null && Input.GetKeyDown(inputKey))
        {
            // Let DanceNote handle the hit logic and scoring
            noteInZone.TryHit();
            noteInZone = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<DanceNote>() != null)
        {
            noteInZone = collision.GetComponent<DanceNote>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<DanceNote>() == noteInZone)
        {
            noteInZone = null;
        }
    }
}