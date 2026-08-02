using System.Collections.Generic;
using UnityEngine;

public class FireBreathSegment : MonoBehaviour
{
    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 0.35f;

    [Header("Damage")]
    [SerializeField] private int damage = 10;
    [SerializeField] private int burnDamage = 10;
    [SerializeField] private float burnInterval = 1f;
    [SerializeField] private float burnDuration = 3f;

    [Header("Damage Check")]
    [Tooltip("Khoảng thời gian giữa mỗi lần kiểm tra Enemy.")]
    [SerializeField] private float damageCheckInterval = 0.05f;

    [Tooltip(
        "Cùng một Enemy chỉ nhận damage Fire Breath " +
        "sau mỗi khoảng thời gian này."
    )]
    [SerializeField] private float sameEnemyDamageInterval = 0.25f;

    [Header("Collision")]
    [SerializeField] private float damageRadius = 0.45f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Effects")]
    [SerializeField] private GameObject burnEffectPrefab;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Bật nếu sprite gốc hướng sang phải.")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Scale")]
    [SerializeField] private float baseScale = 0.3f;
    [SerializeField] private float scalePerLayer = 0.15f;
    [Header("Directional Visual Offset")]
    [SerializeField] private Vector2 rightOffset = Vector2.zero;
    [SerializeField] private Vector2 leftOffset = Vector2.zero;
    [SerializeField] private Vector2 upOffset = new Vector2(-0.2f, 0f);
    [SerializeField] private Vector2 downOffset = new Vector2(0.2f, 0f);

    private Vector3 basePosition;

    private Transform ownerTransform;
    private Transform followOrigin;
    private Players ownerPlayer;

    private Vector2 direction;
    private float distanceFromOrigin;
    private int segmentIndex;

    private float damageCheckTimer;
    private bool initialized;

    /*
     * Dùng chung cho mọi segment.
     * Tránh nhiều lớp lửa cùng gây damage
     * lên một Enemy trong cùng một frame.
     */
    private static readonly Dictionary<EnermyHealth, float>
        nextDamageTimes =
            new Dictionary<EnermyHealth, float>();

    public void Initialize(
        Vector2 castDirection,
        GameObject skillOwner,
        Transform origin,
        float segmentDistance,
        int index)
    {
        if (skillOwner == null)
        {
            Destroy(gameObject);
            return;
        }

        ownerTransform = skillOwner.transform;
        ownerPlayer =
            skillOwner.GetComponent<Players>();

        followOrigin = origin;

        direction =
            GetCardinalDirection(
                castDirection.sqrMagnitude > 0.001f
                    ? castDirection
                    : Vector2.down
            );

        distanceFromOrigin =
            Mathf.Max(0f, segmentDistance);

        segmentIndex =
            Mathf.Max(0, index);

        initialized = true;

        float scaleMultiplier =
            Mathf.Max(
                0.01f,
                baseScale +
                segmentIndex * scalePerLayer
            );

        transform.localScale =
            Vector3.one * scaleMultiplier;

        UpdateFollowPosition();
        basePosition = transform.position;
        ApplyDirectionalOffset();
        ConfigureVisualDirection();

        // Kiểm tra damage ngay khi vừa sinh ra.
        CheckDamage();

        damageCheckTimer = 0f;

        Destroy(
            gameObject,
            Mathf.Max(0.05f, lifeTime)
        );
    }

    private void Update()
    {
        if (!initialized)
            return;

        damageCheckTimer -= Time.deltaTime;

        if (damageCheckTimer <= 0f)
        {
            damageCheckTimer =
                Mathf.Max(
                    0.02f,
                    damageCheckInterval
                );

            CheckDamage();
        }
    }

    private void LateUpdate()
    {
        if (!initialized ||
            ownerTransform == null)
        {
            return;
        }

        /*
         * Cập nhật hướng theo Player.
         */
        if (ownerPlayer != null &&
            ownerPlayer.LastDirection.sqrMagnitude >
            0.001f)
        {
            direction =
                GetCardinalDirection(
                    ownerPlayer.LastDirection
                );
        }

        UpdateFollowPosition();
        ConfigureVisualDirection();
        ApplyDirectionalOffset();
    }

    private void ApplyDirectionalOffset()
{
    Vector2 offset = Vector2.zero;

    if (direction == Vector2.right)
        offset = rightOffset;
    else if (direction == Vector2.left)
        offset = leftOffset;
    else if (direction == Vector2.up)
        offset = upOffset;
    else if (direction == Vector2.down)
        offset = downOffset;

    transform.position += (Vector3)offset;
}
    private void UpdateFollowPosition()
    {
        if (ownerTransform == null)
            return;

        Vector3 originPosition =
            followOrigin != null
                ? followOrigin.position
                : ownerTransform.position;

        transform.position =
            originPosition +
            (Vector3)(
                direction *
                distanceFromOrigin
            );
    }

    private Vector2 GetCardinalDirection(
        Vector2 input)
    {
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

    private void ConfigureVisualDirection()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;

        if (direction == Vector2.right)
        {
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f
                );

            spriteRenderer.flipX =
                !spriteFacesRight;
        }
        else if (direction == Vector2.left)
        {
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    0f
                );

            spriteRenderer.flipX =
                spriteFacesRight;
        }
        else if (direction == Vector2.up)
        {
            /*
             * Sprite ngang quay lên.
             */
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );

            spriteRenderer.flipX =
                !spriteFacesRight;
        }
        else if (direction == Vector2.down)
        {
            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    90f
                );

            spriteRenderer.flipX =
                spriteFacesRight;

            spriteRenderer.flipY = true;
        }
    }

    private void CheckDamage()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                damageRadius,
                enemyLayer
            );

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            EnermyHealth enemy =
                hit.GetComponentInParent<EnermyHealth>();

            if (enemy == null)
                continue;

            /*
             * Enemy đang trong thời gian miễn damage
             * của Fire Breath thì bỏ qua.
             */
            if (nextDamageTimes.TryGetValue(
                    enemy,
                    out float nextDamageTime))
            {
                if (Time.time < nextDamageTime)
                    continue;
            }

            nextDamageTimes[enemy] =
                Time.time +
                Mathf.Max(
                    0.05f,
                    sameEnemyDamageInterval
                );

            enemy.TakeDamage(
                damage,
                direction
            );

            ApplyBurn(enemy);
        }
    }

    private void ApplyBurn(
        EnermyHealth enemy)
    {
        if (enemy == null)
            return;

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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }

    private void OnDestroy()
    {
        /*
         * Xóa các Enemy đã bị Destroy khỏi Dictionary
         * để tránh giữ reference rác lâu dài.
         */
        List<EnermyHealth> invalidEnemies =
            new List<EnermyHealth>();

        foreach (
            KeyValuePair<EnermyHealth, float> pair
            in nextDamageTimes)
        {
            if (pair.Key == null)
                invalidEnemies.Add(pair.Key);
        }

        foreach (EnermyHealth enemy
                 in invalidEnemies)
        {
            nextDamageTimes.Remove(enemy);
        }
    }
}