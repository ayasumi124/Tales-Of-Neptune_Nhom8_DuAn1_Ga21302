using System.Collections;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;
    private Players player;
    private PlayerDash dash;
    private Health health;

    private bool isAttacking;

    public bool IsAttacking =>
        isAttacking;

    // =====================================================
    // HITBOX
    // =====================================================

    [Header("Hitbox")]
    public Transform[] attackPoint;
    public LayerMask enermyLayer;

    // =====================================================
    // ATTACK
    // =====================================================

    [Header("Attack")]
    public float attackRadius = 0.6f;
    public float attackDistance = 0.6f;
    public float attackCooldown = 0.35f;

    [Tooltip(
        "Damage mặc định nếu Combo Damage không hợp lệ."
    )]
    public int damage = 20;

    // =====================================================
    // COMBO
    // =====================================================

    [Header("Combo")]
    [SerializeField]
    private int combo;

    public int maxCombo = 3;

    private bool queueNextAttack;
    private bool comboWindowOpen;

    // =====================================================
    // LUNGE
    // =====================================================

    [Header("Lunge")]
    public float lungeForce = 2f;
    public float lungeTime = 0.06f;

    public float[] comboLunge =
    {
        3.5f,
        5f,
        8f
    };

    // =====================================================
    // COMBO SPEED
    // =====================================================

    [Header("Combo Speed")]
    public float[] comboCooldown =
    {
        0.22f,
        0.16f,
        0.12f
    };

    public float[] comboAnimationSpeed =
    {
        1.4f,
        1.7f,
        2f
    };

    // =====================================================
    // COMBO DAMAGE
    // =====================================================

    [Header("Combo Damage")]
    public int[] comboDamage =
    {
        20,
        25,
        35
    };

    // =====================================================
    // COMBO KNOCKBACK
    // =====================================================

    [Header("Combo Knockback")]
    public float[] comboKnockback =
    {
        4f,
        6f,
        8f
    };

    public float comboFinishDelay = 0.45f;

    // =====================================================
    // ABILITY
    // =====================================================

    [Header("Ability")]
    public AbilityData skillData;
    public SkillSlotUI slotUI;

    private Coroutine lungeCoroutine;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        rb =
            GetComponent<Rigidbody2D>();

        player =
            GetComponent<Players>();

        dash =
            GetComponent<PlayerDash>();

        health =
            GetComponent<Health>();
    }

    private void Start()
    {
        if (slotUI != null &&
            skillData != null)
        {
            slotUI.Setup(
                skillData
            );
        }

        isAttacking = false;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (health != null &&
            (health.IsDead ||
             health.IsHurting))
        {
            return;
        }

        if (!Input.GetKeyDown(
                KeyCode.J))
        {
            return;
        }

        if (!isAttacking)
        {
            if (AbilityManager.Instance ==
                null)
            {
                return;
            }

            if (AbilityManager.Instance
                    .attack.cooldown > 0f)
            {
                return;
            }

            StartAttack();
        }
        else if (comboWindowOpen)
        {
            queueNextAttack = true;
        }
    }

    // =====================================================
    // START ATTACK
    // =====================================================

    private void StartAttack()
    {
        if (health != null &&
            (health.IsDead ||
             health.IsHurting))
        {
            return;
        }

        queueNextAttack = false;
        comboWindowOpen = false;
        isAttacking = true;

        int index =
            Mathf.Clamp(
                combo,
                0,
                maxCombo - 1
            );

        float animationSpeed =
            GetArrayValue(
                comboAnimationSpeed,
                index,
                1f
            );

        if (animator != null)
        {
            animator.speed =
                animationSpeed;

            animator.SetInteger(
                "Combo",
                combo
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.SetTrigger(
                "Attack"
            );
        }

        float cooldown =
            GetArrayValue(
                comboCooldown,
                index,
                attackCooldown
            );

        if (AbilityManager.Instance != null)
        {
            AbilityManager.Instance
                .attack.cooldown =
                cooldown;

            AbilityManager.Instance
                .attack.maxCooldown =
                cooldown;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlaySFX(
                    AudioManager.Instance
                        .attackSound
                );
        }
    }

    // =====================================================
    // DAMAGE
    // =====================================================

    public void DealDamage()
    {
        if (!isAttacking)
            return;

        int currentCombo =
            Mathf.Clamp(
                combo,
                0,
                maxCombo - 1
            );

        // =============================================
        // LUNGE
        // =============================================

        if (dash == null ||
            !dash.IsDashing)
        {
            if (lungeCoroutine != null)
            {
                StopCoroutine(
                    lungeCoroutine
                );
            }

            Vector2 direction =
                player != null
                    ? player.LastDirection
                    : Vector2.down;

            float lungeSpeed =
                GetArrayValue(
                    comboLunge,
                    currentCombo,
                    lungeForce
                );

            lungeCoroutine =
                StartCoroutine(
                    Lunge(
                        direction,
                        lungeSpeed
                    )
                );
        }

        // =============================================
        // DAMAGE CƠ BẢN CỦA COMBO
        // =============================================

        float baseComboDamage =
            GetArrayValue(
                comboDamage,
                currentCombo,
                damage
            );

        // =============================================
        // EQUIPMENT ATTACK BONUS
        // =============================================

        float equipmentAttackBonus =
            0f;

        if (PlayerEquipmentManager.Instance !=
            null)
        {
            equipmentAttackBonus =
                PlayerEquipmentManager.Instance
                    .GetAttackBonus();
        }

        int finalDamage =
    Mathf.RoundToInt(
        baseComboDamage +
        equipmentAttackBonus
    );

        // =============================================
        // HITBOX
        // =============================================

        foreach (Transform point
                 in attackPoint)
        {
            if (point == null)
                continue;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
                    point.position,
                    attackRadius,
                    enermyLayer
                );

            foreach (Collider2D hit
                     in hits)
            {
                EnermyHealth enemyHealth =
                    hit.GetComponentInParent<
                        EnermyHealth
                    >();

                if (enemyHealth == null)
                    continue;

                Vector2 knockbackDirection =
                    (
                        enemyHealth
                            .transform.position -
                        transform.position
                    ).normalized;

                float knockbackStrength =
                    GetArrayValue(
                        comboKnockback,
                        currentCombo,
                        2.5f
                    );

                enemyHealth.TakeDamage(
                    finalDamage,
                    knockbackDirection,
                    knockbackStrength,
                    true
                );
            }
        }
    }

    // =====================================================
    // LUNGE
    // =====================================================

    private IEnumerator Lunge(
        Vector2 direction,
        float speed)
    {
        direction =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        float timer =
            Mathf.Max(
                0.01f,
                lungeTime
            );

        while (timer > 0f)
        {
            if (dash != null &&
                dash.IsDashing)
            {
                lungeCoroutine = null;
                yield break;
            }

            if (rb != null)
            {
                rb.linearVelocity =
                    direction * speed;
            }

            timer -=
                Time.deltaTime;

            yield return null;
        }

        if (rb != null &&
            (dash == null ||
             !dash.IsDashing))
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        lungeCoroutine = null;
    }

    // =====================================================
    // END ATTACK
    // =====================================================

    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (animator != null)
        {
            animator.speed = 1f;
        }

        comboWindowOpen = false;

        if (queueNextAttack)
        {
            queueNextAttack = false;

            combo++;

            if (combo >= maxCombo)
            {
                combo = 0;

                if (AbilityManager.Instance != null)
                {
                    AbilityManager.Instance
                        .attack.maxCooldown =
                        comboFinishDelay;

                    AbilityManager.Instance
                        .attack.cooldown =
                        comboFinishDelay;
                }

                return;
            }

            StartAttack();
            return;
        }

        if (rb != null &&
            (dash == null ||
             !dash.IsDashing))
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        combo = 0;
    }

    // =====================================================
    // CANCEL
    // =====================================================

    public void CancelAttack()
    {
        isAttacking = false;

        combo = 0;

        queueNextAttack = false;

        comboWindowOpen = false;

        if (lungeCoroutine != null)
        {
            StopCoroutine(
                lungeCoroutine
            );

            lungeCoroutine = null;
        }

        if (rb != null &&
            (dash == null ||
             !dash.IsDashing))
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        if (animator != null)
        {
            animator.speed = 1f;

            animator.ResetTrigger(
                "Attack"
            );

            animator.SetInteger(
                "Combo",
                0
            );
        }
    }

    // =====================================================
    // COMBO WINDOW
    // =====================================================

    public void OpenComboWindow()
    {
        if (isAttacking)
        {
            comboWindowOpen = true;
        }
    }

    public void CloseComboWindow()
    {
        comboWindowOpen = false;
    }

    // =====================================================
    // GET FINAL DAMAGE
    // =====================================================

    public float GetCurrentAttackDamage()
    {
        int currentCombo =
            Mathf.Clamp(
                combo,
                0,
                maxCombo - 1
            );

        float baseComboDamage =
            GetArrayValue(
                comboDamage,
                currentCombo,
                damage
            );

        float bonus = 0f;

        if (PlayerEquipmentManager.Instance !=
            null)
        {
            bonus =
                PlayerEquipmentManager.Instance
                    .GetAttackBonus();
        }

        return baseComboDamage + bonus;
    }

    // =====================================================
    // ARRAY HELPERS
    // =====================================================

    private float GetArrayValue(
        float[] values,
        int index,
        float fallback)
    {
        if (values == null ||
            values.Length == 0)
        {
            return fallback;
        }

        index =
            Mathf.Clamp(
                index,
                0,
                values.Length - 1
            );

        return values[index];
    }

    private int GetArrayValue(
        int[] values,
        int index,
        int fallback)
    {
        if (values == null ||
            values.Length == 0)
        {
            return fallback;
        }

        index =
            Mathf.Clamp(
                index,
                0,
                values.Length - 1
            );

        return values[index];
    }

    private void OnDisable()
    {
        CancelAttack();
    }
}