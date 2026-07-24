using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    public EnermyMovement movement;

    public Transform attackPoint;

    public LayerMask playerLayer;
    private bool isAttacking;

    public float attackDistance = 0.6f;
    public float attackRadius = 0.35f;
    public float attackCooldown = 1f;

    public int damage = 1;

    Animator animator;
    EnermyAudio enemyAudio;

    SpriteRenderer sr;
    float timer;
    private Health playerHealth;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAudio = GetComponent<EnermyAudio>();
        sr = GetComponent<SpriteRenderer>();

        playerHealth = FindFirstObjectByType<Health>();
        if (movement == null)
            movement = GetComponent<EnermyMovement>();
    }

    void Update()
    {
        if (movement == null || movement.player == null)
            return;

        if (playerHealth != null && playerHealth.IsDead)
        {
            isAttacking = false;
            movement.StopMove();
            return;
        }

        if (!isAttacking)
        {
            FacePlayer();
        }

        float dis = Vector2.Distance(
    transform.position,
    movement.target.position);

        if (dis > movement.attackRange)
        {
            isAttacking = false;
            movement.CanMove = true;
            return;
        }


        if (isAttacking)
        {
            movement.StopMove();
            return;
        }

        timer -= Time.deltaTime;

        if (timer > 0)
            return;

        if (timer <= 0)
        {
            Attack();
        }
    }

    void Attack()
    {
        isAttacking = true;

        FacePlayer();

        movement.StopMove();

        enemyAudio.PlayAttack();

        animator.SetTrigger("Attack");
    }
    private void FacePlayer()
    {
        if (movement.target == null)
            return;

        Vector2 dir =
            ((Vector2)movement.target.position -
            (Vector2)transform.position).normalized;


        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {

            attackPoint.localPosition =
                new Vector2(
                    Mathf.Sign(dir.x) * attackDistance,
                    0
                );
        }
        else
        {
            attackPoint.localPosition =
                new Vector2(
                    0,
                    Mathf.Sign(dir.y) * attackDistance
                );
        }
    }
    // Animation Event
    public void DealDamage()
    {

        if (playerHealth != null && playerHealth.IsDead)
            return;


        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRadius,
                playerLayer);

        foreach (Collider2D hit in hits)
        {
            Health playerHp = hit.GetComponentInParent<Health>();

            if (playerHp != null)
            {
                playerHp.TakeDamage(damage);
            }

            CloneHealth cloneHp = hit.GetComponentInParent<CloneHealth>();

            if (cloneHp != null)
            {
                cloneHp.TakeDamage(damage);
            }
        }
    }

    // Animation Event
    public void EndAttack()
    {
        isAttacking = false;

        movement.CanMove = true;

        // cho AI cập nhật lại
        movement.ResumeAI();
    }

    private void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position,
            attackPoint.position
        );
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRadius);
    }
}