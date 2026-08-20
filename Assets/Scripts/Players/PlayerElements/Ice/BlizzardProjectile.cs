using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BlizzardProjectile :
    MonoBehaviour
{
    // =====================================================
    // MOVEMENT
    // =====================================================

    [Header("Movement")]
    [Min(0f)]
    [SerializeField]
    private float speed = 6f;

    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 2.5f;


    // =====================================================
    // DAMAGE
    // =====================================================

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 25;

    [SerializeField]
    private float knockbackStrength = 3f;


    // =====================================================
    // AOE
    // =====================================================

    [Header("AOE")]
    [Min(0.05f)]
    [SerializeField]
    private float radius = 0.9f;

    [SerializeField]
    private LayerMask enemyLayer;


    // =====================================================
    // SLOW
    // =====================================================

    [Header("Slow")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float slowMultiplier = 0.3f;

    [Min(0.1f)]
    [SerializeField]
    private float slowDuration = 3f;

    [SerializeField]
    private GameObject slowEffectPrefab;

    [Header("Slow Audio")]
    [SerializeField]
    private AudioClip slowSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slowSoundVolume = 0.4f;


    // =====================================================
    // FREEZE
    // =====================================================

    [Header("Freeze")]
    [SerializeField]
    private bool freezeOnHit = true;

    [Min(0.1f)]
    [SerializeField]
    private float freezeDuration = 1.5f;

    [SerializeField]
    private GameObject freezeVFXPrefab;


    // =====================================================
    // COLLISION
    // =====================================================

    [Header("Collision")]
    [SerializeField]
    private LayerMask obstacleLayer;


    // =====================================================
    // VISUAL
    // =====================================================

    [Header("Visual")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Transform visualRoot;

    [Tooltip(
        "Bật nếu sprite gốc của Blizzard " +
        "đang nhìn sang phải."
    )]
    [SerializeField]
    private bool spriteFacesRight = true;


    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Release Audio")]
    [SerializeField]
    private AudioClip releaseSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float releaseVolume = 1f;


    // =====================================================
    // IMPACT
    // =====================================================

    [Header("Impact")]
    [SerializeField]
    private GameObject impactEffectPrefab;

    [SerializeField]
    private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float impactVolume = 0.7f;


    // =====================================================
    // RUNTIME
    // =====================================================

    private Rigidbody2D rb;

    private Vector2 direction =
        Vector2.right;

    private GameObject owner;

    private bool initialized;
    private bool destroyed;

    private readonly HashSet<EnermyHealth>
        hitEnemies =
            new HashSet<EnermyHealth>();


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<
                    SpriteRenderer
                >();
        }

        if (visualRoot == null &&
            spriteRenderer != null)
        {
            visualRoot =
                spriteRenderer.transform;
        }
    }


    private void Update()
    {
        if (!initialized ||
            destroyed)
        {
            return;
        }

        DamageEnemiesAround();
    }


    private void FixedUpdate()
    {
        if (!initialized ||
            destroyed ||
            rb == null)
        {
            return;
        }

        rb.linearVelocity =
            direction *
            speed;
    }


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        Vector2 moveDirection,
        GameObject projectileOwner)
    {
        direction =
            GetCardinalDirection(
                moveDirection
            );

        owner =
            projectileOwner;

        UpdateVisualDirection();

        PlayReleaseSound();

        initialized = true;

        Destroy(
            gameObject,
            Mathf.Max(
                0.1f,
                lifeTime
            )
        );
    }


    // =====================================================
    // DIRECTION
    // =====================================================

    private Vector2 GetCardinalDirection(
        Vector2 input)
    {
        if (input.sqrMagnitude <
            0.001f)
        {
            return Vector2.down;
        }

        if (Mathf.Abs(input.x) >
            Mathf.Abs(input.y))
        {
            return input.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }

        return input.y >= 0f
            ? Vector2.up
            : Vector2.down;
    }


    // =====================================================
    // VISUAL DIRECTION
    // =====================================================

    private void UpdateVisualDirection()
    {
        if (spriteRenderer == null)
            return;

        /*
         * Sprite gốc hướng PHẢI.
         *
         * Right = bình thường
         * Left  = Flip X
         *
         * Up / Down xoay Visual,
         * KHÔNG xoay root để hitbox
         * và Rigidbody không bị ảnh hưởng.
         */

        if (direction == Vector2.right)
        {
            if (visualRoot != null)
            {
                visualRoot.localRotation =
                    Quaternion.identity;
            }

            spriteRenderer.flipX =
                !spriteFacesRight;

            spriteRenderer.flipY =
                false;

            return;
        }

        if (direction == Vector2.left)
        {
            if (visualRoot != null)
            {
                visualRoot.localRotation =
                    Quaternion.identity;
            }

            spriteRenderer.flipX =
                spriteFacesRight;

            spriteRenderer.flipY =
                false;

            return;
        }

        if (direction == Vector2.up)
        {
            if (visualRoot != null)
            {
                visualRoot.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        90f
                    );
            }

            spriteRenderer.flipX =
                !spriteFacesRight;

            spriteRenderer.flipY =
                false;

            return;
        }

        if (direction == Vector2.down)
        {
            if (visualRoot != null)
            {
                visualRoot.localRotation =
                    Quaternion.Euler(
                        0f,
                        0f,
                        -90f
                    );
            }

            spriteRenderer.flipX =
                !spriteFacesRight;

            spriteRenderer.flipY =
                false;
        }
    }


    // =====================================================
    // DAMAGE
    // =====================================================

    private void DamageEnemiesAround()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                radius,
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

            /*
             * Một projectile chỉ damage
             * cùng Enemy đúng 1 lần.
             */
            if (!hitEnemies.Add(enemy))
                continue;

            HitEnemy(enemy);
        }
    }


    private void HitEnemy(
        EnermyHealth enemy)
    {
        if (enemy == null)
            return;

        enemy.TakeDamage(
            damage,
            direction,
            knockbackStrength,
            true
        );

        if (freezeOnHit)
        {
            ApplyFreeze(enemy);
        }
        else
        {
            ApplySlow(enemy);
        }
    }


    // =====================================================
    // SLOW
    // =====================================================

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
            slowDuration,
            slowEffectPrefab,
            slowSound,
            slowSoundVolume
        );
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
    // COLLISION
    // =====================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (destroyed ||
            other == null)
        {
            return;
        }

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

        if (IsInLayerMask(
                other.gameObject,
                obstacleLayer))
        {
            DestroyProjectile();
        }
    }


    private bool IsInLayerMask(
        GameObject target,
        LayerMask mask)
    {
        return (
            mask.value &
            (1 << target.layer)
        ) != 0;
    }


    // =====================================================
    // DESTROY
    // =====================================================

    private void DestroyProjectile()
    {
        if (destroyed)
            return;

        destroyed = true;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        SpawnImpactEffect();

        PlayImpactSound();

        Destroy(
            gameObject
        );
    }


    // =====================================================
    // EFFECT
    // =====================================================

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                impactEffectPrefab,
                transform.position,
                Quaternion.identity
            );

        Destroy(
            effect,
            1.5f
        );
    }


    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayReleaseSound()
    {
        if (releaseSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    releaseSound,
                    releaseVolume
                );
        }
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
        }
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
        speed =
            Mathf.Max(
                0f,
                speed
            );

        lifeTime =
            Mathf.Max(
                0.1f,
                lifeTime
            );

        damage =
            Mathf.Max(
                1,
                damage
            );

        radius =
            Mathf.Max(
                0.05f,
                radius
            );

        freezeDuration =
            Mathf.Max(
                0.1f,
                freezeDuration
            );
    }
}