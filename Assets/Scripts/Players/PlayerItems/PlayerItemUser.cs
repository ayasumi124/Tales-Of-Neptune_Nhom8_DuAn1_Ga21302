using System.Collections;
using UnityEngine;

public class PlayerItemUser : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Health health;

    [SerializeField]
    private PlayerMana mana;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Attack attack;

    [SerializeField]
    private PlayerDash dash;

    [SerializeField]
    private Rigidbody2D rb;

    [Header("Animation")]
    [SerializeField]
    private string useTrigger = "UseItem";

    private bool isUsingItem;

    private Coroutine useCoroutine;

    public bool IsUsingItem =>
        isUsingItem;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        CacheComponents();
    }

    private void CacheComponents()
    {
        if (health == null)
        {
            health =
                GetComponent<Health>();
        }

        if (mana == null)
        {
            mana =
                GetComponent<PlayerMana>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
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

        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }
    }

    // =====================================================
    // USE ITEM
    // =====================================================

    public bool TryUse(
        ItemData item)
    {
        if (item == null)
            return false;

        if (!item.Usable)
            return false;

        if (isUsingItem)
            return false;

        if (health != null &&
            health.IsDead)
        {
            return false;
        }

        if (!CanUseItem(item))
            return false;

        /*
         * QUAN TRỌNG:
         *
         * Nếu Player vừa Attack rồi mở Inventory
         * và dùng Potion/Heart Container,
         * Animation Event EndAttack có thể chưa chạy.
         *
         * Chủ động reset Attack trước khi dùng item.
         */
        PrepareForItemUse();

        useCoroutine =
            StartCoroutine(
                UseRoutine(item)
            );

        return true;
    }

    // =====================================================
    // PREPARE
    // =====================================================

    private void PrepareForItemUse()
    {
        /*
         * Hủy Attack hoàn toàn.
         *
         * CancelAttack() đã reset:
         * - isAttacking
         * - combo
         * - combo window
         * - lunge
         * - Animator speed
         * - Attack trigger
         */
        if (attack != null)
        {
            attack.CancelAttack();
        }

        /*
         * Nếu đang Dash thì cũng hủy.
         */
        if (dash != null &&
            dash.IsDashing)
        {
            dash.CancelDash();
        }

        /*
         * Xóa velocity còn sót lại từ
         * Attack Lunge.
         */
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        /*
         * Đảm bảo Animator không còn tốc độ
         * của combo Attack.
         */
        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                useTrigger
            );
        }
    }

    // =====================================================
    // CAN USE
    // =====================================================

    private bool CanUseItem(
        ItemData item)
    {
        switch (item.EffectType)
        {
            // ---------------------------------------------
            // HEALTH POTION
            // ---------------------------------------------

            case ItemEffectType.RestoreHealth:

                if (health == null)
                {
                    Debug.LogError(
                        "PlayerItemUser không tìm thấy Health."
                    );

                    return false;
                }

                if (health.IsHealthFull())
                {
                    Debug.Log(
                        "Máu đã đầy."
                    );

                    return false;
                }

                return true;

            // ---------------------------------------------
            // MANA POTION
            // ---------------------------------------------

            case ItemEffectType.RestoreMana:

                if (mana == null)
                {
                    Debug.LogError(
                        "PlayerItemUser không tìm thấy PlayerMana."
                    );

                    return false;
                }

                if (mana.IsManaFull())
                {
                    Debug.Log(
                        "Mana đã đầy."
                    );

                    return false;
                }

                return true;

            // ---------------------------------------------
            // HEART CONTAINER
            // ---------------------------------------------

            case ItemEffectType.IncreaseMaxHealth:

                if (health == null)
                {
                    Debug.LogError(
                        "PlayerItemUser không tìm thấy Health."
                    );

                    return false;
                }

                return true;
        }

        return false;
    }

    // =====================================================
    // USE ROUTINE
    // =====================================================

    private IEnumerator UseRoutine(
        ItemData item)
    {
        isUsingItem = true;

        // =============================================
        // ANIMATION
        // =============================================

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                useTrigger
            );

            animator.SetTrigger(
                useTrigger
            );
        }

        // =============================================
        // EFFECT DELAY
        // =============================================

        float useDuration =
            Mathf.Max(
                0f,
                item.UseDuration
            );

        float effectDelay =
            Mathf.Clamp(
                item.EffectDelay,
                0f,
                useDuration
            );

        /*
         * Inventory thường Time.timeScale = 0.
         *
         * Vì vậy PHẢI dùng Realtime.
         */
        if (effectDelay > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    effectDelay
                );
        }

        // =============================================
        // APPLY EFFECT
        // =============================================

        bool success =
            ApplyEffect(item);

        Debug.Log(
            $"Dùng {item.ItemName}. " +
            $"Effect = {item.EffectType}. " +
            $"Thành công = {success}"
        );

        // =============================================
        // SOUND
        // =============================================

        if (success &&
            item.UseSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                item.UseSound
            );
        }

        // =============================================
        // REMAINING DURATION
        // =============================================

        float remainingTime =
            Mathf.Max(
                0f,
                useDuration -
                effectDelay
            );

        if (remainingTime > 0f)
        {
            yield return
                new WaitForSecondsRealtime(
                    remainingTime
                );
        }

        FinishItemUse();
    }

    // =====================================================
    // APPLY EFFECT
    // =====================================================

    private bool ApplyEffect(
        ItemData item)
    {
        if (item == null)
            return false;

        switch (item.EffectType)
        {
            // ---------------------------------------------
            // HEALTH POTION
            // ---------------------------------------------

            case ItemEffectType.RestoreHealth:

                if (health == null)
                    return false;

                return health.Heal(
                    item.EffectValue
                );

            // ---------------------------------------------
            // MANA POTION
            // ---------------------------------------------

            case ItemEffectType.RestoreMana:

                if (mana == null)
                    return false;

                return mana.RestoreMana(
                    item.EffectValue
                );

            // ---------------------------------------------
            // HEART CONTAINER
            // ---------------------------------------------

            case ItemEffectType.IncreaseMaxHealth:

                if (health == null)
                    return false;

                return health.IncreaseMaxHealth(
                    Mathf.RoundToInt(
                        item.EffectValue
                    ),
                    true
                );
        }

        return false;
    }

    // =====================================================
    // FINISH
    // =====================================================

    private void FinishItemUse()
    {
        isUsingItem = false;

        useCoroutine = null;

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(
                useTrigger
            );
        }

        /*
         * Xóa velocity còn sót lại.
         */
        if (rb != null &&
            (dash == null ||
             !dash.IsDashing))
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }

    // =====================================================
    // CANCEL
    // =====================================================

    public void CancelItemUse()
    {
        if (useCoroutine != null)
        {
            StopCoroutine(
                useCoroutine
            );

            useCoroutine = null;
        }

        isUsingItem = false;

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(
                useTrigger
            );
        }

        if (rb != null &&
            (dash == null ||
             !dash.IsDashing))
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }

    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        CancelItemUse();
    }
}