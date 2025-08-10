using UnityEngine;

public class DanceNote : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    private bool canBeHit = false;
    private DanceGameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<DanceGameManager>();
    }

    private void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    public void TryHit()
    {
        if (canBeHit)
        {
            gameManager.AddScore(100);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("HitZone"))
            canBeHit = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("HitZone"))
            canBeHit = false;
    }
}
