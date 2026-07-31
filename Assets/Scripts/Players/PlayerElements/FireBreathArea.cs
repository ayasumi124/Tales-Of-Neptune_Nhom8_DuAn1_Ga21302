using System.Collections;
using UnityEngine;

public class FireBreathArea : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float duration = 2f;
    [SerializeField] private float tickInterval = 0.4f;

    [Header("Damage")]
    [SerializeField] private int damagePerTick = 10;
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnDuration = 3f;

    [Header("Collision")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject burnEffectPrefab;

    private Transform player;
    private GameObject owner;

    private Vector2 direction;

    public void Initialize(
        Transform playerTransform,
        Vector2 castDirection,
        GameObject skillOwner)
    {
        player = playerTransform;
        owner = skillOwner;

        direction =
            castDirection.sqrMagnitude > 0.001f
                ? castDirection.normalized
                : Vector2.down;

        RotateEffect();

        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            FollowPlayer();

            DamageEnemies();

            yield return new WaitForSeconds(
                tickInterval
            );

            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    private void LateUpdate()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (player == null)
            return;

        transform.position =
            player.position +
            (Vector3)(direction * 0.8f);
    }

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                0.8f,
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

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );
    }
}