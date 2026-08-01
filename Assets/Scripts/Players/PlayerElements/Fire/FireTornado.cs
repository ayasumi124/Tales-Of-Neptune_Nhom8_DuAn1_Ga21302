using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTornado : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Thời gian tồn tại của lốc lửa.")]
    [SerializeField] private float duration = 5f;

    [Header("Visual Layers")]
    [Tooltip("Prefab vòng lửa có SpriteRenderer và Animator.")]
    [SerializeField] private GameObject ringVisualPrefab;

    [Tooltip("Số vòng lửa xếp tầng.")]
    [SerializeField] private int layerCount = 5;

    [Tooltip("Khoảng cách theo trục Y giữa các tầng.")]
    [SerializeField] private float layerHeight = 0.18f;

    [Tooltip("Kích thước vòng thấp nhất.")]
    [SerializeField] private float bottomScale = 0.45f;

    [Tooltip("Mỗi tầng phía trên lớn thêm bao nhiêu.")]
    [SerializeField] private float scalePerLayer = 0.16f;

    [Tooltip("Lệch thời gian animation giữa các vòng.")]
    [SerializeField] private float animationDelayPerLayer = 0.05f;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Tooltip("Phạm vi tác động của Tornado.")]
    [SerializeField] private float effectRadius = 2.5f;

    [Header("Pull")]
    [Tooltip("Lực kéo Enemy về tâm.")]
    [SerializeField] private float pullForce = 8f;

    [Tooltip("Tốc độ kéo tối đa.")]
    [SerializeField] private float maxPullSpeed = 4f;

    [Tooltip(
        "Enemy đến gần tâm hơn khoảng này sẽ không bị kéo thêm."
    )]
    [SerializeField] private float pullStopDistance = 0.25f;

    [Header("Tornado Damage")]
    [SerializeField] private int damagePerTick = 10;

    [Tooltip("Khoảng thời gian giữa mỗi lần gây damage.")]
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Burn")]
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Final Fire Ground")]
    [Tooltip("Prefab mặt lửa được tạo khi Tornado kết thúc.")]
    [SerializeField] private GameObject fireGroundPrefab;

    [Header("Optional Audio")]
    [SerializeField] private AudioClip tornadoLoopSound;
    [SerializeField] private AudioClip tornadoEndSound;

    private GameObject owner;
    private Vector2 castDirection;

    private bool initialized;
    private bool ending;

    private Coroutine damageCoroutine;
    private AudioSource loopAudioSource;

    public void Initialize(
        Vector2 direction,
        GameObject skillOwner)
    {
        if (initialized)
            return;

        initialized = true;

        owner = skillOwner;

        castDirection =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        CreateVisualLayers();
        StartLoopSound();

        damageCoroutine =
            StartCoroutine(
                DamageRoutine()
            );

        StartCoroutine(
            LifetimeRoutine()
        );
    }

    private void FixedUpdate()
    {
        if (!initialized || ending)
            return;

        PullEnemies();
    }

    // =====================================================
    // VISUAL
    // =====================================================

    private void CreateVisualLayers()
    {
        if (ringVisualPrefab == null)
        {
            Debug.LogError(
                "FireTornado chưa được gán Ring Visual Prefab."
            );

            return;
        }

        int count =
            Mathf.Max(
                1,
                layerCount
            );

        for (int i = 0; i < count; i++)
        {
            GameObject layer =
                Instantiate(
                    ringVisualPrefab,
                    transform
                );

            layer.name =
                $"FireRing_Layer_{i + 1}";

            /*
             * Layer 0 là tầng dưới cùng:
             * nhỏ nhất và nằm thấp nhất.
             */
            float scale =
                bottomScale +
                i * scalePerLayer;

            layer.transform.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f
                );

            layer.transform.localPosition =
                new Vector3(
                    0f,
                    i * layerHeight,
                    0f
                );

            layer.transform.localRotation =
                Quaternion.identity;

            SpriteRenderer renderer =
                layer.GetComponentInChildren<
                    SpriteRenderer
                >();

            if (renderer != null)
            {
                renderer.sortingOrder += i;
            }

            Animator layerAnimator =
                layer.GetComponentInChildren<
                    Animator
                >();

            if (layerAnimator != null)
            {
                /*
                 * Các vòng không chạy cùng đúng một frame,
                 * tạo cảm giác lốc xoáy tự nhiên hơn.
                 */
                float normalizedTime =
                    Mathf.Repeat(
                        i * animationDelayPerLayer,
                        1f
                    );

                layerAnimator.Play(
                    0,
                    0,
                    normalizedTime
                );
            }
        }
    }

    // =====================================================
    // PULL
    // =====================================================

    private void PullEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                effectRadius,
                enemyLayer
            );

        HashSet<EnermyHealth> handledEnemies =
            new HashSet<EnermyHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null ||
                !handledEnemies.Add(enemy))
            {
                continue;
            }

            Vector2 enemyPosition =
                enemy.transform.position;

            Vector2 center =
                transform.position;

            Vector2 toCenter =
                center - enemyPosition;

            float distance =
                toCenter.magnitude;

            if (distance <= pullStopDistance ||
                distance <= 0.001f)
            {
                continue;
            }

            Vector2 pullDirection =
                toCenter.normalized;

            EnermyMovement movement =
                enemy.GetComponent<
                    EnermyMovement
                >();

            if (movement != null)
            {
                movement.externalVelocity +=
                    pullDirection *
                    pullForce *
                    Time.fixedDeltaTime;

                movement.externalVelocity =
                    Vector2.ClampMagnitude(
                        movement.externalVelocity,
                        maxPullSpeed
                    );
            }
            else
            {
                Rigidbody2D enemyRb =
                    enemy.GetComponent<Rigidbody2D>();

                if (enemyRb != null)
                {
                    Vector2 nextPosition =
                        Vector2.MoveTowards(
                            enemyRb.position,
                            center,
                            maxPullSpeed *
                            Time.fixedDeltaTime
                        );

                    enemyRb.MovePosition(
                        nextPosition
                    );
                }
            }
        }
    }

    // =====================================================
    // DAMAGE
    // =====================================================

    private IEnumerator DamageRoutine()
    {
        float interval =
            Mathf.Max(
                0.05f,
                damageInterval
            );

        while (!ending)
        {
            DamageEnemies();

            yield return new WaitForSeconds(
                interval
            );
        }
    }

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                effectRadius,
                enemyLayer
            );

        HashSet<EnermyHealth> damagedEnemies =
            new HashSet<EnermyHealth>();

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null ||
                !damagedEnemies.Add(enemy))
            {
                continue;
            }

            Vector2 knockbackDirection =
                (
                    enemy.transform.position -
                    transform.position
                ).normalized;

            enemy.TakeDamage(
                damagePerTick,
                knockbackDirection
            );

            ApplyBurn(enemy);
        }
    }

    private void ApplyBurn(
        EnermyHealth enemy)
    {
        if (enemy == null)
            return;

        EnemyBurnEffect burn =
            enemy.GetComponent<
                EnemyBurnEffect
            >();

        if (burn == null)
        {
            burn =
                enemy.gameObject
                    .AddComponent<
                        EnemyBurnEffect
                    >();
        }

        burn.ApplyBurn(
            burnDamage,
            burnInterval,
            burnDuration,
            burnEffectPrefab
        );
    }

    // =====================================================
    // END
    // =====================================================

    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                duration
            )
        );

        EndTornado();
    }

    private void EndTornado()
    {
        if (ending)
            return;

        ending = true;

        if (damageCoroutine != null)
        {
            StopCoroutine(
                damageCoroutine
            );

            damageCoroutine = null;
        }

        StopLoopSound();

        if (AudioManager.Instance != null &&
            tornadoEndSound != null)
        {
            AudioManager.Instance.PlaySFX(
                tornadoEndSound
            );
        }

        SpawnFireGround();

        Destroy(gameObject);
    }

    private void SpawnFireGround()
    {
        if (fireGroundPrefab == null)
            return;

        GameObject fireGround =
            Instantiate(
                fireGroundPrefab,
                transform.position,
                Quaternion.identity
            );

        FireGroundArea groundArea =
            fireGround.GetComponent<
                FireGroundArea
            >();

        if (groundArea != null)
        {
            groundArea.Initialize(
                owner
            );
        }
        else
        {
            Debug.LogError(
                "Fire Ground Prefab thiếu FireGroundArea."
            );
        }
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void StartLoopSound()
    {
        if (tornadoLoopSound == null)
            return;

        loopAudioSource =
            gameObject.AddComponent<
                AudioSource
            >();

        loopAudioSource.clip =
            tornadoLoopSound;

        loopAudioSource.loop = true;
        loopAudioSource.playOnAwake = false;
        loopAudioSource.spatialBlend = 0f;

        loopAudioSource.Play();
    }

    private void StopLoopSound()
    {
        if (loopAudioSource == null)
            return;

        loopAudioSource.Stop();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            effectRadius
        );
    }
}