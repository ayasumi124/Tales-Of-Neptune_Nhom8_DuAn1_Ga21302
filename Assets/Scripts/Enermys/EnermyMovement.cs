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

    // =====================================================
    // TARGET
    // =====================================================

    [Header("Target")]
    [SerializeField]
    private Transform target;

    [Tooltip("Những Tag mà enemy có thể chọn làm mục tiêu.")]
    [SerializeField]
    private string[] targetTags =
    {
        "Player",
        "Clone"
    };

    [Tooltip("Khoảng thời gian giữa các lần tìm lại mục tiêu.")]
    [Min(0.05f)]
    [SerializeField]
    private float targetSearchInterval = 0.3f;
    private float stuckTimer;
    private Vector2 lastPosition;

    /*
     * Giữ lại để các script cũ dùng movement.player
     * không bị lỗi. Script mới nên dùng Target.
     */
    [HideInInspector]
    public Transform player;

    public Transform Target => target;

    // =====================================================
    // MOVEMENT
    // =====================================================

    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 2f;

    [Min(0f)]
    public float detectRange = 6f;

    [Min(0f)]
    public float attackRange = 0.8f;

    [Tooltip("Khoảng cách được xem là đã tới đích.")]
    [Min(0.01f)]
    [SerializeField]
    private float arrivalDistance = 0.15f;

    // =====================================================
    // WANDER
    // =====================================================

    [Header("Wander")]
    [Min(0.1f)]
    public float roamRadius = 3f;

    [Tooltip("Thời gian đứng nghỉ giữa các lần đi random.")]
    [Min(0.1f)]
    public float idleTime = 2f;

    [Tooltip("Enemy bắt đầu đi random ngay khi Play.")]
    [SerializeField]
    private bool wanderImmediatelyOnStart = true;

    // =====================================================
    // KNOCKBACK
    // =====================================================

    [Header("Knockback")]
    [Min(0f)]
    public float knockbackDecay = 12f;

    public Vector2 externalVelocity;

    // =====================================================
    // PUBLIC STATE
    // =====================================================

    [Header("Runtime")]
    public bool CanMove = true;

    public EnemyState CurrentState
    {
        get;
        private set;
    }

    public Vector2 SpawnPosition =>
        spawnPosition;

    public Vector2 LastMoveDirection
    {
        get;
        private set;
    } = Vector2.down;

    // =====================================================
    // COMPONENTS
    // =====================================================

    private Rigidbody2D rb;
    private Animator animator;
    private EnermyAudio enemyAudio;

    // =====================================================
    // RUNTIME DATA
    // =====================================================

    private Vector2 spawnPosition;
    private Vector2 wanderTargetPosition;
    private Vector2 desiredVelocity;

    private float idleTimer;
    private float targetSearchTimer;

    private bool initialized;
    private bool stoppedImmediately;

    // =====================================================
    // UNITY METHODS
    // =====================================================

    private void Awake()
    {
        CacheComponents();
    }

    private void Start()
    {
        Initialize();
    }

    private void Update()
    {
        CheckIfStuck();
        if (!initialized)
        {
            Initialize();
        }

        UpdateExternalVelocity();
        UpdateTargetSearch();

        if (!CanMove ||
            stoppedImmediately)
        {
            StopMove();
            return;
        }

        float distanceToTarget =
            DistanceToTarget();

        switch (CurrentState)
        {
            case EnemyState.Idle:
                UpdateIdle(
                    distanceToTarget
                );
                break;

            case EnemyState.Wander:
                UpdateWander(
                    distanceToTarget
                );
                break;

            case EnemyState.Chase:
                UpdateChase(
                    distanceToTarget
                );
                break;

            case EnemyState.Return:
                UpdateReturn(
                    distanceToTarget
                );
                break;
        }
    }

    private void OnDisable()
    {
        desiredVelocity =
            Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        SetMovingVisual(false);
    }

    // =====================================================
    // INITIALIZATION
    // =====================================================

    private void CacheComponents()
    {
        if (rb == null)
        {
            rb =
                GetComponent<Rigidbody2D>();
        }

        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (enemyAudio == null)
        {
            enemyAudio =
                GetComponent<EnermyAudio>();
        }
    }

    private void Initialize()
    {
        lastPosition = transform.position;
        if (initialized)
            return;

        initialized = true;

        CacheComponents();

        spawnPosition =
            transform.position;

        CanMove = true;
        stoppedImmediately = false;

        desiredVelocity =
            Vector2.zero;

        externalVelocity =
            Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        FindNearestTarget();

        if (IsTargetInDetectRange())
        {
            CurrentState =
                EnemyState.Chase;
        }
        else if (wanderImmediatelyOnStart)
        {
            BeginWander();
        }
        else
        {
            EnterIdle();
        }
    }

    // =====================================================
    // STATE UPDATES
    // =====================================================

    private void UpdateIdle(
        float distanceToTarget)
    {
        StopMove();

        if (IsTargetInDetectRange())
        {
            CurrentState =
                EnemyState.Chase;

            return;
        }

        idleTimer -=
            Time.deltaTime;

        if (idleTimer > 0f)
            return;

        BeginWander();
    }

    private void UpdateWander(
        float distanceToTarget)
    {
        if (IsTargetInDetectRange())
        {
            CurrentState =
                EnemyState.Chase;

            return;
        }

        float distanceToPoint =
            Vector2.Distance(
                GetCurrentPosition(),
                wanderTargetPosition
            );

        if (distanceToPoint <=
            arrivalDistance)
        {
            EnterIdle();
            return;
        }

        MoveTo(
            wanderTargetPosition
        );
    }

    private void UpdateChase(
        float distanceToTarget)
    {
        if (!HasTarget())
        {
            EnterReturnOrIdle();
            return;
        }

        if (distanceToTarget >
            detectRange)
        {
            EnterReturnOrIdle();
            return;
        }

        if (distanceToTarget <=
            attackRange)
        {
            StopMove();
            FaceTarget();
            return;
        }

        Vector2 direction =
            DirectionToTarget();

        float stoppingDistance =
            Mathf.Max(
                0f,
                attackRange - 0.1f
            );

        Vector2 stoppingPosition =
            (Vector2)target.position -
            direction *
            stoppingDistance;

        MoveTo(
            stoppingPosition
        );
    }

    private void UpdateReturn(
        float distanceToTarget)
    {
        if (IsTargetInDetectRange())
        {
            CurrentState =
                EnemyState.Chase;

            return;
        }

        float distanceToSpawn =
            Vector2.Distance(
                GetCurrentPosition(),
                spawnPosition
            );

        if (distanceToSpawn <=
            arrivalDistance)
        {
            /*
             * Sau khi quay về điểm spawn,
             * enemy tiếp tục đi random.
             */
            BeginWander();
            return;
        }

        MoveTo(
            spawnPosition
        );
    }

    private void EnterReturnOrIdle()
    {
        float distanceToSpawn =
            Vector2.Distance(
                GetCurrentPosition(),
                spawnPosition
            );

        if (distanceToSpawn >
            arrivalDistance)
        {
            CurrentState =
                EnemyState.Return;
        }
        else
        {
            BeginWander();
        }
    }

    // =====================================================
    // IDLE / WANDER
    // =====================================================

    private void EnterIdle()
    {
        CurrentState =
            EnemyState.Idle;

        idleTimer =
            Random.Range(
                0.5f,
                Mathf.Max(
                    0.6f,
                    idleTime
                )
            );

        StopMove();
    }

    private void BeginWander()
    {
        stuckTimer = 0f;
        lastPosition = transform.position;
        ChooseRandomPoint();

        CurrentState =
            EnemyState.Wander;
    }

    private void ChooseRandomPoint()
    {
        float safeRadius =
            Mathf.Max(
                0.1f,
                roamRadius
            );

        Vector2 direction =
            Random.insideUnitCircle;

        /*
         * Tránh chọn vector quá nhỏ,
         * khiến enemy nhìn như đứng yên.
         */
        if (direction.sqrMagnitude <
            0.1f)
        {
            direction =
                Random.value < 0.5f
                    ? Vector2.right
                    : Vector2.left;
        }

        direction =
Random.insideUnitCircle.normalized;

        float angle =
        Random.Range(-35f, 35f);

        direction =
        Quaternion.Euler(0, 0, angle)
        * direction;

        float randomDistance =
        Random.Range(
        roamRadius * 0.3f,
        roamRadius);

        wanderTargetPosition =
            spawnPosition +
            direction *
            randomDistance;
    }

    // =====================================================
    // TARGET SEARCH
    // =====================================================

    private void UpdateTargetSearch()
    {
        targetSearchTimer -=
            Time.deltaTime;

        if (targetSearchTimer > 0f)
            return;

        targetSearchTimer =
            Mathf.Max(
                0.05f,
                targetSearchInterval
            );

        FindNearestTarget();
    }

    public void FindNearestTarget()
    {
        float nearestDistanceSquared =
            Mathf.Infinity;

        Transform nearestTarget =
            null;

        if (targetTags == null)
        {
            SetTarget(null);
            return;
        }

        foreach (string targetTag
                 in targetTags)
        {
            if (string.IsNullOrWhiteSpace(
                    targetTag))
            {
                continue;
            }

            GameObject[] objects;

            try
            {
                objects =
                    GameObject
                        .FindGameObjectsWithTag(
                            targetTag
                        );
            }
            catch (UnityException)
            {
                Debug.LogError(
                    $"{name}: Tag '{targetTag}' " +
                    "chưa tồn tại trong Tag Manager.",
                    this
                );

                continue;
            }

            foreach (GameObject obj
                     in objects)
            {
                if (obj == null ||
                    obj == gameObject ||
                    !obj.activeInHierarchy)
                {
                    continue;
                }

                Transform candidate =
                    obj.transform;

                if (!IsTargetValid(
                        candidate))
                {
                    continue;
                }

                float distanceSquared =
                    (
                        candidate.position -
                        transform.position
                    ).sqrMagnitude;

                if (distanceSquared >=
                    nearestDistanceSquared)
                {
                    continue;
                }

                nearestDistanceSquared =
                    distanceSquared;

                nearestTarget =
                    candidate;
            }
        }

        SetTarget(
            nearestTarget
        );
    }

    public void SetTarget(
        Transform newTarget)
    {
        target =
            newTarget;

        /*
         * Giữ tương thích với script cũ.
         */
        player =
            newTarget;
    }

    public void ClearTarget()
    {
        SetTarget(null);

        targetSearchTimer =
            0f;
    }

    private bool IsTargetValid(
        Transform possibleTarget)
    {
        if (possibleTarget == null)
            return false;

        if (!possibleTarget.gameObject
                .activeInHierarchy)
        {
            return false;
        }

        Health playerHealth =
            possibleTarget
                .GetComponentInParent<Health>();

        if (playerHealth != null &&
            playerHealth.IsDead)
        {
            return false;
        }

        CloneHealth cloneHealth =
            possibleTarget
                .GetComponentInParent<CloneHealth>();

        if (cloneHealth != null &&
            !cloneHealth.enabled)
        {
            return false;
        }

        return true;
    }

    public bool HasTarget()
    {
        return IsTargetValid(
            target
        );
    }

    public bool IsTargetInDetectRange()
    {
        if (!HasTarget())
            return false;

        float detectRangeSquared =
            detectRange *
            detectRange;

        float distanceSquared =
            (
                target.position -
                transform.position
            ).sqrMagnitude;

        return distanceSquared <=
               detectRangeSquared;
    }
    private void CheckIfStuck()
    {
        if (CurrentState != EnemyState.Wander)
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
            return;
        }

        float moved =
            Vector2.Distance(
                lastPosition,
                transform.position);

        if (moved < 0.02f)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= 0.5f)
            {
                ChooseRandomPoint();

                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        lastPosition = transform.position;
    }

    public float DistanceToTarget()
    {
        if (!HasTarget())
        {
            return Mathf.Infinity;
        }

        return Vector2.Distance(
            transform.position,
            target.position
        );
    }

    public Vector2 DirectionToTarget()
    {
        if (!HasTarget())
        {
            return LastMoveDirection;
        }

        Vector2 direction =
            (Vector2)target.position -
            GetCurrentPosition();

        if (direction.sqrMagnitude <=
            0.001f)
        {
            return LastMoveDirection;
        }

        return direction.normalized;
    }

    // =====================================================
    // MOVEMENT
    // =====================================================

    private Vector2 GetCurrentPosition()
    {
        return rb != null
            ? rb.position
            : (Vector2)transform.position;
    }

    private void MoveTo(
        Vector2 destination)
    {
        if (rb == null)
            return;

        Vector2 direction =
            destination -
            rb.position;

        if (direction.sqrMagnitude <=
            0.0001f)
        {
            StopMove();
            return;
        }

        direction.Normalize();

        LastMoveDirection =
            direction;

        desiredVelocity =
            direction *
            Mathf.Max(
                0f,
                moveSpeed
            );

        rb.linearVelocity =
            desiredVelocity +
            externalVelocity;

        SetMovingVisual(true);
        FaceDirection(direction);
    }

    private void FaceDirection(
        Vector2 direction)
    {
        if (Mathf.Abs(direction.x) <=
            0.01f)
        {
            return;
        }

        Vector3 scale =
            transform.localScale;

        float absoluteX =
            Mathf.Abs(
                scale.x
            );

        scale.x =
            direction.x < 0f
                ? -absoluteX
                : absoluteX;

        transform.localScale =
            scale;
    }

    public void FaceTarget()
    {
        if (!HasTarget())
            return;

        FaceDirection(
            DirectionToTarget()
        );
    }

    // =====================================================
    // STOP / RESUME
    // =====================================================

    public void StopMove()
    {
        desiredVelocity =
            Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity =
                externalVelocity;
        }

        SetMovingVisual(false);
    }

    public void StopImmediately()
    {
        CanMove = false;
        stoppedImmediately = true;

        desiredVelocity =
            Vector2.zero;

        externalVelocity =
            Vector2.zero;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        SetMovingVisual(false);
    }

    public void PauseAI()
    {
        CanMove = false;
        StopMove();
    }

    public void ResumeAI()
    {
        stoppedImmediately = false;
        CanMove = true;

        if (!HasTarget())
        {
            FindNearestTarget();
        }

        if (IsTargetInDetectRange())
        {
            CurrentState =
                EnemyState.Chase;

            return;
        }

        float distanceToSpawn =
            Vector2.Distance(
                GetCurrentPosition(),
                spawnPosition
            );

        if (distanceToSpawn >
            arrivalDistance)
        {
            CurrentState =
                EnemyState.Return;
        }
        else
        {
            BeginWander();
        }
    }

    // =====================================================
    // KNOCKBACK
    // =====================================================

    private void UpdateExternalVelocity()
    {
        externalVelocity =
            Vector2.MoveTowards(
                externalVelocity,
                Vector2.zero,
                Mathf.Max(
                    0f,
                    knockbackDecay
                ) *
                Time.deltaTime
            );
    }

    // =====================================================
    // VISUAL / AUDIO
    // =====================================================

    private void SetMovingVisual(
        bool moving)
    {
        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                moving
            );
        }

        if (enemyAudio != null)
        {
            enemyAudio.PlayFootstep(
                moving
            );
        }
    }

    // =====================================================
    // GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                detectRange
            )
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                attackRange
            )
        );

        Gizmos.color =
            Color.cyan;

        Vector3 center =
            Application.isPlaying
                ? (Vector3)spawnPosition
                : transform.position;

        Gizmos.DrawWireSphere(
            center,
            Mathf.Max(
                0f,
                roamRadius
            )
        );

        if (Application.isPlaying &&
            CurrentState ==
            EnemyState.Wander)
        {
            Gizmos.color =
                Color.magenta;

            Gizmos.DrawLine(
                transform.position,
                wanderTargetPosition
            );

            Gizmos.DrawWireSphere(
                wanderTargetPosition,
                0.12f
            );
        }
    }

    private void OnValidate()
    {
        moveSpeed =
            Mathf.Max(
                0f,
                moveSpeed
            );

        detectRange =
            Mathf.Max(
                0f,
                detectRange
            );

        attackRange =
            Mathf.Clamp(
                attackRange,
                0f,
                detectRange
            );

        roamRadius =
            Mathf.Max(
                0.1f,
                roamRadius
            );

        idleTime =
            Mathf.Max(
                0.1f,
                idleTime
            );

        arrivalDistance =
            Mathf.Max(
                0.01f,
                arrivalDistance
            );

        knockbackDecay =
            Mathf.Max(
                0f,
                knockbackDecay
            );

        targetSearchInterval =
            Mathf.Max(
                0.05f,
                targetSearchInterval
            );
    }
}