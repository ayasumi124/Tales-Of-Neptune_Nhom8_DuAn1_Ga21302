using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinotaurosBossAttack : MonoBehaviour
{
    private enum AttackType
    {
        None,
        Normal,
        GroundSlam,
        JumpSlam
    }

    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField]
    private BossMovement movement;

    [SerializeField]
    private EnermyHealth health;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private EnermyAudio bossAudio;

    [SerializeField]
    private Transform normalAttackPoint;

    // =====================================================
    // TARGET
    // =====================================================

    [Header("Target")]
    [SerializeField]
    private LayerMask targetLayer;

    // =====================================================
    // NORMAL ATTACK
    // =====================================================

    [Header("Normal Attack")]
    [Min(1)]
    [SerializeField]
    private int normalDamage = 2;

    [Min(0f)]
    [SerializeField]
    private float normalAttackRadius = 0.75f;

    [Min(0.05f)]
    [SerializeField]
    private float normalAttackCooldown = 1.3f;

    // =====================================================
    // GROUND SLAM
    // =====================================================

    [Header("Ground Slam")]
    [Min(1)]
    [SerializeField]
    private int slamDamage = 3;

    [Min(0f)]
    [SerializeField]
    private float slamUseRange = 7f;

    [Min(0f)]
    [SerializeField]
    private float slamRadius = 2f;

    [Range(0f, 100f)]
    [SerializeField]
    private float slamChance = 25f;

    [Range(0f, 100f)]
    [SerializeField]
    private float distantSlamChance = 60f;

    [Min(0.05f)]
    [SerializeField]
    private float slamCooldown = 7f;

    // =====================================================
    // GROUND SLAM WAVE
    // =====================================================

    [Header("Ground Slam Wave")]
    [SerializeField]
    private Transform slamSpawnPoint;

    [SerializeField]
    private GameObject slamEffectPrefab;

    [Min(0f)]
    [SerializeField]
    private float slamWaveSpeed = 6f;

    [Min(0f)]
    [SerializeField]
    private float waveSpawnOffset = 0.25f;

    [Header("Jump Slam Anti-Cheese Points")]
    [SerializeField]
    private Transform jumpSlamTopPoint;

    [SerializeField]
    private Transform jumpSlamBottomPoint;

    [Min(0f)]
    [SerializeField]
    private float jumpSlamPointRange = 0.8f;

    // =====================================================
    // GROUND SLAM AUDIO
    // =====================================================

    [Header("Slam Audio")]
    [SerializeField]
    private AudioClip slamSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slamVolume = 0.9f;

    // =====================================================
    // JUMP SLAM
    // =====================================================

    [Header("Jump Slam")]
    [Tooltip(
        "Bật skill Jump Slam chống Player đứng quá sát Boss."
    )]
    [SerializeField]
    private bool useJumpSlam = true;


    [Tooltip(
        "Tỷ lệ Boss sử dụng Jump Slam khi Player đứng sát."
    )]
    [Range(0f, 100f)]
    [SerializeField]
    private float jumpSlamChance = 70f;

    [Min(0.05f)]
    [SerializeField]
    private float jumpSlamCooldown = 5f;

    [Min(1)]
    [SerializeField]
    private int jumpSlamDamage = 3;

    [Tooltip(
        "Bán kính damage 360 độ quanh Boss khi đáp xuống."
    )]
    [Min(0f)]
    [SerializeField]
    private float jumpSlamRadius = 1.6f;

    // =====================================================
    // JUMP MOTION
    // =====================================================

    [Header("Jump Slam Motion")]
    [Tooltip(
        "Child chứa Sprite/Animator của Boss. " +
        "KHÔNG kéo Root Minotauros vào đây."
    )]
    [SerializeField]
    private Transform bossVisual;

    [Tooltip(
        "Độ cao Visual được nâng lên khi Boss nhảy."
    )]
    [Min(0f)]
    [SerializeField]
    private float jumpVisualHeight = 1.5f;

    [Tooltip(
        "Thời gian Boss nhảy lên."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float jumpUpDuration = 0.3f;

    [Tooltip(
        "Boss treo trên không trước khi lao xuống."
    )]
    [Min(0f)]
    [SerializeField]
    private float airWaitDuration = 0.15f;

    [Tooltip(
        "Thời gian lao xuống vị trí đã khóa của Player."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float slamDownDuration = 0.2f;

    [Tooltip(
        "Có lao đến vị trí Player hay chỉ nhảy tại chỗ."
    )]
    [SerializeField]
    private bool jumpTowardPlayer = true;

    // =====================================================
    // JUMP SLAM EFFECT
    // =====================================================

    [Header("Jump Slam Effect")]
    [Tooltip(
        "Prefab hiệu ứng impact 360 độ khi Boss đáp đất."
    )]
    [SerializeField]
    private GameObject jumpSlamEffectPrefab;

    [Tooltip(
        "Điểm spawn effect, nên đặt ở giữa chân Boss."
    )]
    [SerializeField]
    private Transform jumpSlamEffectPoint;

    [SerializeField]
    private AudioClip jumpSlamImpactSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float jumpSlamImpactVolume = 1f;

    // =====================================================
    // SAFETY
    // =====================================================

    [Header("Safety")]
    [Min(0.1f)]
    [SerializeField]
    private float actionTimeout = 1.5f;

    // =====================================================
    // ANIMATOR
    // =====================================================

    [Header("Animator")]
    [SerializeField]
    private string normalAttackTrigger =
        "Attack";

    [SerializeField]
    private string slamTrigger =
        "Slam";

    /*
     * Jump Slam hiện không cần animation riêng.
     *
     * Nếu muốn Boss dùng animation Slam lúc chuẩn bị
     * nhảy thì bật option bên dưới.
     */
    [SerializeField]
    private bool playSlamAnimationOnJump = false;

    // =====================================================
    // RUNTIME
    // =====================================================

    private AttackType currentAttack =
        AttackType.None;

    private float normalCooldownTimer;
    private float slamCooldownTimer;
    private float jumpSlamCooldownTimer;

    private float actionTimeoutTimer;

    private bool isActing;

    private Coroutine jumpSlamCoroutine;

    private Vector3 visualStartLocalPosition;

    private readonly HashSet<GameObject>
        damagedTargets =
            new HashSet<GameObject>();

    public bool IsActing =>
        isActing;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        CacheComponents();

        CacheVisualPosition();
    }

    private void Start()
    {
        CacheVisualPosition();
    }

    private void Update()
    {
        UpdateTimers();

        if (movement == null ||
            health == null ||
            health.IsDead)
        {
            return;
        }

        if (!movement.IsActivated)
        {
            if (isActing)
            {
                CancelAction();
            }

            return;
        }

        if (health.IsHurting)
        {
            if (isActing)
            {
                CancelAction();
            }

            return;
        }

        // ==========================================
        // ĐANG THỰC HIỆN ATTACK
        // ==========================================

        if (isActing)
        {
            movement.StopMove();

            /*
             * Jump Slam tự quản lý timing
             * bằng Coroutine.
             */
            if (currentAttack ==
                AttackType.JumpSlam)
            {
                return;
            }

            actionTimeoutTimer -=
                Time.deltaTime;

            if (actionTimeoutTimer <= 0f)
            {
                EndAction();
            }

            return;
        }

        if (!movement.HasTarget())
            return;

        float distance =
            movement.DistanceToTarget();

        // ==========================================
        // JUMP SLAM - ƯU TIÊN CAO NHẤT
        // ==========================================

        /*
         * Không phụ thuộc normal cooldown.
         *
         * Player dí quá sát Boss thì
         * kiểm tra Jump Slam ngay.
         */
        if (TryJumpSlam())
{
    return;
}

        // ==========================================
        // NORMAL / GROUND SLAM
        // ==========================================

        bool inNormalAttackRange =
            distance <=
            movement.attackRange;

        bool inSlamRange =
            distance <=
            Mathf.Max(
                movement.attackRange,
                slamUseRange
            );

        /*
         * Player ở xa:
         * Boss có thể dùng Ground Slam Wave.
         */
        if (!inNormalAttackRange)
        {
            TryDistantSlam(
                inSlamRange
            );

            return;
        }

        movement.StopMove();
        movement.FaceTarget();

        TryChooseCloseAttack();
    }

    // =====================================================
    // CACHE
    // =====================================================

    private void CacheComponents()
    {
        if (movement == null)
        {
            movement =
                GetComponent<BossMovement>();
        }

        if (health == null)
        {
            health =
                GetComponent<EnermyHealth>();
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (bossAudio == null)
        {
            bossAudio =
                GetComponent<EnermyAudio>();
        }
    }

    private void CacheVisualPosition()
    {
        if (bossVisual == null)
            return;

        visualStartLocalPosition =
            bossVisual.localPosition;
    }

    // =====================================================
    // TIMERS
    // =====================================================

    private void UpdateTimers()
    {
        if (normalCooldownTimer > 0f)
        {
            normalCooldownTimer -=
                Time.deltaTime;
        }

        if (slamCooldownTimer > 0f)
        {
            slamCooldownTimer -=
                Time.deltaTime;
        }

        if (jumpSlamCooldownTimer > 0f)
        {
            jumpSlamCooldownTimer -=
                Time.deltaTime;
        }
    }

    // =====================================================
    // DISTANT ATTACK
    // =====================================================

    private void TryDistantSlam(
        bool inSlamRange)
    {
        if (!inSlamRange ||
            slamCooldownTimer > 0f)
        {
            return;
        }

        float roll =
            Random.Range(
                0f,
                100f
            );

        if (roll >
            distantSlamChance)
        {
            /*
             * Không roll liên tục mỗi frame.
             */
            slamCooldownTimer =
                0.5f;

            return;
        }

        movement.StopMove();
        movement.FaceTarget();

        BeginGroundSlam();
    }

    // =====================================================
    // CLOSE ATTACK CHOICE
    // =====================================================

    private void TryChooseCloseAttack()
    {
        if (normalCooldownTimer > 0f)
            return;

        bool canUseSlam =
            slamCooldownTimer <= 0f;

        bool chooseSlam =
            canUseSlam &&
            Random.Range(
                0f,
                100f
            ) <= slamChance;

        if (chooseSlam)
        {
            BeginGroundSlam();
        }
        else
        {
            BeginNormalAttack();
        }
    }

    // =====================================================
    // NORMAL ATTACK
    // =====================================================

    private void BeginNormalAttack()
    {
        BeginAction(
            AttackType.Normal
        );

        if (animator != null)
        {
            animator.ResetTrigger(
                slamTrigger
            );

            animator.ResetTrigger(
                normalAttackTrigger
            );

            animator.SetTrigger(
                normalAttackTrigger
            );
        }
        else
        {
            DealNormalDamage();

            EndAction();
        }
    }

    // =====================================================
    // GROUND SLAM
    // =====================================================

    private void BeginGroundSlam()
    {
        BeginAction(
            AttackType.GroundSlam
        );

        slamCooldownTimer =
            Mathf.Max(
                0.05f,
                slamCooldown
            );

        if (animator != null)
        {
            animator.ResetTrigger(
                normalAttackTrigger
            );

            animator.ResetTrigger(
                slamTrigger
            );

            animator.SetTrigger(
                slamTrigger
            );
        }
        else
        {
            GroundSlamImpact();

            EndAction();
        }
    }

    // =====================================================
    // JUMP SLAM - CHOOSE
    // =====================================================

    private bool TryJumpSlam()
{
    if (!useJumpSlam)
        return false;

    if (isActing)
        return false;

    if (jumpSlamCooldownTimer > 0f)
        return false;

    if (movement == null ||
        !movement.HasTarget() ||
        movement.Target == null)
    {
        return false;
    }

    /*
     * Jump Slam KHÔNG còn kiểm tra
     * khoảng cách tới Root Boss.
     *
     * Chỉ kích hoạt nếu Player:
     * - đứng trên đầu Boss;
     * - hoặc đứng dưới chân Boss.
     */
    if (!IsTargetInJumpSlamZone())
    {
        return false;
    }

    float roll =
        Random.Range(
            0f,
            100f
        );

    if (roll > jumpSlamChance)
    {
        /*
         * Tránh roll lại 60 lần/giây.
         */
        jumpSlamCooldownTimer =
            0.35f;

        return false;
    }

    Debug.Log(
        $"{name}: JUMP SLAM anti-cheese!"
    );

    BeginJumpSlam();

    return true;
}
private bool IsTargetInJumpSlamZone()
{
    if (movement == null ||
        !movement.HasTarget() ||
        movement.Target == null)
    {
        return false;
    }

    Vector2 targetPosition =
        movement.Target.position;

    float range =
        Mathf.Max(
            0f,
            jumpSlamPointRange
        );

    float rangeSqr =
        range * range;

    // ==========================================
    // TOP POINT
    // ==========================================

    if (jumpSlamTopPoint != null)
    {
        Vector2 topPosition =
            jumpSlamTopPoint.position;

        float topDistanceSqr =
            (
                targetPosition -
                topPosition
            ).sqrMagnitude;

        if (topDistanceSqr <= rangeSqr)
        {
            return true;
        }
    }

    // ==========================================
    // BOTTOM POINT
    // ==========================================

    if (jumpSlamBottomPoint != null)
    {
        Vector2 bottomPosition =
            jumpSlamBottomPoint.position;

        float bottomDistanceSqr =
            (
                targetPosition -
                bottomPosition
            ).sqrMagnitude;

        if (bottomDistanceSqr <= rangeSqr)
        {
            return true;
        }
    }

    return false;
}

    // =====================================================
    // JUMP SLAM - START
    // =====================================================

    private void BeginJumpSlam()
    {
        if (isActing)
            return;

        if (movement == null ||
            !movement.HasTarget())
        {
            return;
        }

        BeginAction(
            AttackType.JumpSlam
        );

        jumpSlamCooldownTimer =
            Mathf.Max(
                0.05f,
                jumpSlamCooldown
            );

        /*
         * Có thể dùng animation Slam như động tác
         * lấy đà nếu muốn.
         */
        if (playSlamAnimationOnJump &&
            animator != null)
        {
            animator.ResetTrigger(
                normalAttackTrigger
            );

            animator.ResetTrigger(
                slamTrigger
            );

            animator.SetTrigger(
                slamTrigger
            );
        }

        if (jumpSlamCoroutine != null)
        {
            StopCoroutine(
                jumpSlamCoroutine
            );

            jumpSlamCoroutine =
                null;
        }

        jumpSlamCoroutine =
            StartCoroutine(
                JumpSlamRoutine()
            );
    }

    // =====================================================
    // JUMP SLAM - COROUTINE
    // =====================================================

    private IEnumerator JumpSlamRoutine()
    {
        if (movement == null ||
            !movement.HasTarget())
        {
            FinishJumpSlam();

            yield break;
        }

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                $"{name}: Jump Slam cần Rigidbody2D.",
                this
            );

            FinishJumpSlam();

            yield break;
        }

        movement.StopMove();
        movement.FaceTarget();

        /*
         * Vị trí Boss lúc bắt đầu nhảy.
         */
        Vector2 startPosition =
            rb.position;

        /*
         * Khóa vị trí Player NGAY KHI bắt đầu skill.
         *
         * Player có thể né sau đó.
         */
        Vector2 landingPosition =
            startPosition;

        if (jumpTowardPlayer &&
            movement.Target != null)
        {
            landingPosition =
                movement.Target.position;
        }

        /*
         * Giữ Visual về đúng vị trí trước khi nhảy.
         */
        if (bossVisual != null)
        {
            visualStartLocalPosition =
                bossVisual.localPosition;
        }

        // =================================================
        // PHASE 1 - NHẢY LÊN
        // =================================================

        float timer = 0f;

        float upDuration =
            Mathf.Max(
                0.01f,
                jumpUpDuration
            );

        while (timer < upDuration)
        {
            if (health == null ||
                health.IsDead)
            {
                RestoreJumpVisual();

                jumpSlamCoroutine =
                    null;

                yield break;
            }

            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    upDuration
                );

            t =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            /*
             * Chỉ nâng CHILD VISUAL.
             *
             * Không dịch Root theo trục Y vì đây là
             * game top-down.
             */
            if (bossVisual != null)
            {
                bossVisual.localPosition =
                    visualStartLocalPosition +
                    Vector3.up *
                    Mathf.Lerp(
                        0f,
                        jumpVisualHeight,
                        t
                    );
            }

            yield return null;
        }

        // =================================================
        // PHASE 2 - TREO TRÊN KHÔNG
        // =================================================

        yield return
            new WaitForSeconds(
                Mathf.Max(
                    0f,
                    airWaitDuration
                )
            );

        if (health == null ||
            health.IsDead)
        {
            RestoreJumpVisual();

            jumpSlamCoroutine =
                null;

            yield break;
        }

        // =================================================
        // PHASE 3 - LAO XUỐNG
        // =================================================

        timer = 0f;

        float downDuration =
            Mathf.Max(
                0.01f,
                slamDownDuration
            );

        while (timer < downDuration)
        {
            if (health == null ||
                health.IsDead)
            {
                RestoreJumpVisual();

                jumpSlamCoroutine =
                    null;

                yield break;
            }

            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    downDuration
                );

            /*
             * Boss Root lao tới vị trí Player
             * đã được khóa từ đầu skill.
             */
            Vector2 newPosition =
                Vector2.Lerp(
                    startPosition,
                    landingPosition,
                    t
                );

            rb.MovePosition(
                newPosition
            );

            /*
             * Visual đồng thời hạ xuống.
             */
            if (bossVisual != null)
            {
                bossVisual.localPosition =
                    visualStartLocalPosition +
                    Vector3.up *
                    Mathf.Lerp(
                        jumpVisualHeight,
                        0f,
                        t
                    );
            }

            yield return null;
        }

        /*
         * Đảm bảo Boss đáp đúng vị trí cuối.
         */
        rb.position =
            landingPosition;

        rb.linearVelocity =
            Vector2.zero;

        RestoreJumpVisual();

        // =================================================
        // IMPACT
        // =================================================

        JumpSlamImpact();

        /*
         * Pause cực ngắn để impact có trọng lượng.
         */
        yield return
            new WaitForSeconds(
                0.1f
            );

        jumpSlamCoroutine =
            null;

        FinishJumpSlam();
    }

    // =====================================================
    // BEGIN ACTION
    // =====================================================

    private void BeginAction(
        AttackType attackType)
    {
        isActing = true;

        currentAttack =
            attackType;

        damagedTargets.Clear();

        actionTimeoutTimer =
            Mathf.Max(
                0.1f,
                actionTimeout
            );

        if (movement != null)
        {
            movement.CanMove =
                false;

            movement.StopMove();
            movement.FaceTarget();
        }
    }

    // =====================================================
    // NORMAL DAMAGE
    // Animation Event
    // =====================================================

    public void DealNormalDamage()
    {
        if (!isActing ||
            currentAttack !=
            AttackType.Normal)
        {
            return;
        }

        if (normalAttackPoint == null)
            return;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                normalAttackPoint.position,
                normalAttackRadius,
                targetLayer
            );

        DamageTargets(
            hits,
            normalDamage
        );
    }

    // =====================================================
    // GROUND SLAM IMPACT
    // Animation Event
    // =====================================================

    public void GroundSlamImpact()
    {
        if (!isActing ||
            currentAttack !=
            AttackType.GroundSlam)
        {
            return;
        }

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                slamRadius,
                targetLayer
            );

        DamageTargets(
            hits,
            slamDamage
        );

        SpawnSlamWaves();

        PlaySlamSound();
    }

    // =====================================================
    // GROUND SLAM WAVES
    // =====================================================

    private void SpawnSlamWaves()
    {
        if (slamEffectPrefab == null)
            return;

        Vector3 spawnPosition =
            slamSpawnPoint != null
                ? slamSpawnPoint.position
                : transform.position;

        float offset =
            Mathf.Max(
                0f,
                waveSpawnOffset
            );

        SpawnWave(
            spawnPosition +
            Vector3.left * offset,
            Vector2.left
        );

        SpawnWave(
            spawnPosition +
            Vector3.right * offset,
            Vector2.right
        );
    }

    private void SpawnWave(
        Vector3 position,
        Vector2 direction)
    {
        GameObject waveObject =
            Instantiate(
                slamEffectPrefab,
                position,
                Quaternion.identity
            );

        GroundSlamWave wave =
            waveObject.GetComponent<
                GroundSlamWave
            >();

        if (wave == null)
        {
            Debug.LogError(
                $"{waveObject.name} thiếu GroundSlamWave.",
                waveObject
            );

            Destroy(
                waveObject
            );

            return;
        }

        wave.Initialize(
            direction,
            gameObject,
            slamDamage,
            slamWaveSpeed
        );
    }

    // =====================================================
    // GROUND SLAM SOUND
    // =====================================================

    private void PlaySlamSound()
    {
        if (slamSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    slamSound,
                    slamVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            slamSound,
            transform.position,
            slamVolume
        );
    }

    // =====================================================
    // JUMP SLAM IMPACT
    // =====================================================

    private void JumpSlamImpact()
    {
        if (!isActing ||
            currentAttack !=
            AttackType.JumpSlam)
        {
            return;
        }

        /*
         * Jump Slam là một hit mới.
         */
        damagedTargets.Clear();

        /*
         * AoE 360° quanh toàn thân Boss.
         */
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                Mathf.Max(
                    0f,
                    jumpSlamRadius
                ),
                targetLayer
            );

        DamageTargets(
            hits,
            jumpSlamDamage
        );

        SpawnJumpSlamEffect();

        PlayJumpSlamImpactSound();
    }

    // =====================================================
    // JUMP SLAM EFFECT
    // =====================================================

    private void SpawnJumpSlamEffect()
    {
        if (jumpSlamEffectPrefab == null)
            return;

        Vector3 spawnPosition =
            jumpSlamEffectPoint != null
                ? jumpSlamEffectPoint.position
                : transform.position;

        Instantiate(
            jumpSlamEffectPrefab,
            spawnPosition,
            Quaternion.identity
        );
    }

    // =====================================================
    // JUMP SLAM AUDIO
    // =====================================================

    private void PlayJumpSlamImpactSound()
    {
        AudioClip clip =
            jumpSlamImpactSound != null
                ? jumpSlamImpactSound
                : slamSound;

        if (clip == null)
            return;

        float volume =
            jumpSlamImpactSound != null
                ? jumpSlamImpactVolume
                : slamVolume;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    clip,
                    volume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            volume
        );
    }

    // =====================================================
    // DAMAGE
    // =====================================================

    private void DamageTargets(
        Collider2D[] hits,
        int damageAmount)
    {
        if (hits == null)
            return;

        bool damagedAtLeastOneTarget =
            false;

        foreach (Collider2D hit
                 in hits)
        {
            if (hit == null)
                continue;

            GameObject rootObject =
                hit.transform.root.gameObject;

            /*
             * Một target có nhiều Collider
             * chỉ nhận damage một lần.
             */
            if (!damagedTargets.Add(
                    rootObject))
            {
                continue;
            }

            // ---------------------------------------------
            // CLONE
            // ---------------------------------------------

            CloneHealth cloneHealth =
                hit.GetComponentInParent<
                    CloneHealth
                >();

            if (cloneHealth != null)
            {
                cloneHealth.TakeDamage(
                    damageAmount
                );

                damagedAtLeastOneTarget =
                    true;

                continue;
            }

            // ---------------------------------------------
            // PLAYER
            // ---------------------------------------------

            Health playerHealth =
                hit.GetComponentInParent<
                    Health
                >();

            if (playerHealth != null &&
                !playerHealth.IsDead)
            {
                float before =
                    playerHealth.currentHealth;

                playerHealth.TakeDamage(
                    damageAmount
                );

                /*
                 * Player có thể đang Dash hoặc Invincible.
                 */
                if (playerHealth.currentHealth <
                    before)
                {
                    damagedAtLeastOneTarget =
                        true;
                }
            }
        }

        /*
         * Chỉ phát Impact khi thực sự gây damage.
         */
        if (damagedAtLeastOneTarget &&
            bossAudio != null)
        {
            bossAudio.PlayAttackImpact();
        }
    }

    // =====================================================
    // FINISH JUMP SLAM
    // =====================================================

    private void FinishJumpSlam()
    {
        RestoreJumpVisual();

        currentAttack =
            AttackType.None;

        isActing =
            false;

        damagedTargets.Clear();

        /*
         * Sau Jump Slam vẫn cho Normal Attack
         * nghỉ một chút.
         */
        normalCooldownTimer =
            Mathf.Max(
                0.05f,
                normalAttackCooldown
            );

        if (movement != null &&
            movement.enabled &&
            health != null &&
            !health.IsDead)
        {
            movement.CanMove =
                true;

            movement.ResumeAI();
        }
    }

    private void RestoreJumpVisual()
    {
        if (bossVisual == null)
            return;

        bossVisual.localPosition =
            visualStartLocalPosition;
    }

    // =====================================================
    // END ACTION
    // Animation Event cho Normal / Ground Slam
    // =====================================================

    public void EndAction()
    {
        if (!isActing)
            return;

        /*
         * Jump Slam tự kết thúc bằng Coroutine.
         *
         * Nếu animation Slam có EndAction Event,
         * không cho event đó vô tình kết thúc Jump Slam.
         */
        if (currentAttack ==
            AttackType.JumpSlam)
        {
            return;
        }

        AttackType finishedAttack =
            currentAttack;

        isActing =
            false;

        currentAttack =
            AttackType.None;

        damagedTargets.Clear();

        normalCooldownTimer =
            Mathf.Max(
                0.05f,
                normalAttackCooldown
            );

        if (finishedAttack ==
            AttackType.GroundSlam)
        {
            normalCooldownTimer +=
                0.3f;
        }

        if (movement != null &&
            movement.enabled &&
            health != null &&
            !health.IsDead)
        {
            movement.CanMove =
                true;

            movement.ResumeAI();
        }
    }

    // =====================================================
    // CANCEL
    // =====================================================

    public void CancelAction()
    {
        /*
         * Hủy Jump Slam nếu đang bay.
         */
        if (jumpSlamCoroutine != null)
        {
            StopCoroutine(
                jumpSlamCoroutine
            );

            jumpSlamCoroutine =
                null;
        }

        RestoreJumpVisual();

        isActing =
            false;

        currentAttack =
            AttackType.None;

        damagedTargets.Clear();

        if (animator != null)
        {
            animator.ResetTrigger(
                normalAttackTrigger
            );

            animator.ResetTrigger(
                slamTrigger
            );
        }

        if (movement != null &&
            movement.enabled &&
            health != null &&
            !health.IsDead)
        {
            movement.CanMove =
                true;

            movement.ResumeAI();
        }
    }

    // =====================================================
    // RESET BOSS ATTACK
    // Play Again
    // =====================================================

    public void ResetBossAttack()
    {
        /*
         * Nếu Boss đang Jump Slam khi Player chết.
         */
        if (jumpSlamCoroutine != null)
        {
            StopCoroutine(
                jumpSlamCoroutine
            );

            jumpSlamCoroutine =
                null;
        }

        RestoreJumpVisual();

        isActing =
            false;

        currentAttack =
            AttackType.None;

        normalCooldownTimer =
            0f;

        slamCooldownTimer =
            0f;

        jumpSlamCooldownTimer =
            0f;

        actionTimeoutTimer =
            0f;

        damagedTargets.Clear();

        if (animator != null)
        {
            animator.ResetTrigger(
                normalAttackTrigger
            );

            animator.ResetTrigger(
                slamTrigger
            );
        }

        if (movement != null)
        {
            movement.CanMove =
                true;

            movement.StopMove();
        }
    }

    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        if (jumpSlamCoroutine != null)
        {
            StopCoroutine(
                jumpSlamCoroutine
            );

            jumpSlamCoroutine =
                null;
        }

        RestoreJumpVisual();

        isActing =
            false;

        currentAttack =
            AttackType.None;

        damagedTargets.Clear();
    }

    // =====================================================
    // GIZMOS
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        /*
         * Normal Attack.
         */
        if (normalAttackPoint != null)
        {
            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                normalAttackPoint.position,
                normalAttackRadius
            );
        }

        /*
         * Ground Slam.
         */
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            slamRadius
        );

        /*
         * Jump Slam AoE.
         */
        Gizmos.color =
            Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            jumpSlamRadius
        );

        /*
         * Jump Slam Trigger Range.
         */
        /*
 * Jump Slam Anti-Cheese - TOP.
 */
if (jumpSlamTopPoint != null)
{
    Gizmos.color =
        Color.cyan;

    Gizmos.DrawWireSphere(
        jumpSlamTopPoint.position,
        jumpSlamPointRange
    );
}

/*
 * Jump Slam Anti-Cheese - BOTTOM.
 */
if (jumpSlamBottomPoint != null)
{
    Gizmos.color =
        Color.green;

    Gizmos.DrawWireSphere(
        jumpSlamBottomPoint.position,
        jumpSlamPointRange
    );
}
    }

    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        normalDamage =
            Mathf.Max(
                1,
                normalDamage
            );

        normalAttackRadius =
            Mathf.Max(
                0f,
                normalAttackRadius
            );

        normalAttackCooldown =
            Mathf.Max(
                0.05f,
                normalAttackCooldown
            );

        slamDamage =
            Mathf.Max(
                1,
                slamDamage
            );

        slamUseRange =
            Mathf.Max(
                0f,
                slamUseRange
            );

        slamRadius =
            Mathf.Max(
                0f,
                slamRadius
            );

        slamChance =
            Mathf.Clamp(
                slamChance,
                0f,
                100f
            );

        distantSlamChance =
            Mathf.Clamp(
                distantSlamChance,
                0f,
                100f
            );

        slamCooldown =
            Mathf.Max(
                0.05f,
                slamCooldown
            );

        slamWaveSpeed =
            Mathf.Max(
                0f,
                slamWaveSpeed
            );

        waveSpawnOffset =
            Mathf.Max(
                0f,
                waveSpawnOffset
            );

        slamVolume =
            Mathf.Clamp01(
                slamVolume
            );

        jumpSlamPointRange =
    Mathf.Max(
        0f,
        jumpSlamPointRange
    );

        jumpSlamChance =
            Mathf.Clamp(
                jumpSlamChance,
                0f,
                100f
            );

        jumpSlamCooldown =
            Mathf.Max(
                0.05f,
                jumpSlamCooldown
            );

        jumpSlamDamage =
            Mathf.Max(
                1,
                jumpSlamDamage
            );

        jumpSlamRadius =
            Mathf.Max(
                0f,
                jumpSlamRadius
            );

        jumpVisualHeight =
            Mathf.Max(
                0f,
                jumpVisualHeight
            );

        jumpUpDuration =
            Mathf.Max(
                0.01f,
                jumpUpDuration
            );

        airWaitDuration =
            Mathf.Max(
                0f,
                airWaitDuration
            );

        slamDownDuration =
            Mathf.Max(
                0.01f,
                slamDownDuration
            );

        jumpSlamImpactVolume =
            Mathf.Clamp01(
                jumpSlamImpactVolume
            );

        actionTimeout =
            Mathf.Max(
                0.1f,
                actionTimeout
            );
    }
}