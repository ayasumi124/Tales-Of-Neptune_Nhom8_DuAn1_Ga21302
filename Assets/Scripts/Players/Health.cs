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
    private bool isHurting;
    public bool IsHurting => isHurting;
    public static event System.Action onPlayerDeath;

    [Header("Invincible")]
    public float invincibleTime = 0.2f;

    private bool isInvincible;

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
        PlayerDash dash = GetComponent<PlayerDash>();

        if (dash != null && dash.IsDashing)
            return;
        if (IsDead)
            return;

        if (isHurting || isInvincible)
            return;

        StartCoroutine(HurtRoutine(amount));

        Attack attack = GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();
            animator.speed = 1f;
        }


    }
    // Update is called once per frame
    void Update()
    {

    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        isHurting = false;
        isInvincible = true;

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Hurt");
        animator.SetTrigger("Death");

        audioPlayer.PlayDeath();

        Attack attack = GetComponent<Attack>();
        if (attack != null)
            attack.enabled = false;
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

    IEnumerator HurtRoutine(float damage)
    {
        isHurting = true;
        isInvincible = true;

        currentHealth -= damage;

        onPlayerDamaged?.Invoke();

        animator.ResetTrigger("Attack");
        animator.ResetTrigger("Dash");
        animator.SetTrigger("Hurt");

        audioPlayer.PlayHurt();

        yield return new WaitForSeconds(0.25f);

        isHurting = false;

        if (currentHealth <= 0)
        {
            Die();
            yield break;
        }

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }
}



