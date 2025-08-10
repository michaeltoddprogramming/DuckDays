using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public FeedingMiniGame game;
    public bool isBad = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if we hit the duck
        if (other.gameObject.CompareTag("Player") && game != null)
        {
            // Register food collection
            game.CollectFood(isBad);
            
            // Visual feedback
            LeanTween.scale(gameObject, Vector3.zero, 0.2f)
                .setEaseInBack()
                .setOnComplete(() => {
                    Destroy(gameObject);
                });
        }
    }
}