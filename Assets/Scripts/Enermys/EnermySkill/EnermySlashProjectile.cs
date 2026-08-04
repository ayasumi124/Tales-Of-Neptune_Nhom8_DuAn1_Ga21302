using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnermySlashProjectile : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    private float speed = 6f;

    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 3f;

    [Tooltip(
        "Xoay sprite theo hướng bay."
    )]
    [SerializeField]
    private bool rotateToDirection = true;

    [Tooltip(
        "Góc bù nếu sprite gốc không quay sang phải."
    )]
    [SerializeField]
    private float rotationOffset;

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 2;

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private bool destroyOnHit = true;

    [Header("Environment")]
    [SerializeField]
    private bool destroyOnObstacle = true;

    [SerializeField]
    private LayerMask obstacleLayer;

    [Header("Visual")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Animator animator;

    [Header("Audio")]
    [SerializeField]
    private AudioClip hitSound;

    private Rigidbody2D rb;
    private Vector2 direction;

    private GameObject owner;
    private bool initialized;
    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        Destroy(
            gameObject,
            Mathf.Max(
                0.1f,
                lifeTime
            )
        );
    }

    public void Initialize(
        Vector2 moveDirection,
        GameObject projectileOwner,
        int overrideDamage = -1,
        float overrideSpeed = -1f)
    {

        direction =
            moveDirection.sqrMagnitude > 0.001f
                ? moveDirection.normalized
                : Vector2.right;
        Debug.Log(direction);
        owner = projectileOwner;

        if (overrideDamage > 0)
        {
            damage =
                overrideDamage;
        }

        if (overrideSpeed > 0f)
        {
            speed =
                overrideSpeed;
        }

        initialized = true;

        UpdateVisualDirection();
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
            direction *
            Mathf.Max(
                0f,
                speed
            );
    }

    private void UpdateVisualDirection()
    {
        if (rotateToDirection)
        {
            float angle =
                Mathf.Atan2(
                    direction.y,
                    direction.x
                ) *
                Mathf.Rad2Deg;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle + rotationOffset
                );

            return;
        }

        if (spriteRenderer != null &&
            Mathf.Abs(direction.x) >
            0.01f)
        {
            spriteRenderer.flipX =
                direction.x < 0f;
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (hasHit ||
            other == null)
        {
            return;
        }

        if (owner != null &&
            other.transform.root.gameObject ==
            owner.transform.root.gameObject)
        {
            return;
        }

        int otherLayerMask =
            1 <<
            other.gameObject.layer;

        if ((targetLayer.value &
             otherLayerMask) != 0)
        {
            TryDamageTarget(other);
            return;
        }

        if (destroyOnObstacle &&
            (obstacleLayer.value &
             otherLayerMask) != 0)
        {
            DestroyProjectile();
        }
    }

    private void TryDamageTarget(
        Collider2D hit)
    {
        Health health =
            hit.GetComponentInParent<Health>();

        if (health != null &&
            !health.IsDead)
        {
            health.TakeDamage(damage);

            OnSuccessfulHit();
            return;
        }

        CloneHealth cloneHealth =
            hit.GetComponentInParent<CloneHealth>();

        if (cloneHealth != null)
        {
            cloneHealth.TakeDamage(damage);

            OnSuccessfulHit();
        }
    }

    private void OnSuccessfulHit()
    {
        hasHit = true;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        if (AudioManager.Instance != null &&
            hitSound != null)
        {
            AudioManager.Instance.PlaySFX(
                hitSound
            );
        }

        if (destroyOnHit)
        {
            DestroyProjectile();
        }
    }

    private void DestroyProjectile()
    {
        hasHit = true;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        Destroy(gameObject);
    }

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
    }
}