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

        if (duckPlayer) duckPlayer.transform.localPosition = new Vector3(0, -3.5f, 0);

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        spawnRoutine = StartCoroutine(SpawnFood());
    }

    void Update()
    {
        if (!isPlaying) return;

        timeRemaining -= Time.deltaTime;
        if (timeSlider) timeSlider.value = timeRemaining;
        if (timeRemaining <= 0f) { EndGame(); return; }

        // Horizontal input (keyboard or A/D). For touch, move toward finger x.
        if (duckPlayer)
        {
            float moveX = 0f;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) moveX = -1;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) moveX = 1;

            Vector3 p = duckPlayer.transform.localPosition;
            p.x = Mathf.Clamp(p.x + moveX * duckSpeed * Time.deltaTime, -gameAreaWidth * 0.5f, gameAreaWidth * 0.5f);
            duckPlayer.transform.localPosition = p;

            if (moveX != 0f)
            {
                var s = duckPlayer.transform.localScale;
                s.x = Mathf.Abs(s.x) * Mathf.Sign(moveX);
                duckPlayer.transform.localScale = s;
            }
        }
    }

    IEnumerator SpawnFood()
    {
        var cam = Camera.main;
        float topY = cam ? cam.orthographicSize - 0.5f : 5f;
        float bottomY = cam ? -cam.orthographicSize + 0.5f : -5f;

        while (isPlaying)
        {
            float x = Random.Range(-gameAreaWidth * 0.5f, gameAreaWidth * 0.5f);
            Vector3 spawnPos = new Vector3(x, topY, 0);

            GameObject prefab = Random.value < 0.8f ? goodFoodPrefab : badFoodPrefab;
            GameObject food = Instantiate(prefab, spawnArea);
            food.transform.localPosition = spawnPos;

            if (!food.TryGetComponent<Collider2D>(out var col)) col = food.AddComponent<BoxCollider2D>();
            col.isTrigger = true;

            var item = food.GetComponent<FoodItem>() ?? food.AddComponent<FoodItem>();
            item.game = this;
            item.isBad = (prefab == badFoodPrefab);

            LeanTween.moveLocalY(food, bottomY, 2.0f).setOnComplete(() => { if (food) Destroy(food); });

            yield return new WaitForSeconds(1f / spawnRate);
        }
    }

    public void CollectFood(bool isBad)
    {
        score += isBad ? -5 : 10;
        UpdateScoreUI();

        if (scoreText != null)
        {
            LeanTween.cancel(scoreText.gameObject);
            LeanTween.scale(scoreText.gameObject, Vector3.one * 1.2f, 0.2f).setEasePunch();
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText) scoreText.text = $"Score: {score}";
    }

    public void EndGame()
    {
        if (!isPlaying) { GameManager.Instance?.ReturnToMainScene(); return; }
        isPlaying = false;

        if (spawnRoutine != null) StopCoroutine(spawnRoutine);

        // Award FEED tokens (1 per 50 score)
        if (TokenManager.Instance != null)
        {
            int tokens = Mathf.Max(0, Mathf.FloorToInt(score / 50f));
            if (tokens > 0) TokenManager.Instance.AddTokens("feed", tokens);
        }

        // Clean children
        if (spawnArea != null)
        {
            foreach (Transform t in spawnArea) Destroy(t.gameObject);
        }

        // Go back to Main
        GameManager.Instance?.ReturnToMainScene();
    }
}