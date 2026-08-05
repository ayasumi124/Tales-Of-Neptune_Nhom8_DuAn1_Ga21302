using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [Min(1)]
    public int maxHealth = 8;

    public float currentHealth = 7f;

    public static event System.Action onPlayerDamaged;
    public static event System.Action onPlayerHealed;
    public static event System.Action onMaxHealthChanged;
    public static event System.Action onPlayerDeath;

    [Header("Hurt")]
    [Tooltip(
        "Thời gian Player đứng yên khi bị đánh. " +
        "Nên đặt bằng thời lượng animation Hurt hoặc Hit Effect."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float hurtLockTime = 0.25f;

    [Header("Invincible")]
    [Tooltip(
        "Thời gian bất tử thêm sau khi kết thúc trạng thái Hurt."
    )]
    [Min(0f)]
    public float invincibleTime = 0.2f;

    private PlayerAudio audioPlayer;
    private Animator animator;
    private Rigidbody2D rb;
    private Players players;
    private Attack attack;
    private PlayerDash dash;

    private Coroutine hurtCoroutine;

    private bool isHurting;
    private bool isInvincible;

    public bool IsHurting => isHurting;
    public bool IsInvincible => isInvincible;

    public bool IsDead
    {
        get;
        private set;
    }

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        /*
         * Mỗi lần bắt đầu game, Player đầy máu.
         */
        currentHealth = maxHealth;

        IsDead = false;
        isHurting = false;
        isInvincible = false;

        NotifyHealthChanged();
    }

    private void CacheComponents()
    {
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

        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (players == null)
        {
            players =
                GetComponent<Players>();
        }

        if (attack == null)
        {
            attack =
                GetComponent<Attack>();
        }

        if (dash == null)
        {
            dash =
                GetComponent<PlayerDash>();
        }
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

        if (isHurting ||
            isInvincible)
        {
            return;
        }

        /*
         * Không nhận damage trong lúc Dash.
         */
        if (dash != null &&
            dash.IsDashing)
        {
            return;
        }

        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        hurtCoroutine =
            StartCoroutine(
                HurtRoutine(amount)
            );
    }

    private IEnumerator HurtRoutine(
        float damage)
    {
        isHurting = true;
        isInvincible = true;

        CancelCurrentActions();
        LockPlayerForHurt();

        currentHealth =
            Mathf.Clamp(
                currentHealth - damage,
                0f,
                maxHealth
            );

        onPlayerDamaged?.Invoke();

        if (animator != null)
        {
            animator.speed = 1f;

            animator.SetBool(
                "IsMoving",
                false
            );

            animator.SetBool(
                "IsRunning",
                false
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                "Dash"
            );

            animator.ResetTrigger(
                "Hurt"
            );

            animator.SetTrigger(
                "Hurt"
            );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .StopFootstep();
        }

        if (audioPlayer != null)
        {
            audioPlayer.PlayHurt();
        }

        /*
         * Player đứng yên trong toàn bộ thời gian Hurt.
         */
        float hurtTimer =
            Mathf.Max(
                0.01f,
                hurtLockTime
            );

        while (hurtTimer > 0f)
        {
            KeepPlayerStopped();

            hurtTimer -=
                Time.deltaTime;

            yield return null;
        }

        if (currentHealth <= 0f)
        {
            isHurting = false;
            hurtCoroutine = null;

            Die();
            yield break;
        }

        isHurting = false;

        UnlockPlayerAfterHurt();

        /*
         * Tiếp tục bất tử một khoảng ngắn
         * sau khi Player được phép di chuyển lại.
         */
        float invincibleTimer =
            Mathf.Max(
                0f,
                invincibleTime
            );

        while (invincibleTimer > 0f)
        {
            if (IsDead)
            {
                hurtCoroutine = null;
                yield break;
            }

            invincibleTimer -=
                Time.deltaTime;

            yield return null;
        }

        if (!IsDead)
        {
            isInvincible = false;
        }

        hurtCoroutine = null;
    }

    private void CancelCurrentActions()
    {
        if (attack != null)
        {
            attack.CancelAttack();
        }

        if (dash != null &&
            dash.IsDashing)
        {
            dash.CancelDash();
        }

        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    private void LockPlayerForHurt()
    {
        if (players != null)
        {
            players.StopAutoWalk();
            players.LockControl();
        }

        KeepPlayerStopped();
    }

    private void KeepPlayerStopped()
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity =
            0f;
    }

    private void UnlockPlayerAfterHurt()
    {
        if (IsDead ||
            players == null)
        {
            return;
        }

        /*
         * Không mở điều khiển khi đang load scene.
         */
        bool sceneLoading =
            SceneLoader.Instance != null &&
            SceneLoader.Instance.IsLoading;

        if (sceneLoading)
            return;

        /*
         * Không mở điều khiển nếu Inventory đang mở.
         */
        bool inventoryOpen =
            SkillInventoryUI.Instance != null &&
            SkillInventoryUI.Instance.IsOpen;

        if (inventoryOpen)
            return;

        players.UnlockControl();
    }

    /*
     * Có thể đặt Animation Event này ở frame cuối clip Hurt.
     * Không bắt buộc vì coroutine đã tự xử lý theo hurtLockTime.
     */
    public void EndHurt()
    {
        if (IsDead ||
            !isHurting)
        {
            return;
        }

        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        isHurting = false;

        UnlockPlayerAfterHurt();

        StartCoroutine(
            InvincibilityAfterHurtRoutine()
        );
    }

    private IEnumerator
        InvincibilityAfterHurtRoutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                invincibleTime
            )
        );

        if (!IsDead)
        {
            isInvincible = false;
        }
    }

    public bool Heal(float amount)
    {
        if (IsDead)
            return false;

        if (amount <= 0f)
            return false;

        if (currentHealth >= maxHealth)
        {
            Debug.Log(
                "Máu đã đầy."
            );

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
            currentHealth -
            oldHealth;

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
        {
            maxHealth = 1;
        }

        if (healToFull)
        {
            currentHealth =
                maxHealth;
        }
        else
        {
            currentHealth +=
                maxHealth -
                oldMaxHealth;

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
        return currentHealth >=
               maxHealth;
    }

    private void NotifyHealthChanged()
    {
        onPlayerDamaged?.Invoke();
    }

    private void Die()
    {
        if (IsDead)
            return;

        IsDead = true;
        isHurting = false;
        isInvincible = true;

        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        CancelCurrentActions();

        if (dash != null)
        {
            dash.CancelDash();
            dash.enabled = false;
        }

        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        if (players != null)
        {
            players.StopAutoWalk();
            players.LockControl();
            players.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            rb.constraints =
                RigidbodyConstraints2D
                    .FreezeAll;
        }

        if (animator != null)
        {
            animator.speed = 1f;

            animator.SetBool(
                "IsMoving",
                false
            );

            animator.SetBool(
                "IsRunning",
                false
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                "Dash"
            );

            animator.ResetTrigger(
                "Hurt"
            );

            animator.ResetTrigger(
                "Death"
            );

            animator.SetTrigger(
                "Death"
            );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .StopFootstep();
        }

        if (audioPlayer != null)
        {
            audioPlayer.PlayDeath();
        }
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

        Debug.Log(
            "Player is dead"
        );
    }

    public void ResetHealth()
    {
        StopAllCoroutines();

        CacheComponents();

        currentHealth =
            maxHealth;

        IsDead = false;
        isHurting = false;
        isInvincible = true;
        hurtCoroutine = null;

        if (rb != null)
        {
            rb.constraints =
                RigidbodyConstraints2D
                    .FreezeRotation;

            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        if (players != null)
        {
            players.enabled = true;
            players.StopAutoWalk();
            players.UnlockControl();
        }

        if (attack != null)
        {
            attack.enabled = true;
            attack.CancelAttack();
        }

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

            animator.ResetTrigger(
                "Death"
            );

            animator.ResetTrigger(
                "Hurt"
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                "Dash"
            );

            animator.SetBool(
                "IsMoving",
                false
            );

            animator.SetBool(
                "IsRunning",
                false
            );
        }

        NotifyHealthChanged();

        StartCoroutine(
            ResetInvincibilityRoutine()
        );
    }

    private IEnumerator
        ResetInvincibilityRoutine()
    {
        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                invincibleTime
            )
        );

        if (!IsDead)
        {
            isInvincible = false;
        }
    }

    private void OnDisable()
    {
        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        KeepPlayerStopped();
    }

    private void OnValidate()
    {
        maxHealth =
            Mathf.Max(
                1,
                maxHealth
            );

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0f,
                maxHealth
            );

        hurtLockTime =
            Mathf.Max(
                0.01f,
                hurtLockTime
            );

        invincibleTime =
            Mathf.Max(
                0f,
                invincibleTime
            );
    }
}