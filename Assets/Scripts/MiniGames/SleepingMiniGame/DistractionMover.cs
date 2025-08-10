using UnityEngine;

public class DistractionMover : MonoBehaviour
{
    public Transform target;
    public float speed = 0.5f;
    private bool hasReachedTarget = false;
    
    void OnEnable()
    {
        hasReachedTarget = false;
    }
    
    void Update()
    {
        if (target == null || hasReachedTarget) return;
        
        // Move toward the target
        transform.position = Vector3.MoveTowards(
            transform.position, 
            target.position, 
            speed * Time.deltaTime
        );
        
        // Check if reached target
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.5f) // Target reach threshold
        {
            hasReachedTarget = true;
        }
    }
    
    public bool HasReachedTarget()
    {
        return hasReachedTarget;
    }
    
    public void Reset()
    {
        hasReachedTarget = false;
    }
}