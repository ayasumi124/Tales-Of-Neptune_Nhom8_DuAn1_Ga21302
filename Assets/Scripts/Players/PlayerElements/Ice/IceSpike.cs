using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceSpike : MonoBehaviour
{
    [Header("Damage")]
    [Min(0)]
    [SerializeField]
    private int damage = 25;

    [Header("Slow")]
    [Tooltip(
        "0.6 = Enemy còn 60% tốc độ."
    )]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float slowMultiplier = 0.6f;

    [Min(0.1f)]
    [SerializeField]
    private float slowDuration = 2f;
    [Header("Effects")]
    [SerializeField]
    private GameObject slowEffectPrefab;

    [Header("Slow Audio")]
    [SerializeField]
    private AudioClip slowSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slowSoundVolume = 0.5f;
    [Header("Timing")]
    [Tooltip(
        "Thời gian chờ để hitbox bật, " +
        "khớp lúc gai băng mọc lên."
    )]
    [Min(0f)]
    [SerializeField]
    private float hitboxDelay = 0.1f;

    [Tooltip(
        "Hitbox tồn tại bao lâu."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float hitboxDuration = 0.25f;

    [Tooltip(
        "Tổng thời gian tồn tại của prefab."
    )]
    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 0.8f;

    [Header("References")]
    [SerializeField]
    private Collider2D hitbox;

    private GameObject owner;

    private Vector2 direction =
        Vector2.down;

    private SpriteRenderer spriteRenderer;

    private readonly HashSet<EnermyHealth>
        hitEnemies =
            new HashSet<EnermyHealth>();

    private void Awake()
    {
        if (hitbox == null)
        {
            hitbox =
                GetComponent<Collider2D>();
        }

        if (hitbox == null)
        {
            Debug.LogError(
                $"{name}: IceSpike thiếu Collider2D."
            );

            return;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        hitbox.isTrigger = true;
        hitbox.enabled = false;
    }

    public void Initialize(
        Vector2 castDirection,
        GameObject skillOwner)
    {
        direction =
            castDirection.sqrMagnitude >
            0.001f
                ? castDirection.normalized
                : Vector2.down;

        owner =
            skillOwner;

        RotateToDirection();

        StartCoroutine(
            HitboxRoutine()
        );

        Destroy(
            gameObject,
            Mathf.Max(
                0.1f,
                lifeTime
            )
        );
    }

    private IEnumerator HitboxRoutine()
    {
        if (hitboxDelay > 0f)
        {
            yield return new WaitForSeconds(
                hitboxDelay
            );
        }

        if (hitbox != null)
        {
            hitbox.enabled = true;
        }

        yield return new WaitForSeconds(
            Mathf.Max(
                0.01f,
                hitboxDuration
            )
        );

        if (hitbox != null)
        {
            hitbox.enabled = false;
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (other == null)
            return;

        /*
         * Không đánh Player tạo skill.
         */
        if (owner != null &&
            (
                other.gameObject == owner ||
                other.transform.IsChildOf(
                    owner.transform
                )
            ))
        {
            return;
        }

        EnermyHealth enemy =
            other.GetComponentInParent<
                EnermyHealth
            >();

        if (enemy == null)
            return;

        /*
         * Một Ice Spike chỉ damage
         * mỗi Enemy đúng 1 lần.
         */
        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        Vector2 knockbackDirection =
            (
                enemy.transform.position -
                transform.position
            ).normalized;

        if (knockbackDirection.sqrMagnitude <
            0.001f)
        {
            knockbackDirection =
                direction;
        }

        enemy.TakeDamage(
            damage,
            knockbackDirection
        );

        ApplySlow(
            enemy
        );
    }

    private void ApplySlow(
    EnermyHealth enemy)
    {
        if (enemy == null)
            return;

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
    slowDuration,
    slowEffectPrefab,
    slowSound,
    slowSoundVolume
);
    }

    private void RotateToDirection()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        // =========================================
        // RIGHT
        // =========================================

        if (direction == Vector2.right)
        {
            transform.rotation =
                Quaternion.identity;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = false;
            }

            return;
        }

        // =========================================
        // LEFT
        // =========================================

        if (direction == Vector2.left)
        {
            /*
             * Không rotate 180 độ.
             * Chỉ flip ngang sprite.
             */
            transform.rotation =
                Quaternion.identity;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = true;
                spriteRenderer.flipY = false;
            }

            return;
        }

        // =========================================
        // UP
        // =========================================

        if (direction == Vector2.up)
        {
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = false;
            }

            return;
        }

        // =========================================
        // DOWN
        // =========================================

        if (direction == Vector2.down)
        {
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    -90f
                );

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = false;
                spriteRenderer.flipY = false;
            }
        }
    }

    private void OnValidate()
    {
        damage =
            Mathf.Max(
                0,
                damage
            );

        slowMultiplier =
            Mathf.Clamp(
                slowMultiplier,
                0.05f,
                1f
            );

        slowDuration =
            Mathf.Max(
                0.1f,
                slowDuration
            );

        hitboxDelay =
            Mathf.Max(
                0f,
                hitboxDelay
            );

        hitboxDuration =
            Mathf.Max(
                0.01f,
                hitboxDuration
            );

        lifeTime =
            Mathf.Max(
                hitboxDelay +
                hitboxDuration,
                lifeTime
            );
    }
}