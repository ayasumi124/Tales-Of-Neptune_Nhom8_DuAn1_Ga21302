using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float lifeTime = 4f;

    [Header("Damage")]
    [SerializeField] private int impactDamage = 20;

    [Header("Burn")]
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;

    [Header("Collision")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject burnEffectPrefab;

    private Rigidbody2D rb;
    private GameObject owner;
    private Vector2 direction;

    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        Collider2D projectileCollider =
            GetComponent<Collider2D>();

        if (projectileCollider == null)
        {
            Debug.LogError(
                $"{gameObject.name} chưa có Collider2D."
            );
        }
        else if (!projectileCollider.isTrigger)
        {
            Debug.LogWarning(
                $"{gameObject.name}: Collider2D cần bật Is Trigger."
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
                : Vector2.down;

        owner = projectileOwner;
        initialized = true;

        RotateProjectile();

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

        // Không va chạm với Player đã bắn Fireball.
        if (owner != null &&
            (other.gameObject == owner ||
             other.transform.IsChildOf(
                 owner.transform)))
        {
            return;
        }

        // Tìm Enemy từ collider hiện tại hoặc object cha.
        EnermyHealth enemy =
            other.GetComponentInParent<EnermyHealth>();

        if (enemy != null)
        {
            HitEnemy(enemy);
            return;
        }

        // Chỉ biến mất khi chạm tường/map thuộc Obstacle Layer.
        if (IsInLayerMask(
                other.gameObject,
                obstacleLayer))
        {
            DestroyProjectile();
        }
    }

    private void HitEnemy(
        EnermyHealth enemy)
    {
        if (hasHit || enemy == null)
            return;

        hasHit = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

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

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        SpawnHitEffect();

        Destroy(gameObject);
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

    private void RotateProjectile()
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

    private bool IsInLayerMask(
        GameObject target,
        LayerMask layerMask)
    {
        return (
            layerMask.value &
            (1 << target.layer)
        ) != 0;
    }
}