using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FireSparkProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifeTime = 3f;

    [Header("Damage")]
    [SerializeField] private int impactDamage = 12;

    [Header("Burn")]
    [SerializeField] private int burnDamage = 5;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 2f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Direction")]
    [Tooltip("Góc gốc của sprite: 0 nếu sprite hướng phải, 180 nếu hướng trái.")]
    [SerializeField] private float spriteBaseAngle;

    private Rigidbody2D rb;
    private GameObject owner;
    private Vector2 direction;

    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        Collider2D projectileCollider =
            GetComponent<Collider2D>();

        if (!projectileCollider.isTrigger)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Collider2D của FireSpark nên bật Is Trigger."
            );
        }
    }

    public void Initialize(
        Vector2 moveDirection,
        GameObject projectileOwner)
    {
        direction =
            moveDirection.sqrMagnitude > 0.001f
                ? moveDirection.normalized
                : Vector2.right;

        owner = projectileOwner;
        initialized = true;

        RotateToDirection();

        Destroy(
            gameObject,
            Mathf.Max(0.1f, lifeTime)
        );
    }

    private void FixedUpdate()
    {
        if (!initialized ||
            hasHit ||
            rb == null)
        {
            return;
        }

        rb.linearVelocity =
            direction * speed;
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (hasHit || other == null)
            return;

        if (IsOwnerCollider(other))
            return;

        EnermyHealth enemy =
            other.GetComponentInParent<EnermyHealth>();

        if (enemy != null)
        {
            HitEnemy(enemy);
            return;
        }

        if (IsInLayerMask(
                other.gameObject,
                obstacleLayer))
        {
            DestroyProjectile();
        }
    }

    private bool IsOwnerCollider(
        Collider2D other)
    {
        if (owner == null)
            return false;

        return other.gameObject == owner ||
               other.transform.IsChildOf(
                   owner.transform
               );
    }

    private void HitEnemy(
        EnermyHealth enemy)
    {
        if (hasHit || enemy == null)
            return;

        hasHit = true;
        StopProjectile();

        enemy.TakeDamage(
            impactDamage,
            direction
        );

        EnemyBurnEffect burn =
            enemy.GetComponent<EnemyBurnEffect>();

        if (burn == null)
        {
            burn =
                enemy.gameObject.AddComponent<
                    EnemyBurnEffect
                >();
        }

        burn.ApplyBurn(
            burnDamage,
            burnInterval,
            burnDuration,
            burnEffectPrefab
        );

        SpawnHitEffect();
        Destroy(gameObject);
    }

    private void DestroyProjectile()
    {
        if (hasHit)
            return;

        hasHit = true;
        StopProjectile();

        SpawnHitEffect();
        Destroy(gameObject);
    }

    private void StopProjectile()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    private void RotateToDirection()
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
                angle + spriteBaseAngle
            );
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity
            );

        Destroy(effect, 2f);
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
}