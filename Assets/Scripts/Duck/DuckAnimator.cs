using UnityEngine;

public class DuckAnimator : MonoBehaviour
{
    public Animator animator;
    
    // Add a public getter to check sleep state from other scripts
    public bool IsSleeping => animator && animator.GetBool("IsSleeping");

    void Awake()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void Blink()
    {
        if (animator) animator.SetTrigger("Blink");
    }

    public void Quack()
    {
        if (animator) animator.SetTrigger("Quack");
    }

    public void TakeDamage()
    {
        if (animator) 
        {
            animator.SetTrigger("TakeDamage");
            Debug.Log("TakeDamage animation triggered!");
        }
        else
        {
            Debug.LogWarning("Animator is null in TakeDamage");
        }
    }

    public void SetSleep()
    {
        if (animator)
        {
            animator.SetTrigger("Sleep");
            animator.SetBool("IsSleeping", true);
        }
    }

    public void WakeUp()
    {
        if (animator)
        {
            animator.SetBool("IsSleeping", false);
        }
    }
    
    public void SetDead()
    {
        if (animator)
        {
            animator.SetTrigger("Die");
            animator.SetBool("IsDead", true);
        }
    }

    public void Run()
    {
        if (animator) animator.SetTrigger("Run");
    }

    public void Talk()
    {
        if (animator) animator.SetTrigger("Talk");
    }

    public void SetWalking(bool walking)
    {
        if (animator) animator.SetBool("isWalking", walking);
    }

    public void SetDirection(int dir)
    {
        if (animator) animator.SetInteger("direction", dir);
    }
}
