using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public float speed = 2f;
    public float stopDistance = 1f;

    public float attackCooldown = 1f;
    private float nextAttackTime;

    public int maxHealth = 1;
    private int currentHealth;

    private bool isDead = false;

    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;

        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        if (target == null)
        {
            Debug.Log("Target NULL");
        }
        else
        {
            Debug.Log("Target: " + target.name);
        }
    }

    void Update()
    {
        if (target == null || isDead) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // ===== Lật hướng theo Player =====
        if (target.position.x < transform.position.x)
        {
            sr.flipX = true;
        }
        else
        {
            sr.flipX = false;
        }

        // Di chuyển tới Player
        if (distance > stopDistance)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

            anim.SetBool("Moving", true);
        }
        else
        {
            anim.SetBool("Moving", false);

            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + attackCooldown;
            }
        }
    }

    void Attack()
    {
        if (isDead) return;

        anim.SetTrigger("Attack");
        Debug.Log("Enemy Attack!");
    }

    // Nhận sát thương
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Enemy HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Chết
    void Die()
    {
        isDead = true;

        anim.SetBool("Moving", false);
        anim.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 1f);
    }
}