using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingSlamAttack : EnermyAttackBase
{
    [Header("Slam Movement")]
    [Tooltip("Khoảng thời gian chuẩn bị trước khi lao xuống.")]
    [Min(0f)]
    [SerializeField]
    private float windupTime = 0.35f;

    [Tooltip("Khoảng cách bay lên trước khi bổ nhào.")]
    [Min(0f)]
    [SerializeField]
    private float riseHeight = 0.7f;

    [Tooltip("Tốc độ bay lên.")]
    [Min(0f)]
    [SerializeField]
    private float riseSpeed = 4f;

    [Tooltip("Tốc độ lao tới vị trí mục tiêu.")]
    [Min(0f)]
    [SerializeField]
    private float slamSpeed = 9f;

    [Tooltip("Khoảng cách xem như đã chạm điểm đáp.")]
    [Min(0.01f)]
    [SerializeField]
    private float landingDistance = 0.12f;

    [Tooltip("Thời gian tối đa của pha lao xuống.")]
    [Min(0.05f)]
    [SerializeField]
    private float maximumSlamTime = 0.8f;

    [Header("Animation Event")]
    [SerializeField]
    private bool useAnimationEvent;

    [Tooltip(
        "Nếu Animation Event StartSlam bị thiếu, " +
        "Flying tự lao sau thời gian này."
    )]
    [Min(0.05f)]
    [SerializeField]
    private float animationEventTimeout = 0.7f;

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 2;

    [Min(0f)]
    [SerializeField]
    private float damageRadius = 0.7f;

    [SerializeField]
    private LayerMask targetLayer;

    [Header("Hit Effect")]
    [SerializeField]
    private GameObject hitEffectPrefab;

    [Min(0.05f)]
    [SerializeField]
    private float hitEffectLifeTime = 0.6f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip slamSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slamVolume = 0.8f;

    private Vector2 startPosition;
    private Vector2 risePosition;
    private Vector2 lockedTargetPosition;

    private bool slamStarted;
    private bool landed;

    private readonly HashSet<GameObject>
        damagedTargets =
            new HashSet<GameObject>();

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
        slamStarted = false;
        landed = false;

        damagedTargets.Clear();

        startPosition =
            rb != null
                ? rb.position
                : (Vector2)transform.position;

        risePosition =
            startPosition +
            Vector2.up *
            Mathf.Max(
                0f,
                riseHeight
            );

        if (movement != null &&
            movement.HasTarget() &&
            movement.Target != null)
        {
            lockedTargetPosition =
                movement.Target.position;
        }
        else
        {
            lockedTargetPosition =
                startPosition;
        }
    }

    protected override IEnumerator PerformAttack()
    {
        if (rb == null)
            yield break;

        /*
         * Pha chuẩn bị và bay lên nhẹ.
         */
        float windupTimer =
            Mathf.Max(
                0f,
                windupTime
            );

        while (windupTimer > 0f &&
               isAttacking &&
               !IsDeadOrHurt())
        {
            rb.position =
                Vector2.MoveTowards(
                    rb.position,
                    risePosition,
                    Mathf.Max(
                        0f,
                        riseSpeed
                    ) *
                    Time.deltaTime
                );

            rb.linearVelocity =
                Vector2.zero;

            windupTimer -=
                Time.deltaTime;

            yield return null;
        }

        if (!isAttacking ||
            IsDeadOrHurt())
        {
            yield break;
        }

        if (useAnimationEvent)
        {
            float eventTimer =
                Mathf.Max(
                    0.05f,
                    animationEventTimeout
                );

            while (!slamStarted &&
                   eventTimer > 0f &&
                   isAttacking &&
                   !IsDeadOrHurt())
            {
                eventTimer -=
                    Time.deltaTime;

                yield return null;
            }

            if (!slamStarted &&
                isAttacking &&
                !IsDeadOrHurt())
            {
                StartSlam();
            }
        }
        else
        {
            StartSlam();
        }

        float slamTimer =
            Mathf.Max(
                0.05f,
                maximumSlamTime
            );

        while (slamStarted &&
               !landed &&
               isAttacking &&
               !IsDeadOrHurt() &&
               slamTimer > 0f)
        {
            Vector2 nextPosition =
                Vector2.MoveTowards(
                    rb.position,
                    lockedTargetPosition,
                    Mathf.Max(
                        0f,
                        slamSpeed
                    ) *
                    Time.deltaTime
                );

            rb.MovePosition(
                nextPosition
            );

            float distance =
                Vector2.Distance(
                    nextPosition,
                    lockedTargetPosition
                );

            if (distance <=
                Mathf.Max(
                    0.01f,
                    landingDistance
                ))
            {
                Land();
                break;
            }

            slamTimer -=
                Time.deltaTime;

            yield return null;
        }

        if (!landed &&
            isAttacking &&
            !IsDeadOrHurt())
        {
            Land();
        }
    }

    // Animation Event tại frame bắt đầu bổ nhào
    public void StartSlam()
    {
        if (!isAttacking ||
            slamStarted ||
            IsDeadOrHurt())
        {
            return;
        }

        slamStarted = true;

        if (movement != null &&
            movement.HasTarget() &&
            movement.Target != null)
        {
            lockedTargetPosition =
                movement.Target.position;
        }
    }

    private void Land()
    {
        if (landed)
            return;

        landed = true;
        slamStarted = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.position =
                lockedTargetPosition;
        }

        SpawnHitEffect();
        PlaySlamSound();
        DealSlamDamage();
    }

    private void DealSlamDamage()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                Mathf.Max(
                    0f,
                    damageRadius
                ),
                targetLayer
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            GameObject rootObject =
                hit.transform.root.gameObject;

            if (!damagedTargets.Add(
                    rootObject))
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

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity
            );

        Destroy(
            effect,
            Mathf.Max(
                0.05f,
                hitEffectLifeTime
            )
        );
    }

    private void PlaySlamSound()
    {
        if (slamSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                slamSound
            );
        }
        else
        {
            AudioSource.PlayClipAtPoint(
                slamSound,
                transform.position,
                slamVolume
            );
        }
    }

    protected override void FinishAttack()
    {
        StopSlam();

        damagedTargets.Clear();

        base.FinishAttack();
    }

    public override void EndAttack()
    {
        StopSlam();

        damagedTargets.Clear();

        base.EndAttack();
    }

    public override void CancelAttack()
    {
        StopSlam();

        damagedTargets.Clear();

        base.CancelAttack();
    }

    private void StopSlam()
    {
        slamStarted = false;
        landed = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }
    }

    protected override void OnDisable()
    {
        StopSlam();

        damagedTargets.Clear();

        base.OnDisable();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                attackRange
            )
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                damageRadius
            )
        );

        if (Application.isPlaying)
        {
            Gizmos.color =
                Color.cyan;

            Gizmos.DrawWireSphere(
                lockedTargetPosition,
                0.15f
            );
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        windupTime =
            Mathf.Max(
                0f,
                windupTime
            );

        riseHeight =
            Mathf.Max(
                0f,
                riseHeight
            );

        riseSpeed =
            Mathf.Max(
                0f,
                riseSpeed
            );

        slamSpeed =
            Mathf.Max(
                0f,
                slamSpeed
            );

        landingDistance =
            Mathf.Max(
                0.01f,
                landingDistance
            );

        maximumSlamTime =
            Mathf.Max(
                0.05f,
                maximumSlamTime
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

        damageRadius =
            Mathf.Max(
                0f,
                damageRadius
            );

        hitEffectLifeTime =
            Mathf.Max(
                0.05f,
                hitEffectLifeTime
            );

        slamVolume =
            Mathf.Clamp01(
                slamVolume
            );
    }
}