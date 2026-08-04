using System.Collections.Generic;
using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private EnermyMovement movement;

    [SerializeField]
    private Transform attackPoint;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private EnermyAudio enemyAudio;

    [Header("Target Layer")]
    [SerializeField]
    private LayerMask playerLayer;

    [Header("Attack")]
    [Min(0f)]
    [SerializeField]
    private float attackDistance = 0.6f;

    [Min(0f)]
    [SerializeField]
    private float attackRadius = 0.35f;

    [Min(0.05f)]
    [SerializeField]
    private float attackCooldown = 1f;

    [Min(1)]
    [SerializeField]
    private int damage = 1;

    [Header("Animator")]
    [SerializeField]
    private string attackTrigger = "Attack";

    private bool isAttacking;
    private float cooldownTimer;

    private Vector2 attackDirection =
        Vector2.down;

    private readonly HashSet<GameObject>
        damagedTargets =
            new HashSet<GameObject>();

    public bool IsAttacking =>
        isAttacking;

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        CacheComponents();
    }

    private void Update()
    {
        UpdateCooldown();

        if (movement == null)
            return;

        if (!movement.HasTarget())
        {
            if (isAttacking)
            {
                CancelAttack();
            }

            return;
        }

        if (isAttacking)
        {
            movement.StopMove();
            return;
        }

        movement.FaceTarget();

        float distance =
            movement.DistanceToTarget();

        if (distance >
            movement.attackRange)
        {
            movement.CanMove = true;
            return;
        }

        movement.StopMove();

        if (cooldownTimer > 0f)
            return;

        BeginAttack();
    }

    private void CacheComponents()
    {
        if (movement == null)
        {
            movement =
                GetComponent<EnermyMovement>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (enemyAudio == null)
        {
            enemyAudio =
                GetComponent<EnermyAudio>();
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownTimer <= 0f)
            return;

        cooldownTimer -=
            Time.deltaTime;

        if (cooldownTimer < 0f)
        {
            cooldownTimer = 0f;
        }
    }

    private void BeginAttack()
    {
        if (isAttacking ||
            movement == null ||
            !movement.HasTarget())
        {
            return;
        }

        isAttacking = true;

        damagedTargets.Clear();

        movement.CanMove = false;
        movement.StopMove();
        movement.FaceTarget();

        attackDirection =
            movement.DirectionToTarget();

        UpdateAttackPoint(
            attackDirection
        );

        if (enemyAudio != null)
        {
            enemyAudio.PlayAttack();
        }

        if (animator != null)
        {
            animator.ResetTrigger(
                attackTrigger
            );

            animator.SetTrigger(
                attackTrigger
            );
        }
        else
        {
            DealDamage();
            EndAttack();
        }
    }

    private void UpdateAttackPoint(
        Vector2 direction)
    {
        if (attackPoint == null)
            return;

        if (direction.sqrMagnitude <=
            0.001f)
        {
            direction =
                Vector2.down;
        }

        if (Mathf.Abs(direction.x) >
            Mathf.Abs(direction.y))
        {
            attackPoint.localPosition =
                new Vector2(
                    Mathf.Sign(direction.x) *
                    attackDistance,
                    0f
                );
        }
        else
        {
            attackPoint.localPosition =
                new Vector2(
                    0f,
                    Mathf.Sign(direction.y) *
                    attackDistance
                );
        }
    }

    // Animation Event tại frame chạm mục tiêu
    public void DealDamage()
    {
        if (!isAttacking)
            return;

        if (attackPoint == null)
        {
            Debug.LogError(
                $"{name}: chưa gán AttackPoint.",
                this
            );

            return;
        }

        UpdateAttackPoint(
            attackDirection
        );

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                Mathf.Max(
                    0f,
                    attackRadius
                ),
                playerLayer
            );

        foreach (Collider2D hit
                 in hits)
        {
            if (hit == null)
                continue;

            GameObject rootObject =
                hit.transform.root.gameObject;

            // Tránh một mục tiêu có nhiều collider
            // bị nhận damage nhiều lần trong cùng một đòn.
            if (!damagedTargets.Add(
                    rootObject))
            {
                continue;
            }

            Health health =
                hit.GetComponentInParent<Health>();

            if (health != null &&
                !health.IsDead)
            {
                health.TakeDamage(
                    damage
                );

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
            }
        }
    }

    // Animation Event ở frame cuối
    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        cooldownTimer =
            Mathf.Max(
                0.05f,
                attackCooldown
            );

        damagedTargets.Clear();

        if (movement != null)
        {
            movement.CanMove = true;
            movement.ResumeAI();
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;

        damagedTargets.Clear();

        if (animator != null)
        {
            animator.ResetTrigger(
                attackTrigger
            );
        }

        if (movement != null &&
            movement.enabled &&
            gameObject.activeInHierarchy)
        {
            movement.CanMove = true;
            movement.ResumeAI();
        }
    }

    private void OnDisable()
    {
        isAttacking = false;
        damagedTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color =
            Color.green;

        Gizmos.DrawLine(
            transform.position,
            attackPoint.position
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            Mathf.Max(
                0f,
                attackRadius
            )
        );
    }

    private void OnValidate()
    {
        attackDistance =
            Mathf.Max(
                0f,
                attackDistance
            );

        attackRadius =
            Mathf.Max(
                0f,
                attackRadius
            );

        attackCooldown =
            Mathf.Max(
                0.05f,
                attackCooldown
            );

        damage =
            Mathf.Max(
                1,
                damage
            );
    }
}