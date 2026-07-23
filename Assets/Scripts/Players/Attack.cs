using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private bool isAttacking;
    private float attackTimer;

    public Transform[] attackPoint;
    public LayerMask enermyLayer;

    [Header("Attack")]
    public float attackRadius = 0.6f;
    public float attackDistance = 0.6f;
    public float attackCooldown = 0.35f;
    public int damage = 20;

    private int combo = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        isAttacking = false;
        attackTimer = 0f;
    }

    void Update()
    {
        attackTimer -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Mouse0))
            && !isAttacking
            && attackTimer <= 0f)
        {
            AttackEnemy();
        }
    }

    void AttackEnemy()
    {
        isAttacking = true;
        attackTimer = attackCooldown;

        animator.SetInteger("Combo", combo);
        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");

        combo = (combo + 1) % 2;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
    }

    // Animation Event
    public void DealDamage()
    {
        foreach (Transform point in attackPoint)
        {
            if (point == null)
                continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                point.position,
                attackRadius,
                enermyLayer);

            foreach (Collider2D hit in hits)
            {
                EnermyHealth hp = hit.GetComponent<EnermyHealth>();

                if (hp != null)
                {
                    Vector2 dir =
                        (hp.transform.position - transform.position).normalized;

                    hp.TakeDamage(damage, dir);
                }
            }
        }
    }

    // Animation Event (đặt ở frame cuối animation)
    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        foreach (Transform point in attackPoint)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(point.position, attackRadius);
            }
        }
    }
}