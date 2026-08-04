using System.Collections;
using UnityEngine;

public abstract class EnermyAttackBase : MonoBehaviour
{
    [Header("Common References")]
    [SerializeField]
    protected EnermyMovement movement;

    [SerializeField]
    protected Animator animator;

    [SerializeField]
    protected EnermyAudio enemyAudio;

    [Header("Common Attack")]
    [Min(0f)]
    [SerializeField]
    protected float attackRange = 1.5f;

    [Min(0.05f)]
    [SerializeField]
    protected float attackCooldown = 1f;

    [Min(0f)]
    [SerializeField]
    protected float attackLockTime = 0.6f;

    [Header("Animator")]
    [SerializeField]
    protected string attackTrigger = "Attack";

    protected bool isAttacking;
    protected float cooldownTimer;

    private Coroutine attackCoroutine;

    public bool IsAttacking =>
        isAttacking;

    protected virtual void Awake()
    {
        CacheComponents();
    }

    protected virtual void Update()
    {
        UpdateCooldown();

        if (!CanStartAttack())
            return;

        StartAttack();
    }

    protected virtual void CacheComponents()
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

    protected virtual bool CanStartAttack()
    {
        if (isAttacking)
            return false;

        if (cooldownTimer > 0f)
            return false;

        if (movement == null)
            return false;

        if (!movement.HasTarget())
            return false;

        if (!movement.CanMove)
            return false;

        if (movement.DistanceToTarget() >
            attackRange)
        {
            return false;
        }

        return true;
    }

    protected void StartAttack()
    {
        if (attackCoroutine != null)
            return;

        attackCoroutine =
            StartCoroutine(
                AttackRoutine()
            );
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        movement.CanMove = false;
        movement.StopMove();
        movement.FaceTarget();

        OnAttackStarted();

        if (enemyAudio != null)
        {
            enemyAudio.PlayAttack();
        }

        if (animator != null &&
            !string.IsNullOrWhiteSpace(
                attackTrigger))
        {
            animator.ResetTrigger(
                attackTrigger
            );

            animator.SetTrigger(
                attackTrigger
            );
        }

        yield return PerformAttack();

        if (attackLockTime > 0f)
        {
            yield return
                new WaitForSeconds(
                    attackLockTime
                );
        }

        FinishAttack();
    }

    protected virtual void OnAttackStarted()
    {
    }

    protected abstract IEnumerator PerformAttack();

    protected virtual void FinishAttack()
    {
        isAttacking = false;

        cooldownTimer =
            Mathf.Max(
                0.05f,
                attackCooldown
            );

        attackCoroutine = null;

        if (movement != null)
        {
            movement.CanMove = true;
            movement.ResumeAI();
        }
    }

    public virtual void EndAttack()
    {
        if (!isAttacking)
            return;

        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
        }

        FinishAttack();
    }

    public virtual void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(
                attackCoroutine
            );

            attackCoroutine = null;
        }

        isAttacking = false;

        if (animator != null &&
            !string.IsNullOrWhiteSpace(
                attackTrigger))
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

    protected Vector2 GetDirectionToTarget()
    {
        if (movement == null)
            return Vector2.down;

        return movement.DirectionToTarget();
    }

    protected Transform GetTarget()
    {
        if (movement == null)
            return null;

        return movement.Target;
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

    protected virtual void OnDisable()
    {
        isAttacking = false;
        attackCoroutine = null;
    }

    protected virtual void OnValidate()
    {
        attackRange =
            Mathf.Max(
                0f,
                attackRange
            );

        attackCooldown =
            Mathf.Max(
                0.05f,
                attackCooldown
            );

        attackLockTime =
            Mathf.Max(
                0f,
                attackLockTime
            );
    }
}