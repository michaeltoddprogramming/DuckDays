using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DanceGameManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private TextMeshProUGUI resultsScoreText;

    [Header("Settings")]
    [SerializeField] private float gameDuration = 30f;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    private AudioSource musicSource;

    private int score;
    private float timer;
    private bool isPlaying;

    public AudioSource MusicSource => musicSource;
    public int Score => score;
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        timer = gameDuration;
        score = 0;
        isPlaying = true;
        resultsPanel.SetActive(false);

        // Play background music
        if (backgroundMusic)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.clip = backgroundMusic;
            musicSource.loop = false;
            musicSource.Play();
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            EndGame();
        }

        timerText.text = Mathf.Ceil(timer).ToString();
    }

    public void AddScore(int points)
    {
    score += points;
    scoreText.text = score.ToString();

    if (scoreText != null)
    {
        LeanTween.cancel(scoreText.gameObject);
        scoreText.transform.localScale = Vector3.one; // reset
        LeanTween.scale(scoreText.rectTransform, Vector3.one * 1.15f, 0.15f)
            .setEasePunch();
    }
    }


    private void EndGame()
    {
        isPlaying = false;
        resultsPanel.SetActive(true);
        resultsScoreText.text = "Final Score: " + score;

        // Award PLAY tokens (1 per 500 points, for example)
        int tokens = Mathf.Max(0, score / 500);
        if (TokenManager.Instance != null && tokens > 0)
            TokenManager.Instance.AddTokens("play", tokens);

        // Return to main scene after a short delay
        StartCoroutine(ReturnToMainSceneWithResult(tokens));
    }

    private IEnumerator ReturnToMainSceneWithResult(int tokens)
    {
        yield return new WaitForSeconds(2f);
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainScene();

        // Show result in UI when back in main scene
        yield return new WaitForSeconds(0.5f);
        var ui = FindFirstObjectByType<UIManager>();
        if (ui != null)
            ui.UpdateStatusText(tokens > 0
                ? $"You earned {tokens} play token{(tokens > 1 ? "s" : "")}! Use tokens to play with your duck."
                : "No play tokens earned. Try again!");
    }

    private void ExampleMethod()
    {
        var danceGameManager = FindObjectOfType<DanceGameManager>();
        if (danceGameManager != null)
        {
            int score = danceGameManager.Score;
            // use score as needed
        }
    }
}
