using System.Collections.Generic;
using UnityEngine;

public class FireMeteor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float fallSpeed = 10f;

    [Tooltip("Khoảng cách xác định meteor đã chạm đất.")]
    [SerializeField] private float impactDistance = 0.08f;

    [Tooltip("Thời gian tối đa trước khi meteor tự hủy.")]
    [SerializeField] private float maximumLifeTime = 5f;

    [Header("Visual")]
    [Tooltip("Kích thước cơ bản của meteor.")]
    [SerializeField] private float meteorScale = 1.8f;

    [Tooltip(
        "Góc gốc của sprite. Chỉnh nếu đầu meteor quay sai hướng."
    )]
    [SerializeField] private float spriteBaseAngle = 0f;

    [Header("Impact Damage")]
    [SerializeField] private int impactDamage = 25;
    [SerializeField] private float impactRadius = 1.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Burn")]
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Impact Effects")]
    [SerializeField] private GameObject impactEffectPrefab;

    [Tooltip("Vùng lửa sinh ra sau khi meteor chạm đất.")]
    [SerializeField] private GameObject fireGroundPrefab;

    [Tooltip("Thời gian tồn tại dự phòng của hiệu ứng nổ.")]
    [SerializeField] private float impactEffectLifeTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip fallingSound;
    [SerializeField] private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField] private float fallingSoundVolume = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float impactSoundVolume = 0.8f;

    private GameObject owner;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private bool initialized;
    private bool impacted;

    private float lifeTimer;

    private AudioSource fallingAudioSource;

    public void Initialize(
        Vector3 spawnPosition,
        Vector3 impactPosition,
        GameObject skillOwner,
        float scaleMultiplier = 1f)
    {
        if (initialized)
            return;

        initialized = true;

        owner = skillOwner;
        startPosition = spawnPosition;
        targetPosition = impactPosition;

        transform.position = startPosition;

        float finalScale =
            Mathf.Max(
                0.1f,
                meteorScale *
                Mathf.Max(0.1f, scaleMultiplier)
            );

        transform.localScale =
            new Vector3(
                finalScale,
                finalScale,
                1f
            );

        RotateTowardTarget();
        StartFallingSound();

        lifeTimer =
            Mathf.Max(
                0.5f,
                maximumLifeTime
            );
    }

    private void Update()
    {
        if (!initialized || impacted)
            return;

        lifeTimer -= Time.deltaTime;

        if (lifeTimer <= 0f)
        {
            Impact();
            return;
        }

        float speed =
            Mathf.Max(
                0.01f,
                fallSpeed
            );

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

        float distance =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distance <=
            Mathf.Max(
                0.01f,
                impactDistance
            ))
        {
            Impact();
        }
    }

    private void RotateTowardTarget()
    {
        Vector2 direction =
            targetPosition -
            transform.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle + spriteBaseAngle
            );
    }

    private void Impact()
    {
        if (impacted)
            return;

        impacted = true;

        transform.position =
            targetPosition;

        StopFallingSound();

        if (AudioManager.Instance != null &&
            impactSound != null)
        {
            AudioManager.Instance.PlaySFX(
                impactSound,
                impactSoundVolume
            );
        }

        DamageEnemies();
        SpawnImpactEffect();
        SpawnFireGround();

        Destroy(gameObject);
    }

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                targetPosition,
                impactRadius,
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
                    targetPosition
                ).normalized;

            if (knockbackDirection.sqrMagnitude <=
                0.001f)
            {
                knockbackDirection =
                    Vector2.up;
            }

            enemy.TakeDamage(
                impactDamage,
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

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                impactEffectPrefab,
                targetPosition,
                Quaternion.identity
            );

        Destroy(
            effect,
            Mathf.Max(
                0.1f,
                impactEffectLifeTime
            )
        );
    }

    private void SpawnFireGround()
    {
        if (fireGroundPrefab == null)
            return;

        GameObject fireGround =
            Instantiate(
                fireGroundPrefab,
                targetPosition,
                Quaternion.identity
            );

        FireGroundArea ground =
            fireGround.GetComponent<
                FireGroundArea
            >();

        if (ground == null)
        {
            Debug.LogError(
                "Fire Ground Prefab thiếu FireGroundArea."
            );

            Destroy(fireGround);
            return;
        }

        ground.Initialize(owner);
    }

    private void StartFallingSound()
    {
        if (fallingSound == null)
            return;

        fallingAudioSource =
            gameObject.AddComponent<
                AudioSource
            >();

        fallingAudioSource.clip =
            fallingSound;

        fallingAudioSource.loop = true;
        fallingAudioSource.playOnAwake = false;
        fallingAudioSource.spatialBlend = 0f;
        fallingAudioSource.volume =
            fallingSoundVolume;

        fallingAudioSource.Play();
    }

    private void StopFallingSound()
    {
        if (fallingAudioSource == null)
            return;

        fallingAudioSource.Stop();
    }

    private void OnDisable()
    {
        StopFallingSound();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center =
            initialized
                ? targetPosition
                : transform.position;

        Gizmos.DrawWireSphere(
            center,
            impactRadius
        );
    }
}