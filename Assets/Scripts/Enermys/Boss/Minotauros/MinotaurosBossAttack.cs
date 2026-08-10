using System.Collections.Generic;
using UnityEngine;

public class MinotaurosBossAttack : MonoBehaviour
{
    private enum AttackType
    {
        None,
        Normal,
        GroundSlam
    }

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

    [Header("Target")]
    [SerializeField]
    private LayerMask targetLayer;

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

    [Header("Ground Slam Wave")]
    [SerializeField]
    private Transform slamSpawnPoint;

    [SerializeField]
    private GameObject slamEffectPrefab;

    [Min(0f)]
    [SerializeField]
    private float slamWaveSpeed = 6f;

    [SerializeField]
    private float waveSpawnOffset = 0.25f;

    [Header("Slam Audio")]
    [SerializeField]
    private AudioClip slamSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slamVolume = 0.9f;

    [Header("Safety")]
    [Min(0.1f)]
    [SerializeField]
    private float actionTimeout = 1.5f;

    [Header("Animator")]
    [SerializeField]
    private string normalAttackTrigger =
        "Attack";

    [SerializeField]
    private string slamTrigger =
        "Slam";

    private AttackType currentAttack =
        AttackType.None;

    private float normalCooldownTimer;
    private float slamCooldownTimer;
    private float actionTimeoutTimer;

    private bool isActing;

    private readonly HashSet<GameObject>
        damagedTargets =
            new HashSet<GameObject>();

    public bool IsActing =>
        isActing;

    private void Awake()
    {
        CacheComponents();
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

        /*
         * Boss chưa được Player đánh thức
         * thì tuyệt đối không Attack.
         */
        if (!movement.IsActivated)
        {
            if (isActing)
            {
                CancelAction();
            }

            return;
        }

        /*
         * Boss hiện không Hurt Lock,
         * nhưng vẫn giữ check an toàn.
         */
        if (health.IsHurting)
        {
            if (isActing)
            {
                CancelAction();
            }

            return;
        }

        if (isActing)
        {
            movement.StopMove();

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

        bool inNormalAttackRange =
            distance <=
            movement.attackRange;

        bool inSlamRange =
            distance <=
            Mathf.Max(
                movement.attackRange,
                slamUseRange
            );

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
                GetComponent<Animator>();
        }

        if (bossAudio == null)
        {
            bossAudio =
                GetComponent<EnermyAudio>();
        }
    }

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
    }

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
            slamCooldownTimer =
                0.5f;

            return;
        }

        movement.StopMove();
        movement.FaceTarget();

        BeginGroundSlam();
    }

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

    public void ResetBossAttack()
    {
        isActing = false;

        currentAttack =
            AttackType.None;

        normalCooldownTimer = 0f;
        slamCooldownTimer = 0f;
        actionTimeoutTimer = 0f;

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
            movement.CanMove = true;
        }
    }
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
            waveObject
                .GetComponent<
                    GroundSlamWave
                >();

        if (wave == null)
        {
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

            if (!damagedTargets.Add(
                    rootObject))
            {
                continue;
            }

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

            Health playerHealth =
                hit.GetComponentInParent<
                    Health
                >();

            if (playerHealth != null &&
                !playerHealth.IsDead)
            {
                float before =
                    playerHealth
                        .currentHealth;

                playerHealth.TakeDamage(
                    damageAmount
                );

                if (playerHealth.currentHealth <
                    before)
                {
                    damagedAtLeastOneTarget =
                        true;
                }
            }
        }

        if (damagedAtLeastOneTarget &&
            bossAudio != null)
        {
            bossAudio
                .PlayAttackImpact();
        }
    }

    public void EndAction()
    {
        if (!isActing)
            return;

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

    public void CancelAction()
    {
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

    private void OnDisable()
    {
        isActing =
            false;

        currentAttack =
            AttackType.None;

        damagedTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (normalAttackPoint != null)
        {
            Gizmos.color =
                Color.red;

            Gizmos.DrawWireSphere(
                normalAttackPoint.position,
                normalAttackRadius
            );
        }

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            slamRadius
        );
    }

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

        actionTimeout =
            Mathf.Max(
                0.1f,
                actionTimeout
            );
    }
}