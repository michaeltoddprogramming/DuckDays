using UnityEngine;

public class HitZone : MonoBehaviour
{
    [SerializeField] private KeyCode inputKey; // e.g. W, A, S, D
    [SerializeField] private float hitTolerance = 0.2f; // Time window to hit
    private GameObject arrowInZone;

    void Update()
    {
        if (arrowInZone != null && Input.GetKeyDown(inputKey))
        {
            Arrow arrow = arrowInZone.GetComponent<Arrow>();
            if (Mathf.Abs(arrow.timeToHit) <= hitTolerance)
            {
                arrow.Hit();
            }
            else
            {
                arrow.Miss();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Arrow"))
        {
            arrowInZone = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Arrow") && collision.gameObject == arrowInZone)
        {
            arrowInZone = null;
        }
    }
}
