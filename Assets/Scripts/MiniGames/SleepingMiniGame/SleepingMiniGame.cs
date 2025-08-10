using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SleepingMiniGame : MonoBehaviour
{
    [Header("Distractions")]
    public GameObject[] distractionObjects;
    public float spawnInterval = 3f;
    public float wakeAmount = 0.1f;
    public float distractionLifetime = 4f;
    
    [Header("Movement")]
    public float moveSpeed = 0.5f;       // Speed distractions move toward duck
    public Transform duckTarget;          // Reference to the duck's position
    
    [Header("Audio")]
    public AudioClip distractionSound;
    public AudioClip clickSound;
    public AudioClip distractionHitSound; // <-- Add this line
    
    private AudioSource audioSource;
    private DuckController duck;
    private UIManager uiManager; // Add at the top with other fields
    private bool isPlaying = false;
    private float sleepQuality = 1f;
    private int distractionsClicked = 0;
    private int distractionsMissed = 0;
    private Coroutine gameRoutine;

    void Awake()
    {
        duck = FindFirstObjectByType<DuckController>();
        uiManager = FindFirstObjectByType<UIManager>(); // Add this line
        
        // Find duck target if not assigned
        if (duckTarget == null && duck != null)
            duckTarget = duck.transform;
            
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Make sure all distractions are initially inactive
        DeactivateAllDistractions();
        
        // Make sure distraction scripts are added properly
        SetupDistractions();
        
        Debug.Log("SleepingMiniGame initialized with " + 
            (distractionObjects != null ? distractionObjects.Length : 0) + " distractions");
    }
    
    public void SetupDistractions()
    {
        if (distractionObjects == null || distractionObjects.Length == 0)
        {
            Debug.LogError("No distraction objects assigned to SleepingMiniGame!");
            return;
        }
        
        foreach (GameObject obj in distractionObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                
                SleepDistraction distraction = obj.GetComponent<SleepDistraction>();
                if (distraction == null)
                {
                    distraction = obj.AddComponent<SleepDistraction>();
                    Debug.Log("Added SleepDistraction component to " + obj.name);
                }
                
                distraction.sleepingGame = this;
                
                // Ensure it has a collider
                if (obj.GetComponent<Collider2D>() == null)
                {
                    BoxCollider2D collider = obj.AddComponent<BoxCollider2D>();
                    collider.isTrigger = true;
                    
                    // Size the collider based on sprite if possible
                    SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        collider.size = new Vector2(sr.bounds.size.x, sr.bounds.size.y);
                        Debug.Log($"Added BoxCollider2D to {obj.name} with size {collider.size}");
                    }
                }
                
                // Add DistractionMover component to handle movement
                if (obj.GetComponent<DistractionMover>() == null)
                {
                    DistractionMover mover = obj.AddComponent<DistractionMover>();
                    mover.target = duckTarget;
                    mover.speed = moveSpeed;
                }
            }
        }
    }
    
    void DeactivateAllDistractions()
    {
        if (distractionObjects != null)
        {
            foreach (GameObject obj in distractionObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    void Update()
    {
        // Only run the game when the duck is actually sleeping
        if (isPlaying && duck != null && !duck.DuckAnimator.IsSleeping)
        {
            Debug.Log("Duck woke up - stopping sleep mini-game");
            // Only call StopSleeping(true) if sleepQuality <= 0 (woken up by distractions)
            // Otherwise, call StopSleeping(false) for a natural wake up
            if (sleepQuality <= 0f)
                StopSleeping(true); // Interrupted
            else
                StopSleeping(false); // Natural wake up, give reward
        }
    }

    public void StartSleeping()
    {
        // Don't start if duck isn't sleeping
        if (duck != null && !duck.DuckAnimator.IsSleeping)
        {
            Debug.Log("Can't start sleep mini-game - duck isn't sleeping");
            return;
        }
            
        // Reset game state
        Debug.Log("Starting sleep mini-game");
        isPlaying = true;
        sleepQuality = 1f;
        distractionsClicked = 0;
        distractionsMissed = 0;
        
        // Stop any existing routine
        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
        }
        
        // Start the game routine
        gameRoutine = StartCoroutine(ReactionSleepGame());
    }
    
    public void StopSleeping(bool wasWokenUp = false)
    {
        if (!isPlaying) return;

        Debug.Log("Stopping sleep mini-game");
        isPlaying = false;

        // Stop the game routine
        if (gameRoutine != null)
        {
            StopCoroutine(gameRoutine);
            gameRoutine = null;
        }

        // Hide all distractions
        DeactivateAllDistractions();

        // Calculate sleep quality message
        string qualityMsg;
        if (sleepQuality >= 0.8f)
            qualityMsg = "Great sleep! 💤";
        else if (sleepQuality >= 0.5f)
            qualityMsg = "Okay sleep.";
        else if (sleepQuality > 0.2f)
            qualityMsg = "Restless sleep...";
        else
            qualityMsg = "Terrible sleep! 😴";

        // Apply sleep benefits
        if (!wasWokenUp && duck != null)
        {
            float energyBonus = Mathf.Lerp(10f, 40f, sleepQuality);
            float healthBonus = Mathf.Lerp(0f, 15f, sleepQuality);

            duck.energy.Recover(energyBonus);
            duck.health.Recover(healthBonus);

            Debug.Log($"Sleep ended with quality: {sleepQuality:P0}, Energy +{energyBonus:F0}, Health +{healthBonus:F0}");

            // Show message using UIManager's status system
            if (uiManager != null)
                uiManager.SendMessage("UpdateStatusText", $"{qualityMsg}\nEnergy +{energyBonus:F0}, Health +{healthBonus:F0}");
        }
        else
        {
            Debug.Log("Sleep interrupted!");
            if (uiManager != null)
                uiManager.SendMessage("UpdateStatusText", "The duck was woken up! No rest gained.");
        }
    }
    
    // New improved sleep game with moving distractions
    IEnumerator ReactionSleepGame()
    {
        Debug.Log("ReactionSleepGame coroutine started");
        
        // Force all distractions to be inactive initially
        DeactivateAllDistractions();
        
        // A short initial delay
        yield return new WaitForSeconds(1f);
        
        // Run cycles as long as the game is active and duck is sleeping
        while (isPlaying && duck != null && duck.DuckAnimator.IsSleeping)
        {
            // Choose a random distraction
            if (distractionObjects != null && distractionObjects.Length > 0)
            {
                int index = Random.Range(0, distractionObjects.Length);
                GameObject distraction = distractionObjects[index];
                
                if (distraction != null)
                {
                    // Position it outside the screen in a random direction
                    Vector3 spawnPos = GetRandomSpawnPosition();
                    distraction.transform.position = spawnPos;
                    
                    Debug.Log($"Spawning distraction {distraction.name} at {spawnPos}");
                    
                    // Make sure the mover knows where to go
                    DistractionMover mover = distraction.GetComponent<DistractionMover>();
                    if (mover != null)
                    {
                        mover.target = duckTarget;
                        mover.speed = moveSpeed;
                        mover.Reset();
                    }
                    
                    // Force it active
                    distraction.SetActive(true);
                    
                    // Play sound
                    if (distractionSound && audioSource)
                    {
                        audioSource.PlayOneShot(distractionSound, 0.5f);
                    }
                    
                    // Let it exist for lifetime or until it reaches the duck
                    float timer = 0;
                    while (timer < distractionLifetime && distraction.activeSelf)
                    {
                        timer += Time.deltaTime;
                        
                        // Check if it reached the duck
                        if (mover != null && mover.HasReachedTarget())
                        {
                            distractionsMissed++;
                            sleepQuality = Mathf.Max(0f, sleepQuality - wakeAmount);
                            Debug.Log($"Distraction reached duck! Sleep quality now: {sleepQuality:P0}");

                            // Play hit SFX
                            if (distractionHitSound && audioSource)
                                audioSource.PlayOneShot(distractionHitSound, 0.7f);

                            distraction.SetActive(false);

                            // If sleep quality got too low, wake up duck
                            if (sleepQuality <= 0)
                            {
                                duck.WakeUp();
                                StopSleeping(true);
                                yield break;
                            }
                        }
                        
                        yield return null;
                    }
                    
                    // If it's still active after time is up, it was missed
                    if (distraction && distraction.activeSelf)
                    {
                        distractionsMissed++;
                        sleepQuality = Mathf.Max(0f, sleepQuality - wakeAmount);
                        Debug.Log($"Distraction timed out! Sleep quality now: {sleepQuality:P0}");
                        distraction.SetActive(false);
                    }
                    
                    // Wait before spawning the next distraction
                    yield return new WaitForSeconds(spawnInterval);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
        
        if (!duck.DuckAnimator.IsSleeping)
        {
            Debug.Log("Duck woke up naturally - ending sleep mini-game");
        }
        
        StopSleeping(false);
    }
    
    // Get a random position outside the screen to spawn distractions
    Vector3 GetRandomSpawnPosition()
    {
        // Pick a random direction
        float angle = Random.Range(0, 360) * Mathf.Deg2Rad;
        
        // Calculate position at edge of screen
        float distance = 8f; // Distance from center
        float x = Mathf.Cos(angle) * distance;
        float y = Mathf.Sin(angle) * distance;
        
        // Return world position
        return new Vector3(x, y, 0);
    }
    
    public void OnDistractionClicked(GameObject distraction)
    {
        if (!isPlaying) return;
        
        Debug.Log($"Distraction clicked: {distraction.name}");
        
        // Play click sound
        if (clickSound && audioSource)
        {
            audioSource.PlayOneShot(clickSound, 0.7f);
        }
        
        // Increment counter
        distractionsClicked++;
        
        // Hide the distraction
        distraction.SetActive(false);
    }

    [ContextMenu("Test Start Game")]
    public void TestStartGame()
    {
        Debug.Log("TEST: Manually starting sleep mini-game");
        StartSleeping();
    }
}