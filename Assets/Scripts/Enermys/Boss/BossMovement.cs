using UnityEngine;

public class BossMovement : MonoBehaviour
{
    public enum BossState
    {
        Dormant,
        Idle,
        Wander,
        Chase,
        Return
    }

    [Header("Target")]
    [SerializeField]
    private Transform target;

    [SerializeField]
    private string[] targetTags =
    {
        "Player",
        "Clone"
    };

    [Min(0.05f)]
    [SerializeField]
    private float targetSearchInterval = 0.25f;

    [HideInInspector]
    public Transform player;

    public Transform Target => target;

    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 2f;

    [Min(0f)]
    public float detectRange = 7f;

    [Min(0f)]
    public float attackRange = 1.2f;

    [Min(0.01f)]
    [SerializeField]
    private float arrivalDistance = 0.15f;

    [Header("Boss Activation")]
    [SerializeField]
    private bool stayActivated = true;

    [SerializeField]
    private bool cloneCanActivateBoss = false;

    public bool IsActivated { get; private set; }

    [Header("Respawn Detection")]
    [SerializeField]
    private float respawnDetectionDelay = 1f;

    private float detectionLockTimer;

    /*
     * Chỉ bật khi ResetBoss() do Play Again.
     *
     * Khi true:
     * Boss sẽ không activate cho đến khi Player
     * đi RA NGOÀI Detect Range ít nhất một lần.
     */
    private bool requirePlayerExitBeforeReactivate;

    [Header("Wander After Activation")]
    [SerializeField]
    private bool wanderWhenTargetLost = true;

    [Min(0.1f)]
    public float roamRadius = 4f;

    [Min(0.1f)]
    public float idleTime = 1.5f;

    [Header("Knockback")]
    [Min(0f)]
    public float knockbackDecay = 12f;

    public Vector2 externalVelocity;

    [Header("Runtime")]
    public bool CanMove = true;

    public BossState CurrentState
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

    private Rigidbody2D rb;
    private Animator animator;
    private EnermyAudio enemyAudio;

    private Vector2 spawnPosition;
    private Vector2 wanderTargetPosition;
    private Vector2 desiredVelocity;

    private float idleTimer;
    private float targetSearchTimer;

    private float stuckTimer;
    private Vector2 lastPosition;

    private bool initialized;
    private bool stoppedImmediately;

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
        if (!initialized)
        {
            Initialize();
        }

        UpdateExternalVelocity();
        UpdateTargetSearch();

        if (!IsActivated)
        {
            UpdateDormant();
            return;
        }

        CheckIfStuck();

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
            case BossState.Dormant:
                EnterIdle();
                break;

            case BossState.Idle:
                UpdateIdle(
                    distanceToTarget
                );
                break;

            case BossState.Wander:
                UpdateWander(
                    distanceToTarget
                );
                break;

            case BossState.Chase:
                UpdateChase(
                    distanceToTarget
                );
                break;

            case BossState.Return:
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
        if (initialized)
            return;

        initialized = true;

        CacheComponents();

        spawnPosition =
            transform.position;

        lastPosition =
            transform.position;

        IsActivated = false;

        CanMove = true;
        stoppedImmediately = false;

        desiredVelocity =
            Vector2.zero;

        externalVelocity =
            Vector2.zero;

        target = null;
        player = null;

        targetSearchTimer = 0f;
        detectionLockTimer = 0f;

        /*
         * Load scene bình thường:
         * KHÔNG yêu cầu Player phải đi ra ngoài.
         *
         * Vì vậy lần đầu bước vào Detect Range
         * Boss vẫn hoạt động như bình thường.
         */
        requirePlayerExitBeforeReactivate =
            false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        CurrentState =
            BossState.Dormant;

        StopMove();
    }

    private void UpdateDormant()
    {
        CurrentState =
            BossState.Dormant;

        StopMove();

        /*
         * Delay ngắn sau khi Play Again.
         */
        if (detectionLockTimer > 0f)
        {
            detectionLockTimer -=
                Time.deltaTime;

            return;
        }

        /*
         * Tìm Player.
         *
         * Khi Boss chưa activate và
         * cloneCanActivateBoss = false,
         * FindNearestTarget() chỉ lấy Player.
         */
        FindNearestTarget();

        if (!HasTarget())
            return;

        /*
         * Đảm bảo Clone không thể vô tình
         * kích hoạt Boss nếu option đang tắt.
         */
        if (!cloneCanActivateBoss &&
            !IsPlayerTarget(target))
        {
            Transform realPlayer =
                FindPlayer();

            if (realPlayer == null)
            {
                SetTarget(null);
                return;
            }

            SetTarget(
                realPlayer
            );
        }

        float distance =
            DistanceToTarget();

        /*
         * =====================================
         * PLAY AGAIN PROTECTION
         * =====================================
         *
         * Sau ResetBoss(), Boss KHÔNG được
         * activate nếu Player đang nằm sẵn
         * trong Detect Range.
         *
         * Player bắt buộc phải đi ra ngoài
         * Detect Range trước.
         */
        if (requirePlayerExitBeforeReactivate)
        {
            if (distance > detectRange)
            {
                requirePlayerExitBeforeReactivate =
                    false;

                /*
                 * Xóa target để lần tiếp theo
                 * Boss scan Player lại từ đầu.
                 */
                SetTarget(null);

                targetSearchTimer =
                    targetSearchInterval;

                Debug.Log(
                    $"{name}: Player đã rời Detect Range. " +
                    "Boss có thể detect lại."
                );
            }

            return;
        }

        /*
         * Player vẫn còn ở ngoài.
         */
        if (distance > detectRange)
            return;

        /*
         * Player bước vào Detect Range
         * sau khi đã ra ngoài.
         */
        ActivateBoss();
    }

    private void ActivateBoss()
    {
        if (IsActivated)
            return;

        IsActivated = true;

        stoppedImmediately = false;
        CanMove = true;

        CurrentState =
            BossState.Chase;

        FaceTarget();

        Debug.Log(
            $"{name}: Boss Activated"
        );
    }

    private void UpdateIdle(
        float distanceToTarget)
    {
        StopMove();

        if (HasTarget() &&
            distanceToTarget <= detectRange)
        {
            CurrentState =
                BossState.Chase;

            return;
        }

        idleTimer -=
            Time.deltaTime;

        if (idleTimer > 0f)
            return;

        if (wanderWhenTargetLost)
        {
            BeginWander();
        }
        else
        {
            idleTimer =
                Mathf.Max(
                    0.1f,
                    idleTime
                );
        }
    }

    private void EnterIdle()
    {
        CurrentState =
            BossState.Idle;

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

    private void UpdateWander(
        float distanceToTarget)
    {
        if (HasTarget() &&
            distanceToTarget <= detectRange)
        {
            CurrentState =
                BossState.Chase;

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

    private void BeginWander()
    {
        if (!IsActivated)
        {
            CurrentState =
                BossState.Dormant;

            StopMove();
            return;
        }

        stuckTimer = 0f;

        lastPosition =
            transform.position;

        ChooseRandomPoint();

        CurrentState =
            BossState.Wander;
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

        if (direction.sqrMagnitude <
            0.1f)
        {
            direction =
                Random.value < 0.5f
                    ? Vector2.right
                    : Vector2.left;
        }

        direction.Normalize();

        float randomDistance =
            Random.Range(
                safeRadius * 0.3f,
                safeRadius
            );

        wanderTargetPosition =
            spawnPosition +
            direction *
            randomDistance;
    }

    private void UpdateChase(
        float distanceToTarget)
    {
        if (!HasTarget())
        {
            TargetLost();
            return;
        }

        if (distanceToTarget >
            detectRange)
        {
            TargetLost();
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

    private void TargetLost()
    {
        if (stayActivated)
        {
            if (wanderWhenTargetLost)
            {
                EnterIdle();
            }
            else
            {
                EnterReturn();
            }

            return;
        }

        IsActivated = false;

        EnterReturn();
    }

    private void EnterReturn()
    {
        CurrentState =
            BossState.Return;
    }

    private void UpdateReturn(
        float distanceToTarget)
    {
        if (HasTarget() &&
            distanceToTarget <= detectRange)
        {
            CurrentState =
                BossState.Chase;

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
            if (IsActivated)
            {
                EnterIdle();
            }
            else
            {
                CurrentState =
                    BossState.Dormant;

                StopMove();
            }

            return;
        }

        MoveTo(
            spawnPosition
        );
    }

    private void UpdateTargetSearch()
    {
        /*
         * Trong thời gian khóa detection
         * sau Play Again thì không scan.
         */
        if (!IsActivated &&
            detectionLockTimer > 0f)
        {
            return;
        }

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

                if (!IsActivated &&
                    !cloneCanActivateBoss &&
                    !IsPlayerTarget(candidate))
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

    private Transform FindPlayer()
    {
        GameObject playerObject;

        try
        {
            playerObject =
                GameObject
                    .FindGameObjectWithTag(
                        "Player"
                    );
        }
        catch (UnityException)
        {
            return null;
        }

        if (playerObject == null)
            return null;

        return playerObject.transform;
    }

    private bool IsPlayerTarget(
        Transform possibleTarget)
    {
        if (possibleTarget == null)
            return false;

        return possibleTarget
            .CompareTag("Player");
    }

    public void SetTarget(
        Transform newTarget)
    {
        target =
            newTarget;

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
                .GetComponentInParent<
                    CloneHealth
                >();

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

        float range =
            Mathf.Max(
                0f,
                detectRange
            );

        return (
            target.position -
            transform.position
        ).sqrMagnitude <=
        range * range;
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

        FaceDirection(
            direction
        );
    }

    private void FaceDirection(
        Vector2 direction)
    {
        if (Mathf.Abs(
                direction.x) <= 0.01f)
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


    public void ResetBoss()
    {
        StopAllCoroutines();

        CacheComponents();

        /*
         * =========================
         * RESET AI
         * =========================
         */

        IsActivated = false;

        CanMove = true;
        stoppedImmediately = false;

        CurrentState =
            BossState.Dormant;

        /*
         * Xóa target cũ.
         */
        target = null;
        player = null;

        targetSearchTimer =
            targetSearchInterval;

        idleTimer = 0f;
        stuckTimer = 0f;

        desiredVelocity =
            Vector2.zero;

        externalVelocity =
            Vector2.zero;

        /*
         * =========================
         * PLAY AGAIN DETECTION
         * =========================
         */

        detectionLockTimer =
            Mathf.Max(
                0f,
                respawnDetectionDelay
            );

        /*
         * Đây là dòng quan trọng nhất.
         *
         * Boss phải thấy Player RA NGOÀI
         * Detect Range trước khi được phép
         * activate lại.
         */
        requirePlayerExitBeforeReactivate =
            true;

        /*
         * =========================
         * RESET POSITION
         * =========================
         */

        if (rb != null)
        {
            rb.position =
                spawnPosition;

            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }
        else
        {
            transform.position =
                spawnPosition;
        }

        lastPosition =
            spawnPosition;

        /*
         * =========================
         * RESET ANIMATOR
         * =========================
         */

        if (animator != null)
        {
            animator.speed = 1f;

            animator.Rebind();
            animator.Update(0f);

            animator.SetBool(
                "IsMoving",
                false
            );

            animator.ResetTrigger(
                "Attack"
            );

            animator.ResetTrigger(
                "Slam"
            );

            animator.ResetTrigger(
                "Hurt"
            );

            animator.ResetTrigger(
                "Death"
            );
        }

        /*
         * =========================
         * RESET AUDIO
         * =========================
         */

        if (enemyAudio != null)
        {
            enemyAudio.StopAudio();
        }

        StopMove();

        Debug.Log(
            $"{name}: Boss reset về Dormant. " +
            "Đang chờ Player rời Detect Range."
        );
    }



    public void ResumeAI()
    {
        stoppedImmediately = false;
        CanMove = true;

        if (!IsActivated)
        {
            CurrentState =
                BossState.Dormant;

            StopMove();
            return;
        }

        FindNearestTarget();

        if (IsTargetInDetectRange())
        {
            CurrentState =
                BossState.Chase;

            return;
        }

        if (wanderWhenTargetLost)
        {
            EnterIdle();
        }
        else
        {
            EnterReturn();
        }
    }

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

    private void CheckIfStuck()
    {
        if (CurrentState !=
            BossState.Wander)
        {
            stuckTimer = 0f;

            lastPosition =
                transform.position;

            return;
        }

        float moved =
            Vector2.Distance(
                lastPosition,
                transform.position
            );

        if (moved < 0.02f)
        {
            stuckTimer +=
                Time.deltaTime;

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

        lastPosition =
            transform.position;
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );

        Gizmos.color =
            Color.cyan;

        Vector3 center =
            Application.isPlaying
                ? (Vector3)spawnPosition
                : transform.position;

        Gizmos.DrawWireSphere(
            center,
            roamRadius
        );
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
        respawnDetectionDelay =
Mathf.Max(
    0f,
    respawnDetectionDelay
);
    }
}