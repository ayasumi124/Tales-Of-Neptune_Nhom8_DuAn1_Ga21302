using System.Collections.Generic;
using UnityEngine;

public class IceGroundDamage : MonoBehaviour
{
    // =====================================================
    // EXPANSION
    // =====================================================

    [Header("Expansion")]
    [Min(0f)]
    [SerializeField]
    private float startRadius = 0.2f;

    [Min(0.1f)]
    [SerializeField]
    private float maxRadius = 2.2f;

    [Min(0.01f)]
    [SerializeField]
    private float expandDuration = 0.35f;


    // =====================================================
    // DAMAGE
    // =====================================================

    [Header("Damage")]

    [Tooltip(
        "Damage lần đầu IceGround lan tới Enemy."
    )]
    [Min(1)]
    [SerializeField]
    private int initialDamage = 35;

    [Tooltip(
        "Damage khi Enemy vẫn đứng trong IceGround " +
        "sau khi hết Freeze hoặc bước vào vùng sau đó."
    )]
    [Min(1)]
    [SerializeField]
    private int groundDamage = 10;

    [Min(0f)]
    [SerializeField]
    private float knockbackStrength = 4f;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Ground Audio")]
    [SerializeField]
    private AudioClip groundLoopSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float groundLoopVolume = 0.5f;

    [SerializeField]
    private AudioSource groundAudioSource;

    // =====================================================
    // FREEZE
    // =====================================================

    [Header("Freeze")]
    [Min(0.1f)]
    [SerializeField]
    private float freezeDuration = 2.5f;

    [SerializeField]
    private GameObject freezeVFXPrefab;


    // =====================================================
    // RE-HIT
    // =====================================================

    [Header("Re-Hit")]
    [Tooltip(
        "Khoảng nghỉ sau khi Enemy hết Freeze " +
        "trước khi IceGround có thể đánh lại."
    )]
    [Min(0f)]
    [SerializeField]
    private float reHitDelay = 0.1f;


    // =====================================================
    // RUNTIME
    // =====================================================

    private float timer;

    private float currentRadius;

    private bool expansionFinished;


    /*
     * Enemy đã từng bị cú Frost Nova đầu tiên.
     *
     * Có trong đây:
     * → lần sau chỉ nhận groundDamage.
     */
    private readonly HashSet<EnermyHealth>
        initiallyHitEnemies =
            new HashSet<EnermyHealth>();


    /*
     * Tránh Enemy có nhiều Collider bị
     * xử lý nhiều lần trong cùng một frame.
     */
    private readonly HashSet<EnermyHealth>
        processedThisFrame =
            new HashSet<EnermyHealth>();


    /*
     * Thời điểm sớm nhất Enemy
     * có thể bị IceGround đánh lại.
     */
    private readonly Dictionary<EnermyHealth, float>
        nextHitTimes =
            new Dictionary<EnermyHealth, float>();


    // =====================================================
    // ENABLE
    // =====================================================

    private void OnEnable()
    {
        timer = 0f;

        currentRadius =
            startRadius;

        expansionFinished =
            false;

        initiallyHitEnemies.Clear();

        processedThisFrame.Clear();

        nextHitTimes.Clear();

        SetupGroundAudio();

        PlayGroundLoop();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        UpdateExpansion();

        CheckEnemies();
    }


    // =====================================================
    // EXPANSION
    // =====================================================

    private void UpdateExpansion()
    {
        if (expansionFinished)
        {
            currentRadius =
                maxRadius;

            return;
        }

        timer +=
            Time.deltaTime;

        float normalized =
            Mathf.Clamp01(
                timer /
                Mathf.Max(
                    0.01f,
                    expandDuration
                )
            );

        currentRadius =
            Mathf.Lerp(
                startRadius,
                maxRadius,
                normalized
            );

        if (normalized >= 1f)
        {
            expansionFinished =
                true;

            currentRadius =
                maxRadius;
        }
    }


    // =====================================================
    // CHECK ENEMIES
    // =====================================================

    private void CheckEnemies()
    {
        processedThisFrame.Clear();

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                currentRadius,
                enemyLayer
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null)
                continue;

            /*
             * Enemy có nhiều Collider
             * cũng chỉ xử lý 1 lần/frame.
             */
            if (!processedThisFrame.Add(
                    enemy))
            {
                continue;
            }

            TryHitEnemy(
                enemy
            );
        }
    }


    // =====================================================
    // TRY HIT
    // =====================================================

    private void TryHitEnemy(
        EnermyHealth enemy)
    {
        if (enemy == null)
            return;

        EnemyFreezeEffect freeze =
            enemy.GetComponent<
                EnemyFreezeEffect
            >();

        // =================================================
        // ĐANG FREEZE
        // =================================================

        if (freeze != null &&
            freeze.IsFrozen)
        {
            /*
             * Không damage thêm trong lúc
             * Enemy vẫn còn Freeze.
             */
            return;
        }


        // =================================================
        // RE-HIT DELAY
        // =================================================

        if (nextHitTimes.TryGetValue(
                enemy,
                out float nextHitTime))
        {
            if (Time.time <
                nextHitTime)
            {
                return;
            }
        }


        // =================================================
        // CHỌN DAMAGE
        // =================================================

        bool firstHit =
            !initiallyHitEnemies.Contains(
                enemy
            );

        int finalDamage =
            firstHit
                ? initialDamage
                : groundDamage;


        HitEnemy(
            enemy,
            freeze,
            finalDamage,
            firstHit
        );
    }


    // =====================================================
    // HIT
    // =====================================================

    private void HitEnemy(
        EnermyHealth enemy,
        EnemyFreezeEffect freeze,
        int finalDamage,
        bool firstHit)
    {
        if (enemy == null)
            return;

        Vector2 knockDirection =
            (
                enemy.transform.position -
                transform.position
            ).normalized;

        if (knockDirection.sqrMagnitude <
            0.001f)
        {
            knockDirection =
                Vector2.down;
        }


        // =================================================
        // DAMAGE
        // =================================================

        enemy.TakeDamage(
            finalDamage,
            knockDirection,
            knockbackStrength,
            true
        );


        // =================================================
        // GHI NHẬN INITIAL HIT
        // =================================================

        if (firstHit)
        {
            initiallyHitEnemies.Add(
                enemy
            );

            Debug.Log(
                $"Frost Nova INITIAL HIT: " +
                $"{enemy.name} - " +
                $"{finalDamage} damage."
            );
        }
        else
        {
            Debug.Log(
                $"IceGround GROUND HIT: " +
                $"{enemy.name} - " +
                $"{finalDamage} damage."
            );
        }


        // =================================================
        // FREEZE
        // =================================================

        if (freeze == null)
        {
            freeze =
                enemy.gameObject
                    .AddComponent<
                        EnemyFreezeEffect
                    >();
        }

        freeze.ApplyFreeze(
            freezeDuration,
            freezeVFXPrefab
        );


        // =================================================
        // NEXT HIT
        // =================================================

        nextHitTimes[enemy] =
            Time.time +
            freezeDuration +
            reHitDelay;
    }


    // =====================================================
    // GIZMO
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.cyan;

        float drawRadius =
            Application.isPlaying
                ? currentRadius
                : maxRadius;

        Gizmos.DrawWireSphere(
            transform.position,
            drawRadius
        );
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        startRadius =
            Mathf.Max(
                0f,
                startRadius
            );

        maxRadius =
            Mathf.Max(
                startRadius,
                maxRadius
            );

        expandDuration =
            Mathf.Max(
                0.01f,
                expandDuration
            );

        initialDamage =
            Mathf.Max(
                1,
                initialDamage
            );

        groundDamage =
            Mathf.Max(
                1,
                groundDamage
            );

        knockbackStrength =
            Mathf.Max(
                0f,
                knockbackStrength
            );

        freezeDuration =
            Mathf.Max(
                0.1f,
                freezeDuration
            );

        reHitDelay =
            Mathf.Max(
                0f,
                reHitDelay
            );
    }

    private void SetupGroundAudio()
    {
        if (groundAudioSource == null)
        {
            groundAudioSource =
                GetComponent<AudioSource>();
        }

        if (groundAudioSource == null)
        {
            groundAudioSource =
                gameObject.AddComponent<AudioSource>();
        }

        groundAudioSource.playOnAwake =
            false;

        groundAudioSource.loop =
            true;

        groundAudioSource.spatialBlend =
            0f;
    }

    private void PlayGroundLoop()
    {
        if (groundLoopSound == null ||
            groundAudioSource == null)
        {
            return;
        }

        groundAudioSource.clip =
            groundLoopSound;

        groundAudioSource.volume =
            groundLoopVolume;

        groundAudioSource.Play();
    }

    private void StopGroundLoop()
    {
        if (groundAudioSource == null)
            return;

        groundAudioSource.Stop();
        groundAudioSource.clip = null;
    }

    private void OnDisable()
    {
        StopGroundLoop();
    }

    private void OnDestroy()
    {
        StopGroundLoop();
    }
}