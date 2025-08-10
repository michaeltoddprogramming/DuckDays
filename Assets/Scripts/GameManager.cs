using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // Scene names (must match the .unity file names in Assets/Scenes)
    public const string MainSceneName = "Main";
    public const string FeedingSceneName = "FeedingMiniGame";

    public static GameManager Instance { get; private set; }
    
    // Game state
    public bool isGameOver = false;

    // Mini-game state (NEW)
    public bool isMiniGameActive { get; private set; } = false;
    public event Action<bool> MiniGameStateChanged;

    // Duck stats tracking
    [System.Serializable]
    public class DuckStats
    {
        public float lifespanHours = 0f;
        public int timesFed = 0;
        public int timesPlayed = 0;
        public int timesSlept = 0;
        public float maxHunger = 0f;
        public float maxHappiness = 0f; 
        public float maxEnergy = 0f;
        public string causeOfDeath = "Unknown";
    }
    
    public DuckStats currentStats = new DuckStats();
    public DuckStats bestStats = new DuckStats();
    
    private DateTime startTime;
    
    // Add these fields to track starting time
    private float startingGameHour = 0f;
    private int startingGameDay = 0;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBestStats(); // Add this line to load on startup
        }
        else
        {
            Destroy(gameObject);
        }
        
        startTime = DateTime.Now;
    }
    
    // Add these methods for saving/loading stats
    
    void OnApplicationQuit()
    {
        SaveBestStats(); // Save when game is closed
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveBestStats(); // Save when game is paused (for mobile)
    }
    
    void SaveBestStats()
    {
        PlayerPrefs.SetFloat("BestLifespan", bestStats.lifespanHours);
        PlayerPrefs.SetInt("BestTimesFed", bestStats.timesFed);
        PlayerPrefs.SetInt("BestTimesPlayed", bestStats.timesPlayed);
        PlayerPrefs.SetInt("BestTimesSlept", bestStats.timesSlept);
        PlayerPrefs.SetFloat("BestMaxHunger", bestStats.maxHunger);
        PlayerPrefs.SetFloat("BestMaxHappiness", bestStats.maxHappiness);
        PlayerPrefs.SetFloat("BestMaxEnergy", bestStats.maxEnergy);
        PlayerPrefs.Save();
        
        Debug.Log("Best stats saved!");
    }
    
    void LoadBestStats()
    {
        bestStats.lifespanHours = PlayerPrefs.GetFloat("BestLifespan", 0);
        bestStats.timesFed = PlayerPrefs.GetInt("BestTimesFed", 0);
        bestStats.timesPlayed = PlayerPrefs.GetInt("BestTimesPlayed", 0);
        bestStats.timesSlept = PlayerPrefs.GetInt("BestTimesSlept", 0);
        bestStats.maxHunger = PlayerPrefs.GetFloat("BestMaxHunger", 0);
        bestStats.maxHappiness = PlayerPrefs.GetFloat("BestMaxHappiness", 0);
        bestStats.maxEnergy = PlayerPrefs.GetFloat("BestMaxEnergy", 0);
        
        Debug.Log("Best stats loaded!");
    }
    
    public void StartGame()
    {
        isGameOver = false;
        currentStats = new DuckStats();
        startTime = DateTime.Now;
        
        // Track the starting time to calculate accurate lifespan
        if (BackgroundController.Instance != null)
        {
            startingGameHour = BackgroundController.Instance.currentTime;
            startingGameDay = BackgroundController.Instance.daysPassed;
        }
    }
    
    // Also save when game ends
    public void EndGame(string causeOfDeath)
    {
        if (isGameOver) return;

        isGameOver = true;
        // make sure mini-games and world are paused/closed
        SetMiniGameActive(false);

        // record cause + final lifespan snapshot
        currentStats.causeOfDeath = causeOfDeath;
        var bg = FindFirstObjectByType<BackgroundController>();
        if (bg != null)
            UpdateLifespan(bg.currentTime, bg.daysPassed);

        SaveBestStats();

        Debug.Log("[GameManager] Game over → showing UI");
        GameOverUI.Instance?.ShowGameOver();
    }
    
    public void SetMiniGameActive(bool active)
    {
        if (isMiniGameActive == active) return;
        isMiniGameActive = active;
        MiniGameStateChanged?.Invoke(active);
    }

    public void RestartGame()
    {
        // Reset game state before loading the scene
        isGameOver = false;
        currentStats = new DuckStats();
        isMiniGameActive = false; // reset mini-game state
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void TrackFed()
    {
        currentStats.timesFed++;
    }
    
    public void TrackPlayed()
    {
        currentStats.timesPlayed++;
    }
    
    public void TrackSlept()
    {
        currentStats.timesSlept++;
    }
    
    public void UpdateMaxStats(float hunger, float happiness, float energy)
    {
        currentStats.maxHunger = Mathf.Max(currentStats.maxHunger, hunger);
        currentStats.maxHappiness = Mathf.Max(currentStats.maxHappiness, happiness);
        currentStats.maxEnergy = Mathf.Max(currentStats.maxEnergy, energy);
    }
    
    public void UpdateLifespan(float currentGameHour, int currentGameDay)
    {
        // Calculate actual hours survived
        float totalStartHours = startingGameHour + (startingGameDay * 24f);
        float totalCurrentHours = currentGameHour + (currentGameDay * 24f);
        float actualHoursSurvived = totalCurrentHours - totalStartHours;
        
        // Make sure we never get negative hours
        currentStats.lifespanHours = Mathf.Max(0f, actualHoursSurvived);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only (re)initialize gameplay when returning to the Main scene
        isMiniGameActive = (scene.name != MainSceneName);
        if (scene.name == MainSceneName)
        {
            // Resume main game without wiping stats
            StartGame();
        }
    }

    // Open/close the Feeding mini-game scene
    public void LoadFeedingMiniGameScene()
    {
        isMiniGameActive = true;
        SceneManager.LoadScene(FeedingSceneName, LoadSceneMode.Single);
    }

    public void ReturnToMainScene()
    {
        isMiniGameActive = false;
        SceneManager.LoadScene(MainSceneName, LoadSceneMode.Single);
    }
}