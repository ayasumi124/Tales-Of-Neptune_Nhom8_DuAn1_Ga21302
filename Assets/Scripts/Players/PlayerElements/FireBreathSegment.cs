using System.Collections.Generic;
using UnityEngine;

public class FireBreathSegment : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 0.7f;

    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;

    [Header("Collision")]
    [SerializeField] private float damageRadius = 0.45f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private Vector2 direction;
    private GameObject owner;

    private bool damageApplied;

    private readonly HashSet<EnermyHealth>
        damagedEnemies =
            new HashSet<EnermyHealth>();

    public void Initialize(
        Vector2 castDirection,
        GameObject skillOwner,
        int segmentIndex)
    {
        direction =
            castDirection.sqrMagnitude > 0.001f
                ? castDirection.normalized
                : Vector2.down;

        owner = skillOwner;

        ConfigureRotation();

        float scaleMultiplier =
            0.75f +
            segmentIndex * 0.08f;

        transform.localScale =
            Vector3.one * scaleMultiplier;

        ApplyDamage();

        Destroy(
            gameObject,
            Mathf.Max(0.1f, lifeTime)
        );
    }

    private void ConfigureRotation()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;

        // Hướng phải
        if (direction.x > 0.5f)
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, 0f);

            spriteRenderer.flipX = false;
        }
        // Hướng trái
        else if (direction.x < -0.5f)
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, 0f);

            spriteRenderer.flipX = true;
        }
        // Hướng lên
        else if (direction.y > 0.5f)
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, 90f);

            spriteRenderer.flipX = false;
        }
        // Hướng xuống
        else
        {
            transform.rotation =
                Quaternion.Euler(0f, 0f, -90f);

            spriteRenderer.flipX = false;
        }
    }

    private void ApplyDamage()
    {
        if (damageApplied)
            return;

        damageApplied = true;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                damageRadius,
                enemyLayer
            );

        foreach (Collider2D hit in hits)
        {
            EnermyHealth enemy =
                hit.GetComponentInParent<EnermyHealth>();

            if (enemy == null)
                continue;

            if (damagedEnemies.Contains(enemy))
                continue;

            damagedEnemies.Add(enemy);

            enemy.TakeDamage(
                damage,
                direction
            );

            EnemyBurnEffect burn =
                enemy.GetComponent<EnemyBurnEffect>();

            if (burn == null)
            {
                burn =
                    enemy.gameObject
                        .AddComponent<EnemyBurnEffect>();
            }

            burn.ApplyBurn(
                burnDamage,
                burnInterval,
                burnDuration,
                burnEffectPrefab
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }
}