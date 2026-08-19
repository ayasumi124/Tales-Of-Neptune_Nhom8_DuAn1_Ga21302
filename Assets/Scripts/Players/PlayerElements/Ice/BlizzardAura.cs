using System.Collections.Generic;
using UnityEngine;

public class BlizzardAura : MonoBehaviour
{
    [Header("Follow")]
    [SerializeField]
    private float followSpeed = 10f;

    [Header("Audio")]
    [SerializeField]
    private AudioClip blizzardLoopSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float blizzardLoopVolume = 0.7f;

    private AudioSource audioSource;

    [Header("Damage")]
    [SerializeField]
    private int tickDamage = 5;

    [SerializeField]
    private float damageInterval = 0.75f;

    [SerializeField]
    private float radius = 1.8f;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Slow")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float slowMultiplier = 0.45f;

    [SerializeField]
    private float slowRefreshDuration = 1f;

    [SerializeField]
    private GameObject slowEffectPrefab;

    [Header("Freeze Build-up")]
    [SerializeField]
    private float freezeBuildTime = 2.5f;

    [SerializeField]
    private float freezeDuration = 2f;

    [SerializeField]
    private GameObject freezeVFXPrefab;

    private Transform player;

    private readonly Dictionary<EnermyHealth, float>
        freezeTimers =
            new Dictionary<EnermyHealth, float>();

    private float damageTimer;

    public void Initialize(
    Transform playerTransform)
    {
        player =
            playerTransform;

        transform.position =
            player.position;

        PlayBlizzardLoop();
    }
    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
    }
    private void Update()
    {
        if (player == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position =
            Vector3.Lerp(
                transform.position,
                player.position,
                followSpeed *
                Time.deltaTime
            );

        damageTimer -=
            Time.deltaTime;

        if (damageTimer <= 0f)
        {
            damageTimer =
                damageInterval;

            TickBlizzard();
        }

        UpdateFreezeBuildUp();
    }

    private void PlayBlizzardLoop()
    {
        if (audioSource == null ||
            blizzardLoopSound == null)
        {
            return;
        }

        audioSource.clip =
            blizzardLoopSound;

        audioSource.volume =
            blizzardLoopVolume;

        audioSource.Play();
    }

    private void TickBlizzard()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                radius,
                enemyLayer
            );

        HashSet<EnermyHealth> processed =
            new HashSet<EnermyHealth>();

        foreach (Collider2D hit in hits)
        {
            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null ||
                processed.Contains(enemy))
            {
                continue;
            }

            processed.Add(enemy);

            Vector2 knockDirection =
                (
                    enemy.transform.position -
                    transform.position
                ).normalized;

            enemy.TakeDamage(
                tickDamage,
                knockDirection
            );

            ApplySlow(enemy);

            EnemyFreezeEffect freeze =
    enemy.GetComponent<
        EnemyFreezeEffect
    >();

            bool isFrozen =
                freeze != null &&
                freeze.IsFrozen;

            if (!isFrozen &&
                !freezeTimers.ContainsKey(enemy))
            {
                freezeTimers.Add(
                    enemy,
                    0f
                );
            }
        }
    }

    private void ApplySlow(
        EnermyHealth enemy)
    {
        EnemySlowEffect slow =
            enemy.GetComponent<
                EnemySlowEffect
            >();

        if (slow == null)
        {
            slow =
                enemy.gameObject
                    .AddComponent<
                        EnemySlowEffect
                    >();
        }

        slow.ApplySlow(
            slowMultiplier,
            slowRefreshDuration,
            slowEffectPrefab
        );
    }

    private void UpdateFreezeBuildUp()
    {
        List<EnermyHealth> keys =
            new List<EnermyHealth>(
                freezeTimers.Keys
            );

        foreach (EnermyHealth enemy in keys)
        {
            if (enemy == null)
            {
                freezeTimers.Remove(enemy);
                continue;
            }

            float distance =
                Vector2.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <= radius)
            {
                freezeTimers[enemy] +=
                    Time.deltaTime;

                if (freezeTimers[enemy] >=
                    freezeBuildTime)
                {
                    TryFreeze(enemy);

                    freezeTimers.Remove(
                        enemy
                    );
                }
            }
            else
            {
                freezeTimers.Remove(
                    enemy
                );
            }
        }
    }

    private void TryFreeze(
    EnermyHealth enemy)
    {
        if (enemy == null)
            return;

        EnemyFreezeEffect freeze =
            enemy.GetComponent<
                EnemyFreezeEffect
            >();

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
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}