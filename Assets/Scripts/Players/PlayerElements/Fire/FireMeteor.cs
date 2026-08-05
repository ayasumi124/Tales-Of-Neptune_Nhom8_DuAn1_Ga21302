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
    [SerializeField] private float meteorScale = 1.8f;

    [SerializeField] private float spriteBaseAngle;

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
    [SerializeField] private GameObject fireGroundPrefab;
    [SerializeField] private float impactEffectLifeTime = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip fallingSound;
    [SerializeField] private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField] private float fallingSoundVolume = 0.25f;

    [Range(0f, 1f)]
    [SerializeField] private float impactSoundVolume = 0.8f;

    private static readonly HashSet<FireMeteor>
        activeMeteors =
            new HashSet<FireMeteor>();

    private GameObject owner;

    private Vector3 targetPosition;

    private bool initialized;
    private bool impacted;
    private bool isBeingDestroyed;

    private float lifeTimer;

    private AudioSource fallingAudioSource;

    private void OnEnable()
    {
        activeMeteors.Add(this);
    }

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
        targetPosition = impactPosition;

        transform.position =
            spawnPosition;

        float finalScale =
            Mathf.Max(
                0.1f,
                meteorScale *
                Mathf.Max(
                    0.1f,
                    scaleMultiplier
                )
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
        if (!initialized ||
            impacted ||
            isBeingDestroyed)
        {
            return;
        }

        lifeTimer -=
            Time.deltaTime;

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

        if (direction.sqrMagnitude <=
            0.001f)
        {
            return;
        }

        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle +
                spriteBaseAngle
            );
    }

    private void Impact()
    {
        if (impacted ||
            isBeingDestroyed)
        {
            return;
        }

        impacted = true;

        transform.position =
            targetPosition;

        StopFallingSound();

        if (AudioManager.Instance != null &&
            impactSound != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    impactSound,
                    impactSoundVolume
                );
        }

        DamageEnemies();
        SpawnImpactEffect();
        SpawnFireGround();

        DestroyMeteor();
    }

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                targetPosition,
                impactRadius,
                enemyLayer
            );

        HashSet<EnermyHealth>
            damagedEnemies =
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
    knockbackDirection,
    1f
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
        if (impactEffectPrefab == null ||
            isBeingDestroyed)
        {
            return;
        }

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
        if (fireGroundPrefab == null ||
            isBeingDestroyed)
        {
            return;
        }

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

        /*
         * Nếu prefab đã có AudioSource thì dùng lại,
         * tránh AddComponent nhiều lần.
         */
        fallingAudioSource =
            GetComponent<AudioSource>();

        if (fallingAudioSource == null)
        {
            fallingAudioSource =
                gameObject.AddComponent<
                    AudioSource
                >();
        }

        fallingAudioSource.Stop();

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
        fallingAudioSource.clip = null;
        fallingAudioSource.loop = false;
    }

    private void DestroyMeteor()
    {
        if (isBeingDestroyed)
            return;

        isBeingDestroyed = true;

        StopFallingSound();
        activeMeteors.Remove(this);

        Destroy(gameObject);
    }

    public static void StopAllMeteors()
    {
        if (activeMeteors.Count == 0)
            return;

        FireMeteor[] meteors =
            new FireMeteor[
                activeMeteors.Count
            ];

        activeMeteors.CopyTo(
            meteors
        );

        /*
         * Xóa list trước để OnDisable/OnDestroy
         * không thay đổi collection đang duyệt.
         */
        activeMeteors.Clear();

        foreach (FireMeteor meteor
                 in meteors)
        {
            if (meteor == null)
                continue;

            meteor.isBeingDestroyed =
                true;

            meteor.StopFallingSound();

            Destroy(
                meteor.gameObject
            );
        }
    }

    private void OnDisable()
    {
        StopFallingSound();
        activeMeteors.Remove(this);
    }

    private void OnDestroy()
    {
        StopFallingSound();
        activeMeteors.Remove(this);
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