using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Transform target;
    public float speed = 2f;
    public float stopDistance = 1f;

    public float attackCooldown = 1f;
    private float nextAttackTime;

    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
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
        Debug.Log(target.position.x < transform.position.x);
        if (target == null) return;

        float distance = Vector2.Distance(transform.position, target.position);

        // ===== Lật hướng theo Player =====
        if (target.position.x < transform.position.x)
        {
            // Player ở bên trái
            sr.flipX = true;
        }
        else
        {
            // Player ở bên phải
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
        anim.SetTrigger("Attack");
        Debug.Log("Enemy Attack!");
    }
}