using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking;
    private Rigidbody2D rb;

    public Transform attackPoint;
    public LayerMask enermyLayer;

    public float attackRadius = 0.6f;
    public float attackCooldown = 1f;
    public float attackDistance = 0.6f;
    public int damage = 20;
    private int combo = 0;
   

public Transform attackPoint;
public float attackRange = 0.6f;
public LayerMask enermyLayer;
public int damage = 1;
   

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isAttacking = true;
    }

    void Update()
    {

        if (attackPoint != null)
        {
            Debug.Log(attackPoint.parent);
        }

        //phát âm thanh đánh khi di chuyển
        if (Input.GetKeyDown(KeyCode.J) && isAttacking)
        {
            isAttacking = true;
            UpdateAttackPoint();
            animator.SetInteger("Combo", combo);

            animator.SetTrigger("Attack");
            combo = (combo + 1) % 2;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
            
        }
        Debug.Log(isAttacking);
        //phát âm thanh đánh khi đứng im lặng, không di chuyển
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            isAttacking = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
            animator.SetInteger("Combo", combo);

            animator.SetTrigger("Attack");

            combo = (combo + 1) % 2;

        }


        //tốc độ đánh khi nhấn nút J, nếu nhấn liên tục thì tốc độ đánh sẽ nhanh hơn

    }

    public void EndAttack()
    {
        Debug.Log("EndAttack");
        isAttacking = false;
    }
    void DealDamage()
{
    Collider2D[] hits = Physics2D.OverlapCircleAll(
        attackPoint.position,
        attackRange,
        enermyLayer);

    foreach (Collider2D hit in hits)
    {
        Enemy enemy = hit.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
        }
    }
}


    private void UpdateAttackPoint()
    {
        Players player = GetComponent<Players>();

        Vector2 dir = player.LastDirection;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir = new Vector2(Mathf.Sign(dir.x), 0);
        }
        else
        {
            dir = new Vector2(0, Mathf.Sign(dir.y));
        }

        attackPoint.localPosition = dir * attackDistance;
    }
    public void DealDamage()
    {
        Debug.Log("DealDamage");

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enermyLayer);

        Debug.Log(hits.Length);

        foreach (Collider2D hit in hits)
        {
            Debug.Log(hit.name);

            EnermyHealth hp = hit.GetComponent<EnermyHealth>();

            if (hp != null)
            {
                Debug.Log("Damage");
                hp.TakeDamage(damage);
            }
        }
    }
}