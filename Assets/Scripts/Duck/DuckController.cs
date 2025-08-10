using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DuckAnimator), typeof(DuckAudio))]
public class DuckController : MonoBehaviour
{
    public DuckAnimator DuckAnimator => duckAnimator; // Public accessor for the private field
    public PetNeed hunger;
    public PetNeed happiness;
    public PetNeed energy;
    public PetNeed health;

    public KeyCode feedKey = KeyCode.F;
    public KeyCode playKey = KeyCode.P;
    public KeyCode sleepKey = KeyCode.S;

    public float minBlinkTime = 3f;
    public float maxBlinkTime = 6f;
    public float minQuackInterval = 5f;
    public float maxQuackInterval = 10f;

    private float blinkTimer;
    private float quackTimer;

    private DuckAnimator duckAnimator;
    private DuckAudio duckAudio;
    private BackgroundController background;

    private const float LowNeed = 30f;
    private const float HighNeed = 70f;
    private const float DAMAGE_ANIM_COOLDOWN = 2f; // Prevent animation spam

    private bool isDead = false;
        // Near the top of the class, with other properties

    private float previousHealth;
    private float damageAnimCooldown = 0f;

    void Start()
    {
        duckAnimator = GetComponent<DuckAnimator>();
        duckAudio = GetComponent<DuckAudio>();
        background = FindFirstObjectByType<BackgroundController>();

        // Initialize the previous health
        previousHealth = health.value;
        
        ResetBlinkTimer();
        ResetQuackTimer();

        quackTimer = maxQuackInterval + 1f; // Avoid quacking immediately on start
    }

    void Update()
    {
        // Pause all duck logic while a mini-game is active
        if (GameManager.Instance != null && GameManager.Instance.isMiniGameActive)
            return;

        if (!isDead)
        {
            UpdateNeeds();
            HandleTimers();
            HandleInput();

            if (damageAnimCooldown > 0)
                damageAnimCooldown -= Time.deltaTime;
        }
    }

    void UpdateNeeds()
    {
        float multiplier = background ? background.timeSpeed : 10f;
        float deltaHours = Time.deltaTime * multiplier / 3600f;

        // Check if duck is sleeping
        bool isSleeping = duckAnimator.IsSleeping;

        // Basic need decay (energy doesn't decay when sleeping)
        hunger.Decay(deltaHours); // Still get hungry while sleeping (slower?)
        if (!isSleeping)
        {
            happiness.Decay(deltaHours);
            energy.Decay(deltaHours);
        }
        else
        {
            // Energy recovery during sleep (continuous)
            energy.Recover(10f * deltaHours); // Gradual recovery while sleeping
            
            // Very small health recovery during sleep if not too unhealthy
            if (health.value > 30f)
                health.Recover(1f * deltaHours);
        }

        // Calculate average needs for health regeneration
        float avgNeeds = (hunger.value + happiness.value + energy.value) / 3f;
        
        // Health regeneration when average needs are above 80 (excellent care)
        if (avgNeeds > 80f)
        {
            // Higher regeneration when needs are better cared for
            float regenerationRate = Mathf.Lerp(2f, 5f, (avgNeeds - 80f) / 20f);
            health.Recover(regenerationRate * deltaHours);
            
            // Optional: Debug info to see when healing is happening
            if (health.value < 100f && deltaHours > 0)
                Debug.Log($"Duck healing: +{regenerationRate * deltaHours:F2} (avg needs: {avgNeeds:F1}%)");
        }

        // Night time gives a small energy boost, but only if not sleeping
        // (to avoid double-counting sleep benefits)
        if (IsNightTime() && !isSleeping)
            energy.Recover(2f * deltaHours);

        // If hunger is very high, give tiny energy boost (food provides some energy)
        if (hunger.value > 90f && !isSleeping)
            energy.Recover(1f * deltaHours);

        // HEALTH SYSTEM: Needs directly impact health
        // If any need hits zero, rapidly damage health (except energy when sleeping)
        if (hunger.value <= 0)
        {
            health.Decay(8f * deltaHours);
            if (Random.value < 0.05f * deltaHours)
                duckAnimator.TakeDamage();
        }
        
        if (happiness.value <= 0)
        {
            health.Decay(5f * deltaHours);
            if (Random.value < 0.03f * deltaHours)
                duckAnimator.TakeDamage();
        }
        
        if (energy.value <= 0 && !isSleeping) // Only check when not sleeping
        {
            health.Decay(7f * deltaHours);
            if (Random.value < 0.04f * deltaHours)
                duckAnimator.TakeDamage();
        }

        // Low needs slowly damage health
        if (hunger.value < LowNeed) health.Decay(2f * deltaHours);
        if (happiness.value < LowNeed) health.Decay(1f * deltaHours);
        if (energy.value < LowNeed && !isSleeping) health.Decay(0.7f * deltaHours);

        // Health only recovers when ALL needs are very high (90%+)
        if (hunger.value > 90f && happiness.value > 90f && energy.value > 90f)
        {
            health.Recover(3f * deltaHours);
        }

        // Clamp all values
        hunger.Clamp(); happiness.Clamp(); energy.Clamp(); health.Clamp();
        
        // Check for health decrease and play damage animation
        CheckHealthDecrease();
        
        // Check for death
        if (health.value <= 0 && !isDead)
        {
            Die();
        }

        // Add this near the end of UpdateNeeds to track lifespan
        if (GameManager.Instance != null && background != null)
        {
            GameManager.Instance.UpdateLifespan(background.currentTime, background.daysPassed);
        }
    }
    
    void CheckHealthDecrease()
    {
        // Calculate health change
        float healthChange = health.value - previousHealth;
        
        // If ANY health was lost and cooldown is over
        if (healthChange < 0 && damageAnimCooldown <= 0)
        {
            // MUCH higher chance to play animation - almost guaranteed on any health loss
            float chance = Mathf.Min(1.0f, -healthChange * 2f); // 100% chance with 0.5 health loss
            
            // Debug.Log($"Health decreased by {-healthChange}, chance to play damage: {chance}");
            
            if (Random.value < chance)
            {
                Debug.Log("Playing damage animation due to health decrease");
                duckAnimator.TakeDamage();
                damageAnimCooldown = DAMAGE_ANIM_COOLDOWN;
            }
        }
        
        // Always update previous health for next frame
        previousHealth = health.value;
    }

    void HandleTimers()
    {
        float blinkSpeed = energy.value < LowNeed ? 0.5f : 1f;
        blinkTimer -= Time.deltaTime * blinkSpeed;
        if (blinkTimer <= 0f)
        {
            duckAnimator.Blink();
            ResetBlinkTimer();
        }

        quackTimer -= Time.deltaTime * GetQuackModifier();
        if (quackTimer <= 0f)
        {
            duckAudio.PlayRandomQuack();
            duckAnimator.Quack();
            ResetQuackTimer();
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(feedKey)) Feed();
        if (Input.GetKeyDown(playKey)) Play();
        if (Input.GetKeyDown(sleepKey)) Sleep();
        if (Input.GetKeyDown(KeyCode.Alpha1)) SimulateTime(1f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SimulateTime(20f);
    }

    public void Feed()
    {
        hunger.Recover(30f);
        happiness.Recover(10f);
        energy.Recover(5f);
        
        // Small direct health recovery when fed
        if (hunger.value > 50f) // Only heal if hunger is now above half
        {
            health.Recover(2f); 
        }
        
        duckAnimator.Quack();
        DebugStatus("Fed");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TrackFed();
            GameManager.Instance.UpdateMaxStats(hunger.value, happiness.value, energy.value);
        }
    }

    public void Play()
    {
        happiness.Recover(25f);
        energy.Decay(15f);
        duckAnimator.Quack();
        DebugStatus("Played");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TrackPlayed();
            GameManager.Instance.UpdateMaxStats(hunger.value, happiness.value, energy.value);
        }
    }

    public void Sleep()
    {
        energy.Recover(40f);
        
        if (health.value > 30f)
            health.Recover(5f);
            
        duckAnimator.SetSleep();
        DebugStatus("Slept");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TrackSlept();
            GameManager.Instance.UpdateMaxStats(hunger.value, happiness.value, energy.value);
        }
    }

    // Add a method to handle waking up
    public void WakeUp()
    {
        duckAnimator.WakeUp();
        DebugStatus("Woke up");
    }

    public void SimulateTime(float hours)
    {
        if (background)
        {
            background.SetTime((background.currentTime + hours) % 24f);
            Debug.Log("Simulated " + hours + " hrs. Time: " + background.GetTimeString());
        }
    }

    float GetQuackModifier()
    {
        if (happiness.value > 70f) return 1.5f;
        if (happiness.value < 30f) return 0.5f;
        if (energy.value < 20f) return 0.3f;
        return 1f;
    }

    bool IsNightTime()
    {
        return background && (background.currentTime >= background.nightStart || background.currentTime < background.morningStart);
    }

    void ResetBlinkTimer() => blinkTimer = Random.Range(minBlinkTime, maxBlinkTime);
    void ResetQuackTimer() => quackTimer = Random.Range(minQuackInterval, maxQuackInterval);

    void DebugStatus(string label)
    {
        Debug.Log($"[Duck] {label} | Hunger: {hunger.value:F1}, Happy: {happiness.value:F1}, Energy: {energy.value:F1}, Health: {health.value:F1}");
    }

    // Add the Die method
    void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("[Duck] The duck has died! Game over.");

        string causeOfDeath = "unknown causes";
        if (hunger.value <= 0) causeOfDeath = "starvation";
        else if (happiness.value <= 0) causeOfDeath = "sadness";
        else if (energy.value <= 0) causeOfDeath = "exhaustion";
        else causeOfDeath = "poor health";

        duckAnimator.SetDead();

        if (GameManager.Instance != null)
            GameManager.Instance.EndGame(causeOfDeath);
        else
            Debug.LogWarning("[Duck] GameManager.Instance was null on death");

        // stop duck updates
        enabled = false;
    }
}
