using UnityEngine;

public class EnermyAttack : MonoBehaviour
{
    public EnermyMovement movement;

    public Transform attackPoint;

    public LayerMask playerLayer;
    private bool isAttacking;

    public float attackRadius = 0.45f;
    public float attackDistance = 0.6f;
    public float attackCooldown = 1f;

    public int damage = 1;

    Animator animator;
    EnermyAudio enemyAudio;

    float timer;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyAudio = GetComponent<EnermyAudio>();

        if (movement == null)
            movement = GetComponent<EnermyMovement>();
    }

    void Update()
    {
        if (movement == null || movement.player == null)
            return;

        if (!isAttacking)
        {
            FacePlayer();
        }

        timer -= Time.deltaTime;

        float dis = Vector2.Distance(
            transform.position,
            movement.player.position);

        if (dis > movement.attackRange)
            return;

        if (isAttacking)
            return;

        if (timer > 0)
            return;

        Attack();
    }

    void Attack()
    {
        isAttacking = true;

        FacePlayer();      // Khóa hướng ngay trước khi đánh

        timer = attackCooldown;

        movement.CanMove = false;

        enemyAudio.PlayAttack();

        animator.SetTrigger("Attack");
    }
    private void FacePlayer()
    {
        Vector2 dir = (movement.player.position - transform.position).normalized;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        // Sprite gốc nhìn sang phải
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
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                attackPoint.position,
                attackRadius,
                playerLayer);

        foreach (Collider2D hit in hits)
        {
            Health hp = hit.GetComponentInParent<Health>();

            if (hp != null)
                hp.TakeDamage(damage);
        }
    }

    // Animation Event
    public void EndAttack()
    {
        isAttacking = false;

        movement.CanMove = true;
    }
}