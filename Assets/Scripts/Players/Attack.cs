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
    public bool IsAttacking => isAttacking;

    [Header("Hitbox")]
    public Transform[] attackPoint;
    public LayerMask enermyLayer;

    [Header("Attack")]
    public float attackRadius = 0.6f;
    public float attackDistance = 0.6f;
    public float attackCooldown = 0.35f;
    public int damage = 20;

    [Header("Combo")]
    [SerializeField]
    private int combo;

    public int maxCombo = 3;

    private bool queueNextAttack;
    private bool comboWindowOpen;

    [Header("Lunge")]
    public float lungeForce = 2f;
    public float lungeTime = 0.06f;

    public float[] comboLunge =
    {
        3.5f,
        5f,
        8f
    };

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

    [Header("Combo Damage")]
    public int[] comboDamage =
    {
        20,
        25,
        35
    };

    [Header("Combo Knockback")]
    public float[] comboKnockback =
    {
        4f,
        6f,
        8f
    };

    public float comboFinishDelay = 0.45f;

    [Header("Ability")]
    public AbilityData skillData;
    public SkillSlotUI slotUI;

    private Coroutine lungeCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Players>();
        dash = GetComponent<PlayerDash>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        if (slotUI != null &&
            skillData != null)
        {
            slotUI.Setup(skillData);
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

        /*
         * Không còn chặn Attack khi đang Dash.
         */
        if (!Input.GetKeyDown(KeyCode.J))
            return;

        if (!isAttacking)
        {
            if (AbilityManager.Instance == null)
                return;

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

            /*
             * Attack animation được ưu tiên.
             * Dash movement vẫn tiếp tục chạy.
             */
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
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
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.attackSound
            );
        }
    }

    // Animation Event
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

        /*
         * Chỉ Lunge khi không Dash.
         *
         * Trong lúc Dash:
         * - Attack vẫn gây damage.
         * - Không chạy coroutine Lunge.
         * - Dash tiếp tục giữ velocity.
         */
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

            foreach (Collider2D hit in hits)
            {
                EnermyHealth enemyHealth =
                    hit.GetComponentInParent<
                        EnermyHealth
                    >();

                if (enemyHealth == null)
                    continue;

                Vector2 knockbackDirection =
                    (
                        enemyHealth.transform.position -
                        transform.position
                    ).normalized;

                enemyHealth.knockbackForce =
                    GetArrayValue(
                        comboKnockback,
                        currentCombo,
                        4f
                    );

                enemyHealth.TakeDamage(
                    GetArrayValue(
                        comboDamage,
                        currentCombo,
                        damage
                    ),
                    knockbackDirection
                );
            }
        }
    }

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
            /*
             * Nếu Dash bắt đầu giữa Lunge,
             * dừng Lunge ngay và để Dash xử lý velocity.
             */
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

            timer -= Time.deltaTime;

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

    // Animation Event cuối animation
    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (animator != null)
            animator.speed = 1f;

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

        /*
         * Không được đặt velocity về 0
         * nếu Player đang Dash.
         */
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
            animator.ResetTrigger("Attack");
            animator.SetInteger("Combo", 0);
        }
    }

    public void OpenComboWindow()
    {
        if (isAttacking)
            comboWindowOpen = true;
    }

    public void CloseComboWindow()
    {
        comboWindowOpen = false;
    }

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