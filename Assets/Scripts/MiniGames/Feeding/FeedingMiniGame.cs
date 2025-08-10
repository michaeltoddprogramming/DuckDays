using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FeedingMiniGame : MonoBehaviour
{
    [Header("Game Elements")]
    public GameObject duckPlayer;       // Sprite with Rigidbody2D (Kinematic) + Collider2D (isTrigger), Tag=Player
    public Transform spawnArea;         // Empty parent for spawned food
    public float gameAreaWidth = 8f;

    [Header("Food Prefabs")]
    public GameObject goodFoodPrefab;   // SpriteRenderer + Collider2D (isTrigger)
    public GameObject badFoodPrefab;

    [Header("UI")]
    public Slider timeSlider;
    public TextMeshProUGUI scoreText;
    public Button quitButton;           // Returns to Main

    [Header("Game Settings")]
    public float gameDuration = 20f;
    public float spawnRate = 1f;
    public float duckSpeed = 10f;

    [Header("Audio")]
    public AudioClip collectGoodSfx;
    public AudioClip collectBadSfx;
    public AudioClip walkSfx;
    public AudioClip backgroundMusic; // Add this line

    private AudioSource audioSource;
    private AudioSource musicSource; // Add this line
    private Animator duckAnimator;
    private SpriteRenderer duckSpriteRenderer;
    private float walkSfxCooldown = 0f;

    int score = 0;
    float timeRemaining;
    bool isPlaying = false;
    DuckController mainDuck; // from Main scene (not present here)

    Coroutine spawnRoutine;

    void Start()
    {
        // We don’t need GameManager.SetMiniGameActive here; we’re in a separate scene.
        mainDuck = FindFirstObjectByType<DuckController>(); // will be null in this scene, that’s fine

        if (quitButton) quitButton.onClick.AddListener(EndGame);

        // Initialize and go
        score = 0;
        timeRemaining = gameDuration;
        isPlaying = true;

        if (timeSlider) { timeSlider.maxValue = gameDuration; timeSlider.value = gameDuration; }
        UpdateScoreUI();

        if (duckPlayer) duckPlayer.transform.localPosition = new Vector3(0, -2.5f, 0); // was -3.5f

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnFood());

        if (duckPlayer)
        {
            duckAnimator = duckPlayer.GetComponent<Animator>();
            duckSpriteRenderer = duckPlayer.GetComponent<SpriteRenderer>();
            audioSource = duckPlayer.GetComponent<AudioSource>();
            if (!audioSource) audioSource = duckPlayer.AddComponent<AudioSource>();
        }

        // --- Background music setup ---
        if (backgroundMusic)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = 0.5f;
            musicSource.Play();
        }
    }

    void Update()
    {
        if (!isPlaying) return;
        timeRemaining -= Time.deltaTime;
        if (timeSlider) timeSlider.value = timeRemaining;
        if (timeRemaining <= 0f) { EndGame(); return; }

        if (duckPlayer)
        {
            float moveX = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) moveX = -1;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) moveX = 1;

            Vector3 p = duckPlayer.transform.localPosition;
            p.x = Mathf.Clamp(p.x + moveX * duckSpeed * Time.deltaTime, -gameAreaWidth * 0.5f, gameAreaWidth * 0.5f);
            duckPlayer.transform.localPosition = p;

            // Animation and facing
            if (duckAnimator)
            {
                duckAnimator.SetBool("isWalking", moveX != 0f);
                duckAnimator.SetFloat("direction", moveX);
            }
            if (duckSpriteRenderer && moveX != 0f)
            {
                duckSpriteRenderer.flipX = moveX > 0f;
            }

            // Walk SFX (play every 0.4s while moving)
            if (moveX != 0f && walkSfx && audioSource)
            {
                walkSfxCooldown -= Time.deltaTime;
                if (walkSfxCooldown <= 0f)
                {
                    audioSource.PlayOneShot(walkSfx, 0.3f);
                    walkSfxCooldown = 0.4f;
                }
            }
            else
            {
                walkSfxCooldown = 0f;
            }
        }
    }

    IEnumerator SpawnFood()
    {
        float topY = 3.5f;
        float bottomY = -3.5f;

        while (isPlaying)
        {
            float x = Random.Range(-gameAreaWidth * 0.5f, gameAreaWidth * 0.5f);
            Vector3 spawnPos = new Vector3(x, topY, 0);

            GameObject prefab = Random.value < 0.5f ? goodFoodPrefab : badFoodPrefab;
            GameObject food = Instantiate(prefab, spawnPos, Quaternion.identity, spawnArea); // world position

            if (!food.TryGetComponent<Collider2D>(out var col)) col = food.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            var item = food.GetComponent<FoodItem>() ?? food.AddComponent<FoodItem>();
            item.game = this;
            item.isBad = (prefab == badFoodPrefab);

            LeanTween.moveY(food, bottomY, 2.0f).setOnComplete(() => { if (food) Destroy(food); });

            yield return new WaitForSeconds(1f / spawnRate);
        }
    }

    public void CollectFood(bool isBad)
    {
        score += isBad ? -5 : 10;
        UpdateScoreUI();

        // SFX
        if (audioSource)
        {
            if (isBad && collectBadSfx) audioSource.PlayOneShot(collectBadSfx, 0.7f);
            else if (!isBad && collectGoodSfx) audioSource.PlayOneShot(collectGoodSfx, 0.7f);
        }

        if (scoreText != null)
        {
            LeanTween.cancel(scoreText.gameObject);
            LeanTween.scale(scoreText.gameObject, Vector3.one * 1.2f, 0.2f).setEasePunch();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"{score}";
    }

    public void EndGame()
    {
        if (!isPlaying) { GameManager.Instance?.ReturnToMainScene(); return; }
        isPlaying = false;

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        // Award FEED tokens (1 per 50 score)
        int tokens = 0;
        if (TokenManager.Instance != null)
        {
            tokens = Mathf.Max(0, Mathf.FloorToInt(score / 50f));
            if (tokens > 0) TokenManager.Instance.AddTokens("feed", tokens);
        }

        // Clean children
        if (spawnArea != null)
        {
            foreach (Transform t in spawnArea) Destroy(t.gameObject);
        }

        // Stop background music
        if (musicSource) musicSource.Stop();

        // Return to main scene
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainScene();

        // Show result in UI when back in main scene
        StartCoroutine(ShowResultAfterReturn(tokens));
    }

    private IEnumerator ShowResultAfterReturn(int tokens)
    {
        yield return new WaitForSeconds(0.5f); // Wait for scene to load
        var ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.ShowFeedingMiniGameResult(tokens);
    }
}