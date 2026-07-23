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

    public bool IsDead { get; private set; } = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        audioPlayer = GetComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
    }


    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        currentHealth -= amount;
        onPlayerDamaged?.Invoke();
        Attack attack = GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();
        }

        animator.SetTrigger("Hurt");
        audioPlayer.PlayHurt();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            IsDead = true;

            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Death");
            audioPlayer.PlayDeath();

            Debug.Log("Player is dead");

            if (attack != null)
                attack.enabled = false;
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void OnDeathAnimationFinished()
    {
        onPlayerDeath?.Invoke();

        GameOverManagement gameOver =
            FindFirstObjectByType<GameOverManagement>();

        if (gameOver != null)
            gameOver.ShowGameOver();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            IsDead = true;

            // Dừng di chuyển
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // Tắt điều khiển
            Players players = GetComponent<Players>();
            if (players != null)
                players.enabled = false;

            // Tắt đánh
            Attack attack = GetComponent<Attack>();
            if (attack != null)
                attack.enabled = false;

            Debug.Log("Player is dead");
        }
    }
}



