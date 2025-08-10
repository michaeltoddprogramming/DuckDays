using UnityEngine;

public class HitZone : MonoBehaviour
{
    [SerializeField] private KeyCode inputKey; 
    [SerializeField] private float perfectWindow = 0.1f; 
    [SerializeField] private float goodWindow = 0.25f; 
    [SerializeField] private DuckDanceAnim duckAnim = DuckDanceAnim.Quack;
    [SerializeField] private AudioClip hitSfx;
    [SerializeField] private AudioSource audioSource;

    private DuckAnimator duckAnimator;
    private DanceNote noteInZone;

    private void Start()
    {
        duckAnimator = FindObjectOfType<DuckAnimator>();
        Vector3 baseScale = transform.localScale;
        LeanTween.scale(gameObject, baseScale * 1.25f, 0.8f)
            .setEaseInOutSine()
            .setLoopPingPong();
    }

    void Update()
    {
        if (noteInZone != null && Input.GetKeyDown(inputKey))
        {
            noteInZone.TryHit();

            // --- Play SFX with pitch based on score ---
            if (audioSource != null && hitSfx != null)
            {
                float pitch = 1f;
                var danceGameManager = FindObjectOfType<DanceGameManager>();
                if (danceGameManager != null)
                {
                    // Example: pitch goes from 1.0 to 1.5 as score goes from 0 to 5000
                    pitch = 1f + Mathf.Clamp01(danceGameManager.Score / 5000f) * 0.5f;
                }
                audioSource.pitch = pitch;
                audioSource.PlayOneShot(hitSfx, 0.7f);
            }
            // ------------------------------------------

            if (duckAnimator != null)
            {
                switch (duckAnim)
                {
                    case DuckDanceAnim.Quack:
                        duckAnimator.Quack();
                        break;
                    case DuckDanceAnim.Blink:
                        duckAnimator.Blink();
                        break;
                    case DuckDanceAnim.SetWalking:
                        duckAnimator.SetWalking(true);
                        Invoke(nameof(ResetWalking), 0.3f); // Reset after 0.3 seconds
                        break;
                    case DuckDanceAnim.SetDirection:
                        duckAnimator.SetDirection(1);
                        Invoke(nameof(ResetDirection), 0.3f); // Reset after 0.3 seconds
                        break;
                }
            }
            noteInZone = null;
        }
    }

    private void ResetWalking()
    {
        if (duckAnimator != null)
            duckAnimator.SetWalking(false);
    }

    private void ResetDirection()
    {
        if (duckAnimator != null)
            duckAnimator.SetDirection(0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<DanceNote>() != null)
        {
            noteInZone = collision.GetComponent<DanceNote>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.GetComponent<DanceNote>() == noteInZone)
        {
            noteInZone = null;
        }
    }
}

public enum DuckDanceAnim
{
    Quack,
    Blink,
    SetWalking,
    SetDirection
}