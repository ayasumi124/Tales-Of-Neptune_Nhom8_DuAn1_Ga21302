using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    public EnermyMovement movement;

    public Transform attackPoint;

    public LayerMask playerLayer;
    private bool isAttacking;

    public float attackDistance = 0.8f;
    public float attackRadius = 0.25f;
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
        Vector2 dir = movement.LastMoveDirection;

        // Chỉ lật sprite khi đi trái/phải
        if (Mathf.Abs(dir.x) > 0.01f)
        {
            sr.flipX = dir.x < 0;
        }

        Vector2 attackDir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            attackDir = new Vector2(Mathf.Sign(dir.x), 0);
        else
            attackDir = new Vector2(0, Mathf.Sign(dir.y));

        attackPoint.localPosition = attackDir * attackDistance;
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
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            attackPoint.position,
            attackRadius);
    }
}