using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireGroundArea : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float duration = 4f;

    [Header("Damage")]
    [Tooltip("Damage ngay khi mặt lửa xuất hiện.")]
    [SerializeField] private int initialDamage = 20;

    [Tooltip("Damage mỗi nhịp sau đó.")]
    [SerializeField] private int damagePerTick = 8;

    [SerializeField] private float damageInterval = 0.5f;

    [Header("Area")]
    [SerializeField] private float damageRadius = 2.2f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Burn")]
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Tooltip(
        "Tên trigger mở đầu của Fire Ground. " +
        "Để trống nếu animation tự chạy."
    )]
    [SerializeField] private string appearTrigger =
        "Appear";

    [Header("Audio")]
    [SerializeField] private AudioClip appearSound;

    private GameObject owner;
    private bool initialized;

    public void Initialize(
        GameObject skillOwner)
    {
        if (initialized)
            return;

        initialized = true;
        owner = skillOwner;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null &&
            !string.IsNullOrWhiteSpace(
                appearTrigger
            ))
        {
            animator.SetTrigger(
                appearTrigger
            );
        }

        if (AudioManager.Instance != null &&
            appearSound != null)
        {
            AudioManager.Instance.PlaySFX(
                appearSound
            );
        }

        DamageEnemies(
            initialDamage
        );

        StartCoroutine(
            FireGroundRoutine()
        );
    }

    private IEnumerator FireGroundRoutine()
    {
        float elapsed = 0f;

        float interval =
            Mathf.Max(
                0.05f,
                damageInterval
            );

        while (elapsed < duration)
        {
            yield return new WaitForSeconds(
                interval
            );

            elapsed += interval;

            DamageEnemies(
                damagePerTick
            );
        }

        Destroy(gameObject);
    }

    private void DamageEnemies(
        int damageAmount)
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                damageRadius,
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

            Vector2 damageDirection =
                (
                    enemy.transform.position -
                    transform.position
                ).normalized;

            enemy.TakeDamage(
                damageAmount,
                damageDirection
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }
}