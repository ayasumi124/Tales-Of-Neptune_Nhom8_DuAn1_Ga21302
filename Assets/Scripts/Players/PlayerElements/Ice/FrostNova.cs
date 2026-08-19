using System.Collections.Generic;
using UnityEngine;

public class FrostNova : MonoBehaviour
{
    // =====================================================
    // DAMAGE
    // =====================================================

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 35;

    [Min(0f)]
    [SerializeField]
    private float knockbackStrength = 4f;


    // =====================================================
    // AOE
    // =====================================================

    [Header("AOE")]
    [Min(0.1f)]
    [SerializeField]
    private float radius = 2.2f;

    [SerializeField]
    private LayerMask enemyLayer;


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
    // TIMING
    // =====================================================

    [Header("Timing")]
    [Tooltip(
        "Thời gian từ lúc spawn tới lúc gây damage + freeze."
    )]
    [Min(0f)]
    [SerializeField]
    private float impactDelay = 0.18f;

    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 1.2f;


    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField]
    private AudioClip castSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float castVolume = 1f;

    [SerializeField]
    private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float impactVolume = 1f;


    // =====================================================
    // STATE
    // =====================================================

    private GameObject owner;

    private bool initialized;
    private bool impacted;

    private float impactTimer;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        GameObject novaOwner)
    {
        owner = novaOwner;

        initialized = true;

        impactTimer =
            Mathf.Max(
                0f,
                impactDelay
            );

        PlayCastSound();

        Destroy(
            gameObject,
            Mathf.Max(
                0.1f,
                lifeTime
            )
        );
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!initialized ||
            impacted)
        {
            return;
        }

        impactTimer -=
            Time.deltaTime;

        if (impactTimer <= 0f)
        {
            Impact();
        }
    }


    // =====================================================
    // IMPACT
    // =====================================================

    private void Impact()
    {
        if (impacted)
            return;

        impacted = true;

        PlayImpactSound();

        DamageAndFreezeEnemies();
    }


    // =====================================================
    // DAMAGE + FREEZE
    // =====================================================

    private void DamageAndFreezeEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                radius,
                enemyLayer
            );

        HashSet<EnermyHealth>
            processed =
                new HashSet<EnermyHealth>();

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

            if (!processed.Add(enemy))
                continue;

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

            enemy.TakeDamage(
                damage,
                knockDirection,
                knockbackStrength,
                true
            );

            ApplyFreeze(
                enemy
            );
        }
    }


    // =====================================================
    // FREEZE
    // =====================================================

    private void ApplyFreeze(
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


    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayCastSound()
    {
        if (castSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    castSound,
                    castVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            castSound,
            transform.position,
            castVolume
        );
    }


    private void PlayImpactSound()
    {
        if (impactSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    impactSound,
                    impactVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            impactSound,
            transform.position,
            impactVolume
        );
    }


    // =====================================================
    // GIZMO
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        damage =
            Mathf.Max(
                1,
                damage
            );

        knockbackStrength =
            Mathf.Max(
                0f,
                knockbackStrength
            );

        radius =
            Mathf.Max(
                0.1f,
                radius
            );

        freezeDuration =
            Mathf.Max(
                0.1f,
                freezeDuration
            );

        impactDelay =
            Mathf.Max(
                0f,
                impactDelay
            );

        lifeTime =
            Mathf.Max(
                0.1f,
                lifeTime
            );
    }
}