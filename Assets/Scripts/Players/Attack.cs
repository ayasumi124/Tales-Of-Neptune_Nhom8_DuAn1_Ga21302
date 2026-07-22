using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking;
    private Rigidbody2D rb;

    public Transform[] attackPoint;
    public LayerMask enermyLayer;

    public float attackRadius = 0.6f;
    public float attackCooldown = 1f;
    public float attackDistance = 0.6f;
    public int damage = 20;

    private int combo = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        isAttacking = true;
    }

    void Update()
    {
        // Đánh khi đang di chuyển
        if (Input.GetKeyDown(KeyCode.J) && isAttacking)
        {
            isAttacking = true;

            UpdateAttackPoint();

            animator.SetInteger("Combo", combo);
            animator.SetTrigger("Attack");

            combo = (combo + 1) % 2;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
        }

        // Đánh khi đứng yên
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            isAttacking = true;

            UpdateAttackPoint();

            animator.SetInteger("Combo", combo);
            animator.SetTrigger("Attack");

            combo = (combo + 1) % 2;

            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
        }
    }

    public void EndAttack()
    {
        isAttacking = false;
    }

    private void UpdateAttackPoint()
    {
        // Không cần làm gì.
        // Các AttackPoint đã được đặt sẵn trong Unity
        // theo hình quạt. Chỉ cần để nguyên.
    }

    public void DealDamage()
    {
        Debug.Log("DealDamage");

        foreach (Transform point in attackPoint)
        {
            if (point == null)
                continue;

            Collider2D[] hits =
                Physics2D.OverlapCircleAll(
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

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;

        foreach (Transform point in attackPoint)
        {
            if (point != null)
            {
                Gizmos.DrawWireSphere(
                    point.position,
                    attackRadius);
            }
        }
    }
}