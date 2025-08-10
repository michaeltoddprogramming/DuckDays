using UnityEngine;
using UnityEngine.EventSystems;

public class SleepDistraction : MonoBehaviour
{
    public SleepingMiniGame sleepingGame;
    
    void OnEnable()
    {
        Debug.Log($"Distraction {gameObject.name} enabled");
    }
    
    void OnDisable()
    {
        Debug.Log($"Distraction {gameObject.name} disabled");
    }
    
    void OnMouseDown()
    {
        Debug.Log($"Mouse clicked on {gameObject.name}");
        HandleClick();
    }
    
    private void HandleClick()
    {
        if (sleepingGame != null)
        {
            sleepingGame.OnDistractionClicked(gameObject);
        }
        else
        {
            Debug.LogError($"No sleepingGame reference in {gameObject.name}");
            gameObject.SetActive(false);
        }
    }
}