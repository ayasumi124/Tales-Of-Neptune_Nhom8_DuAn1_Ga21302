using System.Collections.Generic;
using UnityEngine;

public class IceGroundDamage : MonoBehaviour
{
    [Header("Expansion")]
    [SerializeField]
    private float startRadius = 0.2f;

    [SerializeField]
    private float maxRadius = 2.2f;

    [SerializeField]
    private float expandDuration = 0.35f;

    [Header("Damage")]
    [SerializeField]
    private int damage = 35;

    [SerializeField]
    private float knockbackStrength = 4f;

    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Freeze")]
    [SerializeField]
    private float freezeDuration = 2.5f;

    [SerializeField]
    private GameObject freezeVFXPrefab;

    private float timer;

    private float currentRadius;

    private bool finished;

    private readonly HashSet<EnermyHealth>
        hitEnemies =
            new HashSet<EnermyHealth>();


    private void OnEnable()
    {
        timer = 0f;

        currentRadius =
            startRadius;

        finished = false;

        hitEnemies.Clear();
    }


    private void Update()
    {
        if (finished)
            return;

        timer += Time.deltaTime;

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

        CheckEnemies();

        if (normalized >= 1f)
        {
            finished = true;
        }
    }


    private void CheckEnemies()
    {
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

            // Enemy này đã bị IceGround đánh rồi.
            if (!hitEnemies.Add(enemy))
                continue;

            HitEnemy(enemy);
        }
    }


    private void HitEnemy(
        EnermyHealth enemy)
    {
        Vector2 direction =
            (
                enemy.transform.position -
                transform.position
            ).normalized;

        if (direction.sqrMagnitude <
            0.001f)
        {
            direction =
                Vector2.down;
        }

        // Damage
        enemy.TakeDamage(
            damage,
            direction,
            knockbackStrength,
            true
        );

        // Freeze
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

        Debug.Log(
            $"IceGround hit + freeze: " +
            $"{enemy.name}"
        );
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        float drawRadius =
            Application.isPlaying
                ? currentRadius
                : maxRadius;

        Gizmos.DrawWireSphere(
            transform.position,
            drawRadius
        );
    }


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

        damage =
            Mathf.Max(
                1,
                damage
            );

        freezeDuration =
            Mathf.Max(
                0.1f,
                freezeDuration
            );
    }
}