using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int maxHealth;
    public float currentHealth = 7;
    public static event System.Action onPlayerDamaged;
    private PlayerAudio audioPlayer;
    private Animator animator;
    public static event System.Action onPlayerDeath;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        audioPlayer = GetComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        
        onPlayerDamaged?.Invoke();
        animator.SetTrigger("Hurt");
        audioPlayer.PlayHurt();
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            animator.SetTrigger("Death");
            audioPlayer.PlayDeath();
            onPlayerDeath?.Invoke();

            Debug.Log("Player is dead");

            GameOverManagement gameOver =
                FindFirstObjectByType<GameOverManagement>();

            if (gameOver != null)
            {
                gameOver.ShowGameOver();
            }


        }

    }

    // Update is called once per frame
    void Update()
    {

    }

}



