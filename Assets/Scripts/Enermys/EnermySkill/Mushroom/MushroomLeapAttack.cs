using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MushroomLeapAttack :
    EnermyAttackBase
{
    [Header("Leap")]
    [Tooltip("Thời gian chuẩn bị trước khi lao.")]
    [Min(0f)]
    [SerializeField]
    private float windupTime = 0.3f;

    [Tooltip("Tốc độ Mushroom lao tới.")]
    [Min(0f)]
    [SerializeField]
    private float leapSpeed = 7f;

    [Tooltip("Thời gian Mushroom lao.")]
    [Min(0.05f)]
    [SerializeField]
    private float leapDuration = 0.35f;

    [Tooltip(
        "Dùng Animation Event StartLeap " +
        "để bắt đầu lao."
    )]
    [SerializeField]
    private bool useAnimationEvent;

    [Tooltip(
        "Nếu Animation Event bị thiếu, " +
        "Mushroom tự lao sau thời gian này."
    )]
    [Min(0.05f)]
    [SerializeField]
    private float animationEventTimeout = 0.6f;

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 2;

    [Min(0f)]
    [SerializeField]
    private float hitRadius = 0.55f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Knockback Target")]
    [Min(0f)]
    [SerializeField]
    private float targetKnockback = 0f;

    private Vector2 lockedDirection =
        Vector2.down;

    private bool leapStarted;
    private bool isLeaping;

    private readonly HashSet<GameObject>
        damagedTargets =
            new HashSet<GameObject>();

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        health = GetComponent<EnermyHealth>();
    }

    protected override bool CanStartAttack()
    {
        if (!base.CanStartAttack())
            return false;

        if (IsDeadOrHurt())
            return false;
        return true;
    }

    protected override void OnAttackStarted()
    {
        leapStarted = false;
        isLeaping = false;

        damagedTargets.Clear();

        lockedDirection =
            GetDirectionToTarget();

        if (lockedDirection.sqrMagnitude <=
            0.001f)
        {
            lockedDirection =
                transform.localScale.x < 0f
                    ? Vector2.left
                    : Vector2.right;
        }

        lockedDirection.Normalize();
    }

    protected override IEnumerator PerformAttack()
    {
        if (useAnimationEvent)
        {
            float waitTimer =
                Mathf.Max(
                    0.05f,
                    animationEventTimeout
                );

            while (!leapStarted &&
                   waitTimer > 0f &&
                   isAttacking)
            {
                waitTimer -= Time.deltaTime;
                yield return null;
            }

            /*
             * Fallback nếu clip Attack thiếu Event.
             */
            if (!leapStarted &&
                isAttacking)
            {
                StartLeap();
            }
        }
        else
        {
            yield return new WaitForSeconds(
                Mathf.Max(
                    0f,
                    windupTime
                )
            );

            StartLeap();
        }

        float leapTimer =
            Mathf.Max(
                0.05f,
                leapDuration
            );

        while (isLeaping &&
               isAttacking &&
               leapTimer > 0f)
        {
            if (IsDeadOrHurt())
                break;

            if (rb != null)
            {
                rb.linearVelocity =
                    lockedDirection *
                    Mathf.Max(
                        0f,
                        leapSpeed
                    );
            }

            CheckHit();

            leapTimer -= Time.deltaTime;

            yield return null;
        }

        StopLeap();
    }

    /*
     * Animation Event đặt tại frame Mushroom
     * bắt đầu lao về phía trước.
     */
    public void StartLeap()
    {
        if (!isAttacking ||
            leapStarted)
        {
            return;
        }

        if (IsDeadOrHurt())
            return;

        leapStarted = true;
        isLeaping = true;

        if (movement != null)
        {
            movement.StopMove();
        }

        if (rb != null)
        {
            rb.linearVelocity =
                lockedDirection *
                Mathf.Max(
                    0f,
                    leapSpeed
                );
        }
    }

    private void CheckHit()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                Mathf.Max(
                    0f,
                    hitRadius
                ),
                targetLayer
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            GameObject targetRoot =
                hit.transform.root.gameObject;

            if (!damagedTargets.Add(
                    targetRoot))
            {
                continue;
            }

            CloneHealth cloneHealth =
                hit.GetComponentInParent<
                    CloneHealth
                >();

            if (cloneHealth != null)
            {
                cloneHealth.TakeDamage(
                    damage
                );

                continue;
            }

            Health playerHealth =
                hit.GetComponentInParent<
                    Health
                >();

            if (playerHealth != null &&
                !playerHealth.IsDead)
            {
                playerHealth.TakeDamage(
                    damage
                );
            }
        }
    }

    private void StopLeap()
    {
        isLeaping = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }

    protected override void FinishAttack()
    {
        StopLeap();

        damagedTargets.Clear();

        base.FinishAttack();
    }

    public override void EndAttack()
    {
        StopLeap();

        damagedTargets.Clear();

        base.EndAttack();
    }

    public override void CancelAttack()
    {
        StopLeap();

        leapStarted = false;
        damagedTargets.Clear();

        base.CancelAttack();
    }

    protected override void OnDisable()
    {
        StopLeap();

        damagedTargets.Clear();

        base.OnDisable();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                attackRange
            )
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                hitRadius
            )
        );
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        windupTime =
            Mathf.Max(
                0f,
                windupTime
            );

        leapSpeed =
            Mathf.Max(
                0f,
                leapSpeed
            );

        leapDuration =
            Mathf.Max(
                0.05f,
                leapDuration
            );

        animationEventTimeout =
            Mathf.Max(
                0.05f,
                animationEventTimeout
            );

        damage =
            Mathf.Max(
                1,
                damage
            );

        hitRadius =
            Mathf.Max(
                0f,
                hitRadius
            );

        targetKnockback =
            Mathf.Max(
                0f,
                targetKnockback
            );
    }
}