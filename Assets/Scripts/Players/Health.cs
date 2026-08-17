using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    // =====================================================
    // HEALTH
    // =====================================================

    [Header("Health")]
    [Min(1)]
    public int maxHealth = 8;

    public float currentHealth = 7f;

    public static event System.Action onPlayerDamaged;
    public static event System.Action onPlayerHealed;
    public static event System.Action onMaxHealthChanged;
    public static event System.Action onPlayerDeath;

    // =====================================================
    // HURT
    // =====================================================

    [Header("Hurt")]
    [Tooltip(
        "Thời gian Player đứng yên khi bị đánh. " +
        "Nên đặt bằng thời lượng animation Hurt hoặc Hit Effect."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float hurtLockTime = 0.25f;

    // =====================================================
    // INVINCIBLE
    // =====================================================

    [Header("Invincible")]
    [Tooltip(
        "Thời gian bất tử thêm sau khi kết thúc trạng thái Hurt."
    )]
    [Min(0f)]
    public float invincibleTime = 0.2f;

    [Header("Fairy Revive")]
    [Tooltip(
        "Thời gian bất tử sau khi Fairy hồi sinh Player."
    )]
    [Min(0f)]
    [SerializeField]
    private float fairyReviveInvincibleTime = 1.5f;

    // =====================================================
    // COMPONENTS
    // =====================================================

    private PlayerAudio audioPlayer;
    private Animator animator;
    private Rigidbody2D rb;
    private Players players;
    private Attack attack;
    private PlayerDash dash;

    private FairySkill fairySkill;

    // =====================================================
    // COROUTINES
    // =====================================================

    private Coroutine hurtCoroutine;
    private Coroutine reviveInvincibleCoroutine;

    // =====================================================
    // STATE
    // =====================================================

    private bool isHurting;
    private bool isInvincible;

    public bool IsHurting =>
        isHurting;

    public bool IsInvincible =>
        isInvincible;

    public bool IsDead
    {
        get;
        private set;
    }

    // =====================================================
    // UNITY
    // =====================================================

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
         * Bắt đầu game với full HP.
         */
        currentHealth =
            maxHealth;

        IsDead = false;
        isHurting = false;
        isInvincible = false;

        FindFairySkill();

        NotifyHealthChanged();
    }

    // =====================================================
    // CACHE
    // =====================================================

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

    private void FindFairySkill()
    {
        if (fairySkill != null)
            return;

        fairySkill =
            FindFirstObjectByType<
                FairySkill
            >();
    }

    // =====================================================
    // INVINCIBLE
    // =====================================================

    public void SetInvincible(
        bool value)
    {
        isInvincible =
            value;

        Debug.Log(
            $"Player Invincible = {value}"
        );
    }

    // =====================================================
    // TAKE DAMAGE
    // =====================================================

    public void TakeDamage(
        float amount)
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
         * Không nhận damage khi Dash.
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
                HurtRoutine(
                    amount
                )
            );
    }

    // =====================================================
    // HURT
    // =====================================================

    private IEnumerator HurtRoutine(
        float damage)
    {
        isHurting = true;
        isInvincible = true;

        CancelCurrentActions();

        LockPlayerForHurt();

        currentHealth =
            Mathf.Clamp(
                currentHealth -
                damage,
                0f,
                maxHealth
            );

        onPlayerDamaged?.Invoke();

        // =================================================
        // HURT ANIMATION
        // =================================================

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

        // =================================================
        // AUDIO
        // =================================================

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .StopFootstep();
        }

        if (audioPlayer != null)
        {
            audioPlayer.PlayHurt();
        }

        // =================================================
        // HURT LOCK
        // =================================================

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

        // =================================================
        // LETHAL DAMAGE
        // =================================================

        if (currentHealth <= 0f)
        {
            /*
             * Coroutine này chuẩn bị kết thúc.
             * Clear reference trước để ReviveFull()
             * không cố Stop chính coroutine hiện tại.
             */
            hurtCoroutine = null;

            isHurting = false;

            /*
             * FAIRY REVIVE
             *
             * Nếu Player đang giữ Fairy Revive:
             *
             * - Consume buff.
             * - Full HP.
             * - Không Die().
             * - Không Game Over.
             */
            if (TryFairyRevive())
            {
                yield break;
            }

            /*
             * Không còn Fairy Revive.
             */
            Die();

            yield break;
        }

        // =================================================
        // NORMAL HURT END
        // =================================================

        isHurting = false;

        UnlockPlayerAfterHurt();

        /*
         * Tiếp tục bất tử một khoảng ngắn.
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
            isInvincible =
                false;
        }

        hurtCoroutine = null;
    }

    // =====================================================
    // FAIRY REVIVE
    // =====================================================

    private bool TryFairyRevive()
    {
        /*
         * Health này là của Player,
         * nhưng vẫn check Tag cho an toàn.
         */
        if (!CompareTag("Player"))
            return false;

        FindFairySkill();

        if (fairySkill == null)
        {
            return false;
        }

        /*
         * FairySkill sẽ:
         *
         * 1. Check hasReviveEffect.
         * 2. Consume revive.
         * 3. Gọi Health.ReviveFull().
         */
        bool revived =
            fairySkill
                .TryConsumeRevive();

        if (!revived)
        {
            return false;
        }

        Debug.Log(
            "Health: Fairy đã chặn Death."
        );

        return true;
    }

    // =====================================================
    // REVIVE FULL
    // =====================================================

    public void ReviveFull()
    {
        CacheComponents();

        /*
         * Nếu đang có Hurt coroutine khác
         * thì dừng lại.
         */
        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        if (reviveInvincibleCoroutine != null)
        {
            StopCoroutine(
                reviveInvincibleCoroutine
            );

            reviveInvincibleCoroutine = null;
        }

        // =================================================
        // HEALTH STATE
        // =================================================

        IsDead = false;

        currentHealth =
            maxHealth;

        isHurting = false;

        /*
         * Bất tử ngay khi vừa revive.
         */
        isInvincible = true;

        // =================================================
        // PHYSICS
        // =================================================

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

        // =================================================
        // PLAYER MOVEMENT
        // =================================================

        if (players != null)
        {
            players.enabled = true;

            players.StopAutoWalk();

            /*
             * Chỉ Unlock nếu không đang
             * chuyển scene/UI khóa Player.
             */
            UnlockPlayerAfterRevive();
        }

        // =================================================
        // ATTACK
        // =================================================

        if (attack != null)
        {
            attack.enabled = true;

            attack.CancelAttack();
        }

        // =================================================
        // DASH
        // =================================================

        if (dash != null)
        {
            dash.enabled = true;

            dash.CancelDash();
        }

        // =================================================
        // ANIMATOR
        // =================================================

        if (animator != null)
        {
            animator.speed = 1f;

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

            /*
             * Không Rebind ở đây.
             *
             * Fairy bắt Death trước khi
             * animation Death thực sự chạy,
             * nên không cần reset toàn Animator.
             */
        }

        // =================================================
        // UI
        // =================================================

        onPlayerHealed?.Invoke();

        /*
         * Một số Heart UI cũ có thể
         * đang subscribe event Damage,
         * nên gọi Notify để chắc chắn sync.
         */
        NotifyHealthChanged();

        // =================================================
        // INVINCIBILITY
        // =================================================

        reviveInvincibleCoroutine =
            StartCoroutine(
                FairyReviveInvincibilityRoutine()
            );

        Debug.Log(
            $"FAIRY REVIVE! HP: " +
            $"{currentHealth}/{maxHealth}"
        );
    }

    private IEnumerator
        FairyReviveInvincibilityRoutine()
    {
        isInvincible = true;

        float timer =
            Mathf.Max(
                0f,
                fairyReviveInvincibleTime
            );

        while (timer > 0f)
        {
            if (IsDead)
            {
                reviveInvincibleCoroutine =
                    null;

                yield break;
            }

            timer -=
                Time.deltaTime;

            yield return null;
        }

        if (!IsDead)
        {
            isInvincible =
                false;
        }

        reviveInvincibleCoroutine =
            null;
    }

    private void UnlockPlayerAfterRevive()
    {
        if (IsDead ||
            players == null)
        {
            return;
        }

        /*
         * Không Unlock trong lúc load scene.
         */
        bool sceneLoading =
            SceneLoader.Instance != null &&
            SceneLoader.Instance.IsLoading;

        if (sceneLoading)
            return;

        /*
         * Không Unlock nếu Skill Inventory
         * đang mở.
         */
        bool skillInventoryOpen =
            SkillInventoryUI.Instance != null &&
            SkillInventoryUI.Instance.IsOpen;

        if (skillInventoryOpen)
            return;

        players.UnlockControl();
    }

    // =====================================================
    // CANCEL ACTION
    // =====================================================

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

    // =====================================================
    // HURT MOVEMENT
    // =====================================================

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
         * Không mở điều khiển
         * khi đang load scene.
         */
        bool sceneLoading =
            SceneLoader.Instance != null &&
            SceneLoader.Instance.IsLoading;

        if (sceneLoading)
            return;

        /*
         * Không mở control nếu
         * Skill Inventory đang mở.
         */
        bool inventoryOpen =
            SkillInventoryUI.Instance != null &&
            SkillInventoryUI.Instance.IsOpen;

        if (inventoryOpen)
            return;

        players.UnlockControl();
    }

    // =====================================================
    // ANIMATION EVENT - HURT
    // =====================================================

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
            isInvincible =
                false;
        }
    }

    // =====================================================
    // HEAL
    // =====================================================

    public bool Heal(
        float amount)
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
                currentHealth +
                amount,
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

    // =====================================================
    // MAX HEALTH
    // =====================================================

    public bool IncreaseMaxHealth(
        int amount,
        bool healToFull = true)
    {
        if (amount <= 0)
            return false;

        int oldMaxHealth =
            maxHealth;

        maxHealth +=
            amount;

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
            $"HP hiện tại: " +
            $"{currentHealth}/{maxHealth}"
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

    // =====================================================
    // DEATH
    // =====================================================

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

        if (reviveInvincibleCoroutine != null)
        {
            StopCoroutine(
                reviveInvincibleCoroutine
            );

            reviveInvincibleCoroutine =
                null;
        }

        CancelCurrentActions();

        // =================================================
        // DASH
        // =================================================

        if (dash != null)
        {
            dash.CancelDash();

            dash.enabled =
                false;
        }

        // =================================================
        // ATTACK
        // =================================================

        if (attack != null)
        {
            attack.CancelAttack();

            attack.enabled =
                false;
        }

        // =================================================
        // PLAYER
        // =================================================

        if (players != null)
        {
            players.StopAutoWalk();

            players.LockControl();

            players.enabled =
                false;
        }

        // =================================================
        // PHYSICS
        // =================================================

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

        // =================================================
        // ANIMATOR
        // =================================================

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

        // =================================================
        // AUDIO
        // =================================================

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

    // =====================================================
    // DEATH ANIMATION FINISHED
    // =====================================================

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

    // =====================================================
    // RESET HEALTH
    // =====================================================

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

        reviveInvincibleCoroutine =
            null;

        // =================================================
        // PHYSICS
        // =================================================

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

        // =================================================
        // PLAYER
        // =================================================

        if (players != null)
        {
            players.enabled =
                true;

            players.StopAutoWalk();

            players.UnlockControl();
        }

        // =================================================
        // ATTACK
        // =================================================

        if (attack != null)
        {
            attack.enabled =
                true;

            attack.CancelAttack();
        }

        // =================================================
        // DASH
        // =================================================

        if (dash != null)
        {
            dash.enabled =
                true;

            dash.CancelDash();
        }

        // =================================================
        // ANIMATOR
        // =================================================

        if (animator != null)
        {
            animator.speed = 1f;

            animator.Rebind();

            animator.Update(
                0f
            );

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
            isInvincible =
                false;
        }
    }

    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        if (reviveInvincibleCoroutine != null)
        {
            StopCoroutine(
                reviveInvincibleCoroutine
            );

            reviveInvincibleCoroutine =
                null;
        }

        KeepPlayerStopped();
    }

    // =====================================================
    // VALIDATE
    // =====================================================

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

        fairyReviveInvincibleTime =
            Mathf.Max(
                0f,
                fairyReviveInvincibleTime
            );
    }
}