using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [Header("Status UI (pretty)")]
    public RectTransform statusRoot;   // assign UI/UIPanel/Status (container)
    public CanvasGroup   statusGroup;  // optional; added if missing
    public Image         statusBackdrop; // optional; added if missing
    public Vector2       statusPadding = new Vector2(8, 4);

    void Awake()
    {
        // Cache scene refs
        duck = FindFirstObjectByType<DuckController>();
        background = FindFirstObjectByType<BackgroundController>();
        feedingGame = FindFirstObjectByType<FeedingMiniGame>();

        WireButtons();

        SetupStatusUI();
    }

    void OnEnable()
    {
        if (TokenManager.Instance != null)
            TokenManager.Instance.TokensChanged += UpdateFeedButtonState;

        UpdateFeedButtonState();
    }

    void OnDisable()
    {
        if (TokenManager.Instance != null)
            TokenManager.Instance.TokensChanged -= UpdateFeedButtonState;

        UnwireButtons();
    }

    void Start()
    {
        // Set up button click events
        if (feedButton) feedButton.onClick.AddListener(OnFeedButtonClicked);
        if (playButton) playButton.onClick.AddListener(OnPlayButtonClicked);
        if (sleepButton) sleepButton.onClick.AddListener(OnSleepButtonClicked);
        if (feedMiniGameButton) feedMiniGameButton.onClick.AddListener(StartFeedingGame);

        if (simulate1HourButton) simulate1HourButton.onClick.AddListener(() => OnSimulateTimeButtonClicked(1f));
        if (simulate20HoursButton) simulate20HoursButton.onClick.AddListener(() => OnSimulateTimeButtonClicked(20f));

        // Initial update of UI
        UpdateNeedBars();
        UpdateTimeDisplay();
        UpdateSleepButton();
        UpdateFeedButtonState();
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
            feedButton.onClick.RemoveListener(OnFeedButtonClicked);
            feedButton.onClick.AddListener(OnFeedButtonClicked);
        }

        if (feedMiniGameButton)
        {
            feedMiniGameButton.onClick.RemoveAllListeners();
            feedMiniGameButton.onClick.AddListener(StartFeedingGame);
        }

        if (playButton)
        {
            playButton.onClick.RemoveListener(OnPlayButtonClicked);
            playButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (sleepButton)
        {
            sleepButton.onClick.RemoveListener(OnSleepButtonClicked);
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

        // Always allow clicking FEED; clicking with 0 tokens will open the mini-game
        bool canClick = duck != null && (GameManager.Instance == null || !GameManager.Instance.isGameOver);
        feedButton.interactable = canClick;

        int tokenCount = TokenManager.Instance ? TokenManager.Instance.GetCount("feed") : 0;

        // Optional: update the FEED button label to show state
        if (feedButtonText != null)
            feedButtonText.text = tokenCount > 0 ? $"FEED ({tokenCount})" : "EARN FEED";

        if (statusText)
            statusText.text = tokenCount > 0
                ? "Tap FEED to spend a token"
                : "No feed tokens – playing the Feeding game will earn tokens!";
    }

    void OnFeedButtonClicked()
    {
        Debug.Log("[UIManager] FEED clicked");
        if (duck == null) return;

        if (TokenManager.Instance != null && TokenManager.Instance.UseToken("feed"))
        {
            duck.Feed();
            if (statusText) statusText.text = "Fed using a token!";
        }
        else
        {
            StartFeedingGame();
        }

        UpdateFeedButtonState();
    }

    void StartFeedingGame()
    {
        Debug.Log("[UIManager] Loading Feeding mini-game scene");
        GameManager.Instance?.LoadFeedingMiniGameScene();
    }
    
    void OnPlayButtonClicked()
    {
        if (duck != null)
        {
            duck.Play();
            UpdateStatusText("Duck played!");
            
            // Highlight the affected bars
            HighlightBar(happinessBar);
            HighlightBar(energyBar, false); // Decreases energy
        }
    }
    
    void OnSleepButtonClicked()
    {
        if (duck != null)
        {
            if (isDuckSleeping)
            {
                // Duck is already sleeping, so wake it up
                duck.DuckAnimator.WakeUp();
                UpdateStatusText("Duck woke up!");
            }
            else
            {
                // Duck is awake, so put it to sleep
                duck.Sleep();
                UpdateStatusText("Duck is sleeping!");
            }
        }
    }
    
    void OnSimulateTimeButtonClicked(float hours)
    {
        if (duck != null)
        {
            duck.SimulateTime(hours);
            UpdateStatusText($"Simulated {hours} hours");
        }
    }
    
    // Pretty status with backdrop sizing + fade
    void UpdateStatusText(string message)
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
}