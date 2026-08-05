using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class GroundSlamWave : MonoBehaviour
{
    [Header("Movement")]
    [Min(0f)]
    [SerializeField]
    private float speed = 6f;

    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 2f;

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 3;

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private bool destroyOnHit = true;

    [Header("Visual")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [Tooltip("Bật nếu sprite gốc quay sang phải.")]
    [SerializeField]
    private bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private Vector2 moveDirection;
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
        Vector2 direction,
        GameObject waveOwner,
        int overrideDamage,
        float overrideSpeed)
    {
        moveDirection =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.right;

        owner = waveOwner;

        damage =
            Mathf.Max(
                1,
                overrideDamage
            );

        speed =
            Mathf.Max(
                0f,
                overrideSpeed
            );

        initialized = true;

        UpdateVisual();
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
            moveDirection * speed;
    }

    private void UpdateVisual()
    {
        if (spriteRenderer == null)
            return;

        if (Mathf.Abs(moveDirection.x) >
            0.01f)
        {
            bool movingLeft =
                moveDirection.x < 0f;

            spriteRenderer.flipX =
                spriteFacesRight
                    ? movingLeft
                    : !movingLeft;
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

        int layerMask =
            1 << other.gameObject.layer;

        if ((targetLayer.value &
             layerMask) == 0)
        {
            return;
        }

        CloneHealth cloneHealth =
            other.GetComponentInParent<
                CloneHealth
            >();

        if (cloneHealth != null)
        {
            cloneHealth.TakeDamage(
                damage
            );

            OnHit();
            return;
        }

        Health playerHealth =
            other.GetComponentInParent<
                Health
            >();

        if (playerHealth != null &&
            !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(
                damage
            );

            OnHit();
        }
    }

    private void OnHit()
    {
        hasHit = true;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
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