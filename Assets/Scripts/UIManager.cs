using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button feedButton;
    public Button playButton;
    public Button sleepButton;
    public Button simulate1HourButton;
    public Button simulate20HoursButton;
    public Button feedMiniGameButton; // optional separate button

    [Header("Need Bars")]
    public Slider hungerBar;
    public Slider happinessBar;
    public Slider energyBar;
    public Slider healthBar;
    
    [Header("Need Bar Labels")]
    public SpriteRenderer hungerLabel;
    public SpriteRenderer happinessLabel;
    public SpriteRenderer energyLabel;
    public SpriteRenderer healthLabel;
    
    // REMOVED: numeric value texts – bars only now
    // public TextMeshProUGUI hungerValue;
    // public TextMeshProUGUI happinessValue;
    // public TextMeshProUGUI energyValue;
    // public TextMeshProUGUI healthValue;
    
    [Header("UI Elements")]
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI statusText; // optional feedback
    
    [Header("Bar Colors")]
    public Color goodColor = new Color(0.2f, 0.8f, 0.2f);
    public Color mediumColor = new Color(0.8f, 0.8f, 0.2f);
    public Color badColor = new Color(0.8f, 0.2f, 0.2f);

    private DuckController duck;
    private BackgroundController background;
    private FeedingMiniGame feedingGame;
    private bool isDuckSleeping = false;
    
    [Header("Sleep Button")]
    public TextMeshProUGUI sleepButtonText;

    [Header("Feed Button")]
    public TextMeshProUGUI feedButtonText; // assign UI/UIPanel/Buttons/Feed/Text (TMP) in Inspector

    [Header("Play Button")]
    public TextMeshProUGUI playButtonText; // Assign UI/UIPanel/Buttons/Play/Text (TMP) in Inspector

    [Header("Status UI (pretty)")]
    public RectTransform statusRoot;   // assign UI/UIPanel/Status (container)
    public CanvasGroup   statusGroup;  // optional; added if missing
    public Image         statusBackdrop; // optional; added if missing
    public Vector2       statusPadding = new Vector2(8, 4);

    [Header("Sleep Mini-Game")]
    public SleepingMiniGame sleepMiniGame;   // Assign in inspector
    private bool nightCheckStarted = false;

    void Awake()
    {
        // Cache scene refs
        duck = FindFirstObjectByType<DuckController>();
        background = FindFirstObjectByType<BackgroundController>();
        feedingGame = FindFirstObjectByType<FeedingMiniGame>();

        WireButtons();
        SetupStatusUI();
        UpdatePlayButtonState(); // <-- Add this line
    }

    void OnEnable()
    {
        if (TokenManager.Instance != null)
        {
            TokenManager.Instance.TokensChanged += UpdateFeedButtonState;
            TokenManager.Instance.TokensChanged += UpdatePlayButtonState; // <-- Add this line
        }
        UpdateFeedButtonState();
        UpdatePlayButtonState();
    }

    void OnDisable()
    {
        if (TokenManager.Instance != null)
        {
            TokenManager.Instance.TokensChanged -= UpdateFeedButtonState;
            TokenManager.Instance.TokensChanged -= UpdatePlayButtonState; // <-- Add this line
        }
        UnwireButtons();
    }

    void Start()
    {
        // REMOVE these lines:
        // if (feedButton) feedButton.onClick.AddListener(OnFeedButtonClicked);
        // if (playButton) playButton.onClick.AddListener(OnPlayButtonClicked);
        // if (sleepButton) sleepButton.onClick.AddListener(OnSleepButtonClicked);
        // if (feedMiniGameButton) feedMiniGameButton.onClick.AddListener(StartFeedingGame);

        // if (simulate1HourButton) simulate1HourButton.onClick.AddListener(() => OnSimulateTimeButtonClicked(1f));
        // if (simulate20HoursButton) simulate20HourButton.onClick.AddListener(() => OnSimulateTimeButtonClicked(20f));

        // Initial update of UI
        UpdateNeedBars();
        UpdateTimeDisplay();
        UpdateSleepButton();
        UpdateFeedButtonState();
        UpdatePlayButtonState(); // Ensure play button is also updated

        // Start the night check coroutine if not already running
        if (!nightCheckStarted)
            StartCoroutine(CheckNightTime());
    }
    
    void SetupStatusUI()
    {
        // Auto‑find Status root and text if not set
        if (statusRoot == null)
        {
            var go = GameObject.Find("Status");
            if (go) statusRoot = go.GetComponent<RectTransform>();
        }
        if (statusText == null && statusRoot != null)
            statusText = statusRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        // Ensure group for fading
        if (statusRoot != null)
        {
            statusGroup = statusRoot.GetComponent<CanvasGroup>() ?? statusRoot.gameObject.AddComponent<CanvasGroup>();
            statusGroup.alpha = 0f;
            statusGroup.interactable = false;
            statusGroup.blocksRaycasts = false;

            // Top‑center anchor
            statusRoot.anchorMin = new Vector2(0.5f, 1f);
            statusRoot.anchorMax = new Vector2(0.5f, 1f);
            statusRoot.pivot     = new Vector2(0.5f, 1f);
            if (Mathf.Approximately(statusRoot.anchoredPosition.y, 0f))
                statusRoot.anchoredPosition = new Vector2(0f, -10f);
        }

        // Ensure readable text (center + outline)
        if (statusText != null)
        {
            statusText.alignment = TextAlignmentOptions.Center;
            var outline = statusText.GetComponent<Outline>() ?? statusText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        // Create/find backdrop behind text
        if (statusBackdrop == null && statusRoot != null)
        {
            var found = statusRoot.Find("Backdrop");
            if (found) statusBackdrop = found.GetComponent<Image>();
            if (statusBackdrop == null)
            {
                var go = new GameObject("Backdrop", typeof(RectTransform), typeof(Image));
                var rt = go.GetComponent<RectTransform>();
                rt.SetParent(statusRoot, false);
                rt.SetAsFirstSibling(); // behind text
                statusBackdrop = go.GetComponent<Image>();
                statusBackdrop.color = new Color(0f, 0f, 0f, 0.6f); // semi‑transparent black
                statusBackdrop.raycastTarget = false;
            }
        }
    }

    void WireButtons()
    {
        if (feedButton)
        {
            feedButton.onClick.RemoveAllListeners();
            feedButton.onClick.AddListener(OnFeedButtonClicked);
        }
        if (feedMiniGameButton)
        {
            feedMiniGameButton.onClick.RemoveAllListeners();
            feedMiniGameButton.onClick.AddListener(StartFeedingGame);
        }
        if (playButton)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        if (sleepButton)
        {
            sleepButton.onClick.RemoveAllListeners();
            sleepButton.onClick.AddListener(OnSleepButtonClicked);
        }
    }

    void UnwireButtons()
    {
        if (feedButton) feedButton.onClick.RemoveListener(OnFeedButtonClicked);
        if (feedMiniGameButton) feedMiniGameButton.onClick.RemoveAllListeners();
        if (playButton) playButton.onClick.RemoveListener(OnPlayButtonClicked);
        if (sleepButton) sleepButton.onClick.RemoveListener(OnSleepButtonClicked);
    }

    void Update()
    {
        UpdateNeedBars();
        UpdateTimeDisplay();
        
        // Check if duck sleep state changed and update UI
        CheckDuckSleepState();
    }
    
    void UpdateNeedBars()
    {
        if (duck == null) return;

        UpdateSingleBar(hungerBar,    duck.hunger.value);
        // no label tinting

        UpdateSingleBar(happinessBar, duck.happiness.value);
        // no label tinting

        UpdateSingleBar(energyBar,    duck.energy.value);
        // no label tinting

        UpdateSingleBar(healthBar,    duck.health.value);
        // no label tinting
    }

    void UpdateSingleBar(Slider bar, float value)
    {
        if (bar == null) return;
        bar.value = value / 100f;

        // Do not change fill color dynamically anymore
        // if (bar.fillRect != null)
        // {
        //     var fillImage = bar.fillRect.GetComponent<Image>();
        //     if (fillImage != null)
        //         fillImage.color = GetBarColor(value);
        // }
    }
    
    void UpdateTimeDisplay()
    {
        if (background != null && timeText != null)
        {
            timeText.text = background.GetTimeString();
        }
    }
    
    void CheckDuckSleepState()
    {
        if (duck != null && duck.DuckAnimator != null && duck.DuckAnimator.animator != null)
        {
            bool currentlySleeping = duck.DuckAnimator.animator.GetBool("IsSleeping");
            
            // If sleep state changed
            if (currentlySleeping != isDuckSleeping)
            {
                isDuckSleeping = currentlySleeping;
                UpdateSleepButton();
            }
        }
    }
    
    void UpdateSleepButton()
    {
        if (sleepButtonText != null)
        {
            sleepButtonText.text = isDuckSleeping ? "Wake Up" : "Sleep";
        }
    }
    
    void UpdateFeedButtonState()
    {
        if (feedButton == null) return;

        bool canClick = duck != null && (GameManager.Instance == null || !GameManager.Instance.isGameOver);
        feedButton.interactable = canClick;

        int tokenCount = TokenManager.Instance ? TokenManager.Instance.GetCount("feed") : 0;

        if (feedButtonText != null)
            feedButtonText.text = tokenCount > 0 ? $"FEED ({tokenCount})" : "EARN FEED";

        // Only update statusText if it is empty or default (not after a feed)
        if (statusText && string.IsNullOrEmpty(statusText.text))
        {
            statusText.text = tokenCount > 0
                ? "Tap FEED to spend a token"
                : "No feed tokens – playing the Feeding game will earn tokens!";
        }
    }

    void UpdatePlayButtonState()
    {
        if (playButton == null) return;

        bool canClick = duck != null && (GameManager.Instance == null || !GameManager.Instance.isGameOver);
        playButton.interactable = canClick;

        int tokenCount = TokenManager.Instance ? TokenManager.Instance.GetCount("play") : 0;

        if (playButtonText != null)
            playButtonText.text = tokenCount > 0 ? $"PLAY ({tokenCount})" : "EARN PLAY";

        // Only update statusText if it is empty or default (not after a play)
        if (statusText && string.IsNullOrEmpty(statusText.text))
        {
            statusText.text = tokenCount > 0
                ? "Tap PLAY to spend a token"
                : "No play tokens – playing the Dancing game will earn tokens!";
        }
    }

    void OnFeedButtonClicked()
    {
        SFXManager.Instance?.PlayButton();
        if (duck == null || feedButton == null)
            return;

        int tokenCount = TokenManager.Instance != null ? TokenManager.Instance.feedTokens.count : 0;
        if (tokenCount > 0)
        {
            if (TokenManager.Instance.UseToken("feed"))
            {
                duck.Feed();
                UpdateStatusText("Fed using 1 token!");
            }
            else
            {
                UpdateStatusText("No tokens available!");
            }
        }
        else
        {
            StartFeedingGame();
            UpdateStatusText("No tokens! Play the Feeding Game to earn tokens.");
        }

        UpdateFeedButtonState();

        // Prevent double-clicks
        feedButton.interactable = false;
        StartCoroutine(ReenableFeedButton());
    }

    private IEnumerator ReenableFeedButton()
    {
        yield return new WaitForSeconds(0.3f);
        if (feedButton != null)
            feedButton.interactable = true;
    }

    void StartFeedingGame()
    {
        SFXManager.Instance?.PlayMiniGameStart();
        Debug.Log("[UIManager] Loading Feeding mini-game scene");
        GameManager.Instance?.LoadFeedingMiniGameScene();
    }
    
    void OnPlayButtonClicked()
    {
        SFXManager.Instance?.PlayButton();
        if (duck == null || playButton == null)
            return;

        int tokenCount = TokenManager.Instance != null ? TokenManager.Instance.playTokens.count : 0;
        if (tokenCount > 0)
        {
            if (TokenManager.Instance.UseToken("play"))
            {
                duck.Play();
                UpdateStatusText("Played using 1 token!");
                HighlightBar(happinessBar);
                HighlightBar(energyBar, false); // Decreases energy
            }
            else
            {
                UpdateStatusText("No play tokens available!");
            }
        }
        else
        {
            Debug.Log("[UIManager] Loading Dancing mini-game scene");
            GameManager.Instance?.LoadDancingMiniGameScene();
            UpdateStatusText("No play tokens! Play the Dancing Game to earn tokens.");
        }

        UpdatePlayButtonState();
    }
    
    void OnSleepButtonClicked()
    {
        SFXManager.Instance?.PlayButton();
        if (duck != null)
        {
            if (isDuckSleeping)
            {
                // Wake up the duck
                duck.WakeUp();
                
                // Stop the sleep mini-game if it's running
                if (sleepMiniGame != null)
                {
                    sleepMiniGame.StopSleeping(true);
                }
            }
            else
            {
                // Put the duck to sleep
                duck.Sleep();
                
                // If it's night time, start the mini-game
                if (background != null)
                {
                    bool isNightTime = (background.currentTime >= background.nightStart || 
                                       background.currentTime < background.morningStart);
                                       
                    if (isNightTime && sleepMiniGame != null)
                    {
                        sleepMiniGame.StartSleeping();
                    }
                }
            }
        }
    }
    
    void OnSimulateTimeButtonClicked(float hours)
    {
        SFXManager.Instance?.PlayButton();
        if (duck != null)
        {
            duck.SimulateTime(hours);
            UpdateStatusText($"Simulated {hours} hours");
        }
    }
    
    // Pretty status with backdrop sizing + fade
    public void UpdateStatusText(string message)
    {
        if (statusText == null || statusRoot == null) return;

        statusText.text = message;
        statusText.ForceMeshUpdate();

        // Resize backdrop to text + padding
        if (statusBackdrop != null)
        {
            var size = new Vector2(statusText.preferredWidth, statusText.preferredHeight) + statusPadding * 2f;
            statusBackdrop.rectTransform.sizeDelta = size;
            statusBackdrop.transform.SetAsFirstSibling();
        }

        // Pop + fade in
        LeanTween.cancel(statusRoot.gameObject);
        statusRoot.localScale = Vector3.one * 1.02f;
        LeanTween.scale(statusRoot.gameObject, Vector3.one, 0.2f).setEaseOutBack();

        if (statusGroup != null)
        {
            LeanTween.cancel(statusGroup.gameObject);
            statusGroup.alpha = 1f;
        }

        CancelInvoke(nameof(ClearStatusText));
        Invoke(nameof(ClearStatusText), 3f);
    }

    void ClearStatusText()
    {
        if (statusText == null || statusGroup == null) return;

        // Smooth fade out
        LeanTween.value(statusGroup.gameObject, statusGroup.alpha, 0f, 0.25f)
                 .setOnUpdate((float a) => statusGroup.alpha = a)
                 .setOnComplete(() => statusText.text = string.Empty);
    }

    // Visual feedback when a bar is affected (scale only, no color flash)
    void HighlightBar(Slider bar, bool positive = true)
    {
        if (bar == null) return;

        GameObject barParent = bar.gameObject;
        LeanTween.cancel(barParent);

        barParent.transform.localScale = Vector3.one;
        LeanTween.scale(barParent, Vector3.one * 1.1f, 0.2f)
                 .setEasePunch()
                 .setLoopCount(1);

        // Removed: temporary color flash on the fill
        // var fillImage = bar.fillRect.GetComponent<Image>();
        // ...
    }

    IEnumerator CheckNightTime()
    {
        nightCheckStarted = true;
        bool wasNightTime = false;
        
        while (gameObject.activeInHierarchy)
        {
            // Check if it's night time
            bool isNightTime = (background != null) && 
                          (background.currentTime >= background.nightStart || 
                           background.currentTime < background.morningStart);
        
            // Night just started - put duck to sleep automatically
            if (isNightTime && !wasNightTime && duck != null)
            {
                // Don't auto-sleep if duck is taking damage or other critical states
                if (!duck.IsInCriticalState())
                {
                    Debug.Log("Night time - auto sleeping duck");
                    duck.Sleep(); // Put duck to sleep
                    
                    // Start the sleep mini-game
                    if (sleepMiniGame != null)
                    {
                        sleepMiniGame.StartSleeping();
                    }
                }
            }
            // Morning just arrived - wake up the duck
            else if (!isNightTime && wasNightTime && duck != null && duck.DuckAnimator.IsSleeping)
            {
                Debug.Log("Morning time - auto waking duck");
                duck.WakeUp();
            }
            
            wasNightTime = isNightTime;
            
            // If it's night and duck is sleeping, ensure mini-game is running
            if (isNightTime && duck != null && duck.DuckAnimator != null && duck.DuckAnimator.IsSleeping)
            {
                if (sleepMiniGame != null)
                {
                    sleepMiniGame.StartSleeping();
                }
            }
            
            yield return new WaitForSeconds(1.0f);
        }
        
        nightCheckStarted = false;
    }

    // Add this method if not present
    public void ShowFeedingMiniGameResult(int tokensEarned)
    {
        if (tokensEarned > 0)
            UpdateStatusText($"You earned {tokensEarned} feed token{(tokensEarned > 1 ? "s" : "")}! Use tokens to feed your duck.");
        else
            UpdateStatusText("No tokens earned. Try again!");
    }
}