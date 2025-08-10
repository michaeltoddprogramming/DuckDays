using UnityEngine;

public class DanceNote : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject missEffectPrefab;
    [SerializeField] private GameObject perfectEffectPrefab;

    // optional to set in inspector, otherwise auto-found/created
    [SerializeField] private RectTransform effectsParent;

    private bool canBeHit = false;
    private DanceGameManager gameManager;
    private float hitZoneY;

    private void Start()
    {
        gameManager = FindObjectOfType<DanceGameManager>();

        // find hit zone Y
        var hitZoneObj = GameObject.FindWithTag("HitZone");
        if (hitZoneObj != null)
            hitZoneY = hitZoneObj.transform.position.y;

        // Auto find or create an EffectsParent under the first Canvas
        if (effectsParent == null)
        {
            var existing = GameObject.Find("EffectsParent");
            if (existing != null)
                effectsParent = existing.GetComponent<RectTransform>();
            else
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    var go = new GameObject("EffectsParent", typeof(RectTransform));
                    go.transform.SetParent(canvas.transform, false);
                    effectsParent = go.GetComponent<RectTransform>();
                }
            }
        }
    }

    private void Update()
    {
        transform.Translate(Vector3.down * speed * Time.deltaTime);
    }

    public void TryHit()
    {
        if (canBeHit)
        {
            float timingError = Mathf.Abs(transform.position.y - hitZoneY);

            if (timingError < 0.05f)
            {
                SpawnEffect(perfectEffectPrefab);
                gameManager.AddScore(300);
            }
            else
            {
                SpawnEffect(hitEffectPrefab);
                gameManager.AddScore(100);
            }

            Destroy(gameObject);
        }
        else
        {
            SpawnEffect(missEffectPrefab);
        }
    }

    private void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab == null)
            return;

        if (effectsParent != null)
        {
            // instantiate then parent under the canvas effects container
            GameObject effect = Instantiate(effectPrefab);
            effect.transform.SetParent(effectsParent, false); // worldPositionStays = false

            // convert this note's world position to canvas-local position
            Camera cam = Camera.main;
            Vector2 localPoint;
            Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                effectsParent, screenPos, cam, out localPoint);

            var rt = effect.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = localPoint;
            else
                effect.transform.localPosition = (Vector3)localPoint;
        }
        else
        {
            // fallback: spawn in world space
            Instantiate(effectPrefab, transform.position, Quaternion.identity);
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
