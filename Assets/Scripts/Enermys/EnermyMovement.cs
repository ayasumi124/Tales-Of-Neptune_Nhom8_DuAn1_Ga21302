using UnityEngine;

public class EnermyMovement : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Wander,
        Chase,
        Return
    }

    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectRange = 6f;
    public float attackRange = 1.1f;

    [Header("Wander")]
    public float roamRadius = 3f;
    public float idleTime = 2f;

    public bool CanMove = true;

    public EnemyState CurrentState { get; private set; }

    Rigidbody2D rb;
    Animator animator;
    EnermyAudio enemyAudio;

    Vector2 spawnPos;
    Vector2 targetPos;

    float idleTimer;
    public Vector2 externalVelocity;

    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyAudio = GetComponent<EnermyAudio>();
        sr = GetComponent<SpriteRenderer>();

        spawnPos = transform.position;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        EnterIdle();
    }

    void Update()
    {

        externalVelocity = Vector2.Lerp(
        externalVelocity,
        Vector2.zero,
        12f * Time.deltaTime);
        if (!CanMove)
        {
            StopMove();
            return;
        }

        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        switch (CurrentState)
        {
            case EnemyState.Idle:

                idleTimer -= Time.deltaTime;

                if (distance <= detectRange)
                {
                    CurrentState = EnemyState.Chase;
                    break;
                }

                if (idleTimer <= 0)
                {
                    ChooseRandomPoint();
                    CurrentState = EnemyState.Wander;
                }

                break;

            case EnemyState.Wander:

                if (distance <= detectRange)
                {
                    CurrentState = EnemyState.Chase;
                    break;
                }

                MoveTo(targetPos);

                if (Vector2.Distance(transform.position, targetPos) < 0.2f)
                    EnterIdle();

                break;

            case EnemyState.Chase:

                if (distance > detectRange)
                {
                    CurrentState = EnemyState.Return;
                    break;
                }

                if (distance > attackRange)
                    MoveTo(player.position);
                else
                    StopMove();

                break;

            case EnemyState.Return:

                MoveTo(spawnPos);

                if (distance <= detectRange)
                {
                    CurrentState = EnemyState.Chase;
                    break;
                }

                if (Vector2.Distance(transform.position, spawnPos) < 0.2f)
                    EnterIdle();

                break;
        }
    }

    void MoveTo(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)transform.position).normalized;

        rb.linearVelocity = dir * moveSpeed + externalVelocity;

        animator.SetBool("IsMoving", true);

        enemyAudio.PlayFootstep(true);

        sr.flipX = dir.x < 0;
    }

    void StopMove()
    {
        rb.linearVelocity = externalVelocity;

        animator.SetBool("IsMoving", false);

        enemyAudio.PlayFootstep(false);
    }

    void ChooseRandomPoint()
    {
        targetPos = spawnPos + Random.insideUnitCircle * roamRadius;
    }

    void EnterIdle()
    {
        CurrentState = EnemyState.Idle;

        idleTimer = Random.Range(1f, idleTime);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)spawnPos : transform.position, roamRadius);
    }


}