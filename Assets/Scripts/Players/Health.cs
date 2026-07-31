using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth;
    public float currentHealth = 7f;

    public static event System.Action onPlayerDamaged;
    public static event System.Action onPlayerDeath;

    private PlayerAudio audioPlayer;
    private Animator animator;

    private bool isHurting;
    public bool IsHurting => isHurting;

    [Header("Invincible")]
    public float invincibleTime = 0.2f;

    private bool isInvincible;
    public bool IsInvincible => isInvincible;

    public bool IsDead { get; private set; }

    private void Start()
    {
        currentHealth = maxHealth;

        audioPlayer = GetComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;

        Debug.Log($"Player Invincible = {value}");
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        if (isHurting || isInvincible)
            return;

        PlayerDash dash = GetComponent<PlayerDash>();

        if (dash != null && dash.IsDashing)
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

    private IEnumerator HurtRoutine(float damage)
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

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        isHurting = false;
        isInvincible = true;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        Players players = GetComponent<Players>();

        if (players != null)
            players.enabled = false;

        Attack attack = GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Death");
        }

        if (audioPlayer != null)
            audioPlayer.PlayDeath();
    }

    public void OnDeathAnimationFinished()
    {
        if (!IsDead)
            return;

        onPlayerDeath?.Invoke();

        GameOverManagement gameOver =
            FindFirstObjectByType<GameOverManagement>();

        if (gameOver != null)
        {
            gameOver.ShowGameOver();
        }
        else
        {
            Debug.LogError(
                "Không tìm thấy GameOverManagement."
            );
        }

        Debug.Log("Player is dead");
    }

    public void ResetHealth()
    {
        StopAllCoroutines();

        currentHealth = maxHealth;

        IsDead = false;
        isHurting = false;

        /*
         * Chưa tắt bất tử ở đây.
         * SceneLoader sẽ tắt sau khi:
         * - scene load xong,
         * - Player tới SpawnPoint,
         * - Fade hoàn tất.
         */
        isInvincible = true;

        if (audioPlayer == null)
            audioPlayer = GetComponent<PlayerAudio>();

        if (animator == null)
            animator = GetComponent<Animator>();

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Players players = GetComponent<Players>();

        if (players != null)
            players.enabled = true;

        Attack attack = GetComponent<Attack>();

        if (attack != null)
        {
            attack.enabled = true;
            attack.CancelAttack();
        }

        if (animator != null)
        {
            animator.speed = 1f;

            animator.Rebind();
            animator.Update(0f);

            animator.ResetTrigger("Death");
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dash");
        }

        onPlayerDamaged?.Invoke();
    }
}