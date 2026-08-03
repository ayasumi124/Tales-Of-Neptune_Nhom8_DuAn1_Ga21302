using System.Collections;
using UnityEngine;

public class FireBreathArea : MonoBehaviour
{
    [Header("Position")]
    [Tooltip("Khoảng cách hiệu ứng nằm trước mặt Player.")]
    [SerializeField] private float forwardOffset = 1.1f;

    [Tooltip("Chỉnh lệch riêng theo trục X/Y nếu sprite có pivot lệch.")]
    [SerializeField] private Vector2 visualOffset = Vector2.zero;

    [Header("Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float tickInterval = 0.4f;

    [Header("Damage")]
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnDuration = 3f;

    [Header("Collision")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float damageRadius = 0.8f;

    [Header("Effects")]
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Visual")]
    [SerializeField] private Transform visual;
    [SerializeField]
    private Vector2 visualLocalOffset =
        new Vector2(-0.8f, 0f);

    [SerializeField] private SpriteRenderer visualRenderer;

    private Transform player;
    private Vector2 direction;

    public void Initialize(
        Transform playerTransform,
        Vector2 castDirection,
        GameObject skillOwner)
    {
        player = playerTransform;

        direction =
            castDirection.sqrMagnitude > 0.001f
                ? castDirection.normalized
                : Vector2.down;

        RotateEffect();
        UpdatePosition();

        StartCoroutine(DamageRoutine());
        UpdateVisualOffset();
    }

    private void LateUpdate()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (player == null)
            return;

        transform.position =
            player.position +
            (Vector3)(direction * forwardOffset);
    }

    private Vector2 RotateOffsetForDirection(
        Vector2 offset,
        Vector2 castDirection)
    {
        float angle =
            Mathf.Atan2(
                castDirection.y,
                castDirection.x
            ) * Mathf.Rad2Deg;

        return Quaternion.Euler(
            0f,
            0f,
            angle
        ) * offset;
    }
    private void UpdateVisualOffset()
    {
        if (visual == null)
            return;

        visual.localPosition = visualLocalOffset;
    }

    private IEnumerator DamageRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            DamageEnemies();

            yield return new WaitForSeconds(
                Mathf.Max(0.05f, tickInterval)
            );

            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                damageRadius,
                enemyLayer
            );

        foreach (Collider2D hit in hits)
        {
            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null)
                continue;

            enemy.TakeDamage(
                damagePerTick,
                direction
            );

            EnemyBurnEffect burn =
                enemy.GetComponent<
                    EnemyBurnEffect
                >();

            if (burn == null)
            {
                burn = enemy.gameObject
                    .AddComponent<
                        EnemyBurnEffect
                    >();
            }

            burn.ApplyBurn(
                burnDamage,
                1f,
                burnDuration,
                burnEffectPrefab
            );
        }
    }

    private void RotateEffect()
    {
        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        angle += 180f;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        if (visualRenderer != null)
        {
            visualRenderer.flipY =
                direction.x < 0f;
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