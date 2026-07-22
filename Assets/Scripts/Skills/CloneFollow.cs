using UnityEngine;

public class CloneFollow : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float roamRadius = 2f;
    public float followDistance = 5f;
    public float stopFollowDistance = 1f;

    [Header("Combat")]
    public float detectRange = 6f;
    public float attackRange = 0.8f;
    public float attackCooldown = 1f;
    public float attackRadius = 0.25f;

    public Transform[] attackPoints;
    public LayerMask enermyLayer;

    public int damage = 20;

    private bool isAttacking;

    public AudioClip footstepSound;
    public AudioClip attackSound;

    Rigidbody2D rb;
    Animator animator;

    AudioSource footstepSource;
    AudioSource attackSource;

    Transform targetEnemy;

    Vector2 targetPos;

    float idleTimer;
    float attackTimer;
    float searchTimer;

    enum State
    {
        Idle,
        Wander,
        Follow,
        Combat
    }

    State state;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        AudioSource[] audio = GetComponents<AudioSource>();

        footstepSource = audio[0];
        attackSource = audio[1];

        footstepSource.clip = footstepSound;
        footstepSource.loop = true;

        EnterIdle();
        isAttacking= true;
    }

    void Update()
    {
        if (player == null)
            return;

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0)
        {
            searchTimer = 0.25f;
            FindNearestEnemy();
        }

        switch (state)
        {
            case State.Idle:
                Idle();
                break;

            case State.Wander:
                Wander();
                break;

            case State.Follow:
                Follow();
                break;

            case State.Combat:
                Combat();
                break;
        }
    }

    void Idle()
    {
        if (targetEnemy != null)
        {
            state = State.Combat;
            return;
        }

        float dis = Vector2.Distance(transform.position, player.position);

        if (dis > followDistance)
        {
            state = State.Follow;
            return;
        }

        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsRunning", false);
        StopFootstep();

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0)
        {
            ChooseRandomTarget();
            state = State.Wander;
        }
    }

    void Wander()
    {
        if (targetEnemy != null)
        {
            state = State.Combat;
            return;
        }

        float dis = Vector2.Distance(transform.position, player.position);

        if (dis > followDistance)
        {
            state = State.Follow;
            return;
        }

        MoveTo(targetPos);

        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            EnterIdle();
        }
    }

    void Follow()
    {
        if (targetEnemy != null)
        {
            state = State.Combat;
            return;
        }

        MoveTo(player.position);

        if (Vector2.Distance(transform.position, player.position) <= stopFollowDistance)
        {
            EnterIdle();
        }
    }

    void Combat()
    {
        if (targetEnemy == null)
        {
            EnterIdle();
            return;
        }

        float dis = Vector2.Distance(rb.position, targetEnemy.position);

        if (dis > detectRange)
        {
            targetEnemy = null;
            EnterIdle();
            return;
        }

        // Chưa đủ gần thì tiếp tục đuổi
        if (dis > attackRange)
        {
            MoveTo(targetEnemy.position);
            return;
        }

        rb.linearVelocity = Vector2.zero;

        animator.SetBool("IsRunning", false);

        StopFootstep();

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0)
            return;

        attackTimer = attackCooldown;

        Vector2 face =
            ((Vector2)targetEnemy.position - rb.position).normalized;

        if (Mathf.Abs(face.x) > Mathf.Abs(face.y))
            face = new Vector2(Mathf.Sign(face.x), 0);
        else
            face = new Vector2(0, Mathf.Sign(face.y));

        animator.SetFloat("LastMoveX", face.x);
        animator.SetFloat("LastMoveY", face.y);

        animator.SetInteger("Combo", Random.Range(0, 2));

        animator.SetTrigger("Attack");

        attackSource.PlayOneShot(attackSound);
    }
    void MoveTo(Vector2 target)
    {
        Vector2 offset = target - rb.position;

        // Giữ khoảng dừng nhỏ để không chồng collider
        if (offset.magnitude < 0.1f)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsRunning", false);
            StopFootstep();
            return;
        }

        Vector2 dir = offset.normalized;

        rb.linearVelocity = dir * moveSpeed;

        animator.SetBool("IsRunning", true);

        Vector2 face;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            face = new Vector2(Mathf.Sign(dir.x), 0);
        else
            face = new Vector2(0, Mathf.Sign(dir.y));

        animator.SetFloat("MoveX", face.x);
        animator.SetFloat("MoveY", face.y);

        animator.SetFloat("LastMoveX", face.x);
        animator.SetFloat("LastMoveY", face.y);

        if (!footstepSource.isPlaying)
            footstepSource.Play();
    }

    void ChooseRandomTarget()
    {
        Vector2 random =
            Random.insideUnitCircle.normalized *
            Random.Range(0.5f, roamRadius);

        targetPos = (Vector2)player.position + random;
    }

    void EnterIdle()
    {
        state = State.Idle;
        idleTimer = Random.Range(1f, 2f);
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enermy");

        float min = Mathf.Infinity;
        targetEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float dis = Vector2.Distance(transform.position, enemy.transform.position);

            if (dis < detectRange && dis < min)
            {
                min = dis;
                targetEnemy = enemy.transform;
            }
        }
    }

    public void DealDamage()
    {
        foreach (Transform point in attackPoints)
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
    void StopFootstep()
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (state == State.Wander)
            ChooseRandomTarget();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoints == null)
            return;

        Gizmos.color = Color.red;

        foreach (Transform point in attackPoints)
        {
            if (point != null)
                Gizmos.DrawWireSphere(point.position, attackRadius);
        }
    }

        public void EndAttack()
    {
        isAttacking = false;
    }

}