using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance { get; private set; }

    [Header("Root")]
    public GameObject gameOverPanel; // assign UI/GameOverPanel (or auto-found)

    [Header("Optional Children (auto-found by name)")]
    public TextMeshProUGUI causeOfDeathText;  // child named "CauseOfDeath"
    public TextMeshProUGUI lifespanText;      // child named "Lifespan"
    public TextMeshProUGUI statsText;         // child named "Stats"
    public TextMeshProUGUI bestStatsText;     // child named "Best Stats"
    public Button restartButton;              // child named "Restart"

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // Try to auto-find the panel if not assigned
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        // Make sure it blocks clicks behind it
        if (gameOverPanel != null)
        {
            var cg = gameOverPanel.GetComponent<CanvasGroup>() ?? gameOverPanel.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            cg.interactable = true;
            cg.alpha = 1f;

            AutoWireChildren();
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[GameOverUI] GameOverPanel not found. Create UI/GameOverPanel");
        }
    }

    void AutoWireChildren()
    {
        // Safe find helper
        T FindChild<T>(string name) where T : Component
        {
            if (gameOverPanel == null) return null;
            var t = gameOverPanel.transform.Find(name);
            return t ? t.GetComponent<T>() : null;
        }

        if (causeOfDeathText == null) causeOfDeathText = FindChild<TextMeshProUGUI>("CauseOfDeath");
        if (lifespanText     == null) lifespanText     = FindChild<TextMeshProUGUI>("Lifespan");
        if (statsText        == null) statsText        = FindChild<TextMeshProUGUI>("Stats");
        if (bestStatsText    == null) bestStatsText    = FindChild<TextMeshProUGUI>("Best Stats");
        if (restartButton    == null) restartButton    = FindChild<Button>("Restart");

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    public void ShowGameOver()
    {
        if (gameOverPanel == null)
        {
            Debug.LogError("[GameOverUI] gameOverPanel not assigned");
            return;
        }

        AutoWireChildren();
        gameOverPanel.SetActive(true);

        var gm = GameManager.Instance;
        if (gm == null) { Debug.LogError("[GameOverUI] GameManager.Instance is null"); return; }

        UpdateStatsUI(gm);

        // gentle pop if LeanTween is present
        try { LeanTween.scale(gameOverPanel, Vector3.one * 1.05f, 0.4f).setEasePunch(); } catch {}
    }

    void UpdateStatsUI(GameManager gm)
    {
        var stats = gm.currentStats;
        var best  = gm.bestStats;

        int days  = Mathf.FloorToInt(stats.lifespanHours / 24f);
        int hours = Mathf.FloorToInt(stats.lifespanHours % 24f);

        if (causeOfDeathText) causeOfDeathText.text = $"Your duck died from {stats.causeOfDeath}!";
        if (lifespanText)     lifespanText.text     = $"Lifespan -> {days} days, {hours} hours";
        if (statsText)        statsText.text        = $"Fed -> {stats.timesFed}\nPlayed -> {stats.timesPlayed}\nSlept -> {stats.timesSlept}";
        if (bestStatsText)    bestStatsText.text    = $"Best -> {Mathf.FloorToInt(best.lifespanHours/24f)}d {Mathf.FloorToInt(best.lifespanHours%24f)}h";
    }

    void OnRestartClicked()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance?.RestartGame();
    }
}