using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 8;
    public float currentHealth = 7f;

    public static event System.Action onPlayerDamaged;
    public static event System.Action onPlayerHealed;
    public static event System.Action onMaxHealthChanged;
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

    private void Awake()
    {
        audioPlayer = GetComponent<PlayerAudio>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        /*
         * Nếu muốn mỗi lần chạy game luôn đầy máu,
         * giữ dòng này.
         */
        currentHealth = maxHealth;

        NotifyHealthChanged();
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;

        Debug.Log(
            $"Player Invincible = {value}"
        );
    }

    public void TakeDamage(float amount)
    {
        if (IsDead)
            return;

        if (amount <= 0f)
            return;

        if (isHurting || isInvincible)
            return;

        PlayerDash dash =
            GetComponent<PlayerDash>();

        if (dash != null &&
            dash.IsDashing)
        {
            return;
        }

        Attack attack =
            GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();

            if (animator != null)
                animator.speed = 1f;
        }

        StartCoroutine(
            HurtRoutine(amount)
        );
    }

    private IEnumerator HurtRoutine(
        float damage)
    {
        isHurting = true;
        isInvincible = true;

        currentHealth =
            Mathf.Clamp(
                currentHealth - damage,
                0f,
                maxHealth
            );

        onPlayerDamaged?.Invoke();

        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dash");
            animator.SetTrigger("Hurt");
        }

        if (audioPlayer != null)
            audioPlayer.PlayHurt();

        yield return new WaitForSeconds(
            0.25f
        );

        isHurting = false;

        if (currentHealth <= 0f)
        {
            Die();
            yield break;
        }

        yield return new WaitForSeconds(
            invincibleTime
        );

        isInvincible = false;
    }

    public bool Heal(float amount)
    {
        if (IsDead)
            return false;

        if (amount <= 0f)
            return false;

        if (currentHealth >= maxHealth)
        {
            Debug.Log("Máu đã đầy.");
            return false;
        }

        float oldHealth =
            currentHealth;

        currentHealth =
            Mathf.Clamp(
                currentHealth + amount,
                0f,
                maxHealth
            );

        float healedAmount =
            currentHealth - oldHealth;

        if (healedAmount <= 0f)
            return false;

        Debug.Log(
            $"Hồi {healedAmount} HP. " +
            $"HP: {currentHealth}/{maxHealth}"
        );

        onPlayerHealed?.Invoke();

        return true;
    }

    public bool IncreaseMaxHealth(
        int amount,
        bool healToFull = true)
    {
        if (amount <= 0)
            return false;

        int oldMaxHealth =
            maxHealth;

        maxHealth += amount;

        if (maxHealth < 1)
            maxHealth = 1;

        if (healToFull)
        {
            currentHealth =
                maxHealth;
        }
        else
        {
            currentHealth +=
                maxHealth - oldMaxHealth;

            currentHealth =
                Mathf.Clamp(
                    currentHealth,
                    0f,
                    maxHealth
                );
        }

        Debug.Log(
            $"Max Health tăng từ " +
            $"{oldMaxHealth} lên {maxHealth}. " +
            $"HP hiện tại: {currentHealth}/{maxHealth}"
        );

        onMaxHealthChanged?.Invoke();
        onPlayerHealed?.Invoke();

        return true;
    }

    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }

    private void NotifyHealthChanged()
    {
        /*
         * Dùng event damage để ép UI vẽ lại
         * khi khởi tạo hoặc reset.
         */
        onPlayerDamaged?.Invoke();
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        isHurting = false;
        isInvincible = true;

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        Players players =
            GetComponent<Players>();

        Attack attack =
            GetComponent<Attack>();

        PlayerDash dash =
            GetComponent<PlayerDash>();

        /*
         * Dừng Dash trước để coroutine Dash
         * không tiếp tục gán velocity.
         */
        if (dash != null)
        {
            dash.CancelDash();
            dash.enabled = false;
        }

        /*
         * Dừng Attack và coroutine Lunge.
         */
        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        /*
         * Tắt script di chuyển sau khi
         * các trạng thái combat đã được hủy.
         */
        if (players != null)
        {
            players.StopAutoWalk();
            players.enabled = false;
        }

        /*
         * Xóa toàn bộ vận tốc cuối cùng.
         */
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;

            /*
             * Freeze Position để đảm bảo Player
             * không bị lực hoặc va chạm đẩy trượt.
             */
            rb.constraints =
                RigidbodyConstraints2D.FreezeAll;
        }

        if (animator != null)
        {
            animator.speed = 1f;

            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);

            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dash");
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Death");

            animator.SetTrigger("Death");
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopFootstep();

        if (audioPlayer != null)
            audioPlayer.PlayDeath();
    }

    public void OnDeathAnimationFinished()
    {
        if (!IsDead)
            return;

        onPlayerDeath?.Invoke();

        GameOverManagement gameOver =
            FindFirstObjectByType<
                GameOverManagement
            >();

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
        isInvincible = true;

        if (audioPlayer == null)
        {
            audioPlayer =
                GetComponent<PlayerAudio>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints2D.FreezeRotation;

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Players players =
            GetComponent<Players>();

        if (players != null)
            players.enabled = true;

        Attack attack =
            GetComponent<Attack>();

        if (attack != null)
        {
            attack.enabled = true;
            attack.CancelAttack();
        }
        PlayerDash dash =
    GetComponent<PlayerDash>();

        if (dash != null)
        {
            dash.enabled = true;
            dash.CancelDash();
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

        NotifyHealthChanged();
    }
}