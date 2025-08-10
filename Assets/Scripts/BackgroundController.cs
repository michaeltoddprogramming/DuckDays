using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    public static BackgroundController Instance { get; private set; }

    public Sprite morningSprite;
    public Sprite daySprite;
    public Sprite eveningSprite;
    public Sprite nightSprite;

    [Range(0f, 24f)]
    public float currentTime = 12f;
    public float timeSpeed = 1f;

    public float morningStart = 6f;
    public float dayStart = 10f;
    public float eveningStart = 18f;
    public float nightStart = 22f;

    public int daysPassed = 0;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        // Singleton pattern to persist time across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateBackground();
    }

    void Update()
    {
        // Stop advancing time while a mini-game is active
        if (GameManager.Instance != null && GameManager.Instance.isMiniGameActive)
            return;

        float prevTime = currentTime;
        currentTime += Time.deltaTime * timeSpeed / 3600f;

        if (currentTime >= 24f)
        {
            currentTime = 0f;
            daysPassed++;
        }

        UpdateBackground();
    }

    void UpdateBackground()
    {
        if (!spriteRenderer)
            spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetSpriteForTime(currentTime);
    }

    Sprite GetSpriteForTime(float time)
    {
        if (time >= morningStart && time < dayStart) return morningSprite;
        if (time >= dayStart && time < eveningStart) return daySprite;
        if (time >= eveningStart && time < nightStart) return eveningSprite;
        return nightSprite;
    }

    public void SetTime(float newTime)
    {
        currentTime = Mathf.Clamp(newTime, 0f, 24f);
        UpdateBackground();
    }

    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60);
        return $"{hours:00}:{minutes:00}";
    }
}