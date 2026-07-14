using UnityEngine;

public class CloneFollow : MonoBehaviour
{
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float roamRadius = 2f;
    public float followDistance = 5f;
    public float stopFollowDistance = 2f;

    [Header("Combat")]
    public float detectRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 0.7f;

    public AudioClip footstepSound;
    public AudioClip attackSound;

    private Rigidbody2D rb;
    private Animator animator;
    private Animator playerAnimator;

    private AudioSource footstepSource;
    private AudioSource attackSource;

    private Vector2 targetPos;

    private Transform targetEnemy;

    private float idleTimer;
    private float attackTimer;
    private float searchTimer;

    private bool attackPlaying;

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
        Debug.Log("ScriptisRunning");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        playerAnimator = player.GetComponent<Animator>();

        AudioSource[] audio = GetComponents<AudioSource>();

        footstepSource = audio[0];
        attackSource = audio[1];

        footstepSource.clip = footstepSound;
        footstepSource.loop = true;

        EnterIdle();
    }

    void Update()
    {
        if (player == null) return;

        searchTimer -= Time.deltaTime;

        if (searchTimer <= 0)
        {
            searchTimer = 0.3f;
            FindNearestEnemy();
        }

        if (targetEnemy != null)
        {
            state = State.Combat;
        }
        else if (Vector2.Distance(transform.position, player.position) > followDistance)
        {
            state = State.Follow;
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
        rb.linearVelocity = Vector2.zero;

        animator.SetBool("IsMoving", false);

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
        MoveTo(targetPos);

        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            EnterIdle();
        }
    }

    void Follow()
    {
        MoveTo(player.position);

        if (Vector2.Distance(transform.position, player.position) < stopFollowDistance)
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

        float dis = Vector2.Distance(transform.position, targetEnemy.position);

        if (dis > attackRange)
        {
            MoveTo(targetEnemy.position);
            return;
        }

        rb.linearVelocity = Vector2.zero;

        animator.SetBool("IsMoving", false);

        StopFootstep();

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0)
        {
            attackTimer = attackCooldown;

            Vector2 dir = (targetEnemy.position - transform.position).normalized;

            if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            {
                dir.x = Mathf.Sign(dir.x);
                dir.y = 0;
            }
            else
            {
                dir.y = Mathf.Sign(dir.y);
                dir.x = 0;
            }

            animator.SetFloat("LastMoveX", dir.x);
            animator.SetFloat("LastMoveY", dir.y);

            animator.SetInteger("Combo", Random.Range(0, 2));

            animator.SetTrigger("Attack");

            attackSource.PlayOneShot(attackSound);
        }
    }

    void MoveTo(Vector2 target)
    {
        Vector2 dir = target - rb.position;

        if (dir.magnitude < 0.05f)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            StopFootstep();
            return;
        }

        dir.Normalize();

        rb.linearVelocity = dir * moveSpeed;

        animator.SetBool("IsMoving", true);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir.x = Mathf.Sign(dir.x);
            dir.y = 0;
        }
        else
        {
            dir.y = Mathf.Sign(dir.y);
            dir.x = 0;
        }

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);

        animator.SetFloat("LastMoveX", dir.x);
        animator.SetFloat("LastMoveY", dir.y);

        if (!footstepSource.isPlaying)
            footstepSource.Play();
    }

    void ChooseRandomTarget()
    {
        Vector2 random = Random.insideUnitCircle * roamRadius;
        targetPos = (Vector2)player.position + random;
    }

    void EnterIdle()
    {
        state = State.Idle;
        idleTimer = Random.Range(1f, 3f);
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

    void StopFootstep()
    {
        if (footstepSource.isPlaying)
            footstepSource.Stop();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ChooseRandomTarget();
    }
}