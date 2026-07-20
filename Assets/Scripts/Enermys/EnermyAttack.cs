using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    [Header("Reference")]
    public EnermyMovement movement;

    [Header("Attack")]
    public Transform attackPoint;
    public LayerMask playerLayer;

    public float attackRange = 1.1f;
    public float attackDistance = 0.6f;
    public float attackRadius = 0.45f;
    public float attackCooldown = 1f;

    public int damage = 1;

    private Animator animator;

    private float attackTimer;

    void Start()
    {
        animator = GetComponent<Animator>();

        if (movement == null)
            movement = GetComponent<EnermyMovement>();
    }

    void Update()
    {
        if (movement.player == null)
            return;

        attackTimer -= Time.deltaTime;

        float distance =
            Vector2.Distance(
                transform.position,
                movement.player.position);

        if (distance > attackRange)
            return;

        if (attackTimer > 0)
            return;

        attackTimer = attackCooldown;

        Attack();
    }

    void Attack()
    {
        movement.CanMove = false;

        FacePlayer();

        animator.SetTrigger("Attack");
        //when player out of range
    }

    void FacePlayer()
    {
        Vector2 dir =
            (movement.player.position - transform.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir = new Vector2(Mathf.Sign(dir.x), 0);
        }
        else
        {
            dir = new Vector2(0, Mathf.Sign(dir.y));
        }

        animator.SetFloat("LastMoveX", dir.x);
        animator.SetFloat("LastMoveY", dir.y);

        attackPoint.localPosition = dir * attackDistance;
    }

    // Animation Event
    public void DealDamage()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRadius,
                playerLayer);

        foreach (Collider2D hit in hits)
        {
            Health hp = hit.GetComponentInParent<Health>();

            if (hp != null)
            {
                hp.TakeDamage(damage);
            }
        }
    }

    // Animation Event
    public void EndAttack()
    {
        movement.CanMove = true;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRadius);
    }
}