using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth;
    public float currentHealth = 7;

    public static event System.Action onPlayerDamaged;
    public static event System.Action onPlayerDeath;

    private PlayerAudio audioPlayer;
    private Animator animator;

    private bool isHurting;
    public bool IsHurting => isHurting;

    [Header("Invincible")]
    public float invincibleTime = 0.2f;

    private bool isInvincible;

    public bool IsDead { get; private set; }

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

            if (animator != null)
                animator.speed = 1f;
        }
    }

    void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        isHurting = false;
        isInvincible = true;

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Death");
        }

        if (audioPlayer != null)
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

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            IsDead = true;

            Rigidbody2D rb = GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            Players players = GetComponent<Players>();

            if (players != null)
                players.enabled = false;

            Attack attack = GetComponent<Attack>();

            if (attack != null)
                attack.enabled = false;

            Debug.Log("Player is dead");
        }
    }

    public void ResetHealth()
    {
        StopAllCoroutines();

        currentHealth = maxHealth;

        IsDead = false;
        isHurting = false;
        isInvincible = false;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.speed = 1f;
            animator.ResetTrigger("Death");
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dash");
        }

        onPlayerDamaged?.Invoke();
    }

    IEnumerator HurtRoutine(float damage)
    {
        isHurting = true;
        isInvincible = true;

        currentHealth -= damage;

        if (currentHealth < 0f)
            currentHealth = 0f;

        onPlayerDamaged?.Invoke();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dash");
            animator.SetTrigger("Hurt");
        }

        if (audioPlayer != null)
            audioPlayer.PlayHurt();

        yield return new WaitForSeconds(0.25f);

        isHurting = false;

        if (currentHealth <= 0f)
        {
            Die();
            yield break;
        }

        yield return new WaitForSeconds(invincibleTime);

        isInvincible = false;
    }
}