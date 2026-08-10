using System.Collections;
using UnityEngine;

public class WorldItemDropMotion : MonoBehaviour
{
    [Header("Drop Motion")]
    [Tooltip("Khoảng cách item văng ngang khỏi enemy.")]
    [Min(0f)]
    [SerializeField]
    private float scatterDistance = 0.7f;

    [Tooltip("Độ cao của quỹ đạo văng.")]
    [Min(0f)]
    [SerializeField]
    private float arcHeight = 0.8f;

    [Tooltip("Thời gian item bay rồi đáp đất.")]
    [Min(0.01f)]
    [SerializeField]
    private float dropDuration = 0.4f;

    [Header("Pickup")]
    [Min(0f)]
    [SerializeField]
    private float pickupDelay = 0.3f;

    [Header("Lifetime")]
    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 8f;

    [Min(0f)]
    [SerializeField]
    private float blinkDuration = 2f;

    [Header("Blink")]
    [Min(0.01f)]
    [SerializeField]
    private float blinkStartInterval = 0.25f;

    [Min(0.01f)]
    [SerializeField]
    private float blinkEndInterval = 0.07f;

    [Header("References")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Collider2D pickupCollider;

    private Vector3 startPosition;
    private Vector3 groundPosition;

    private Color originalColor;

    private bool landed;

    public bool Landed =>
        landed;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (pickupCollider == null)
        {
            pickupCollider =
                GetComponent<Collider2D>();
        }

        if (spriteRenderer != null)
        {
            originalColor =
                spriteRenderer.color;
        }
    }

    private void Start()
    {
        StartDrop();
    }

    // =====================================================
    // DROP
    // =====================================================

    private void StartDrop()
    {
        StopAllCoroutines();

        landed = false;

        startPosition =
            transform.position;

        /*
         * Chọn hướng văng ngẫu nhiên.
         */
        Vector2 direction =
            Random.insideUnitCircle;

        /*
         * Ưu tiên văng ngang nhiều hơn,
         * tránh item chỉ bay thẳng lên/xuống.
         */
        direction.y *= 0.45f;

        if (direction.sqrMagnitude <
            0.05f)
        {
            direction =
                Random.value < 0.5f
                    ? Vector2.left
                    : Vector2.right;
        }

        direction.Normalize();

        float distance =
            Random.Range(
                scatterDistance * 0.6f,
                Mathf.Max(
                    scatterDistance * 0.6f,
                    scatterDistance
                )
            );

        groundPosition =
            startPosition +
            new Vector3(
                direction.x,
                direction.y,
                0f
            ) *
            distance;

        /*
         * Trong lúc đang bay,
         * chưa cho Player nhặt.
         */
        if (pickupCollider != null)
        {
            pickupCollider.enabled =
                false;
        }

        ResetVisual();

        StartCoroutine(
            DropRoutine()
        );
    }

    private IEnumerator DropRoutine()
    {
        float timer = 0f;

        float duration =
            Mathf.Max(
                0.01f,
                dropDuration
            );

        while (timer < duration)
        {
            timer +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            /*
             * Di chuyển từ enemy
             * tới điểm rơi.
             */
            Vector3 position =
                Vector3.Lerp(
                    startPosition,
                    groundPosition,
                    t
                );

            /*
             * Quỹ đạo cong:
             *
             * 4 * t * (1 - t)
             *
             * đầu = 0
             * giữa = 1
             * cuối = 0
             */
            float arc =
                4f *
                t *
                (1f - t);

            position.y +=
                arc *
                arcHeight;

            transform.position =
                position;

            yield return null;
        }

        transform.position =
            groundPosition;

        landed = true;

        /*
         * Lifetime bắt đầu sau khi
         * item chạm đất.
         */
        StartCoroutine(
            LifeRoutine()
        );

        if (pickupDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    pickupDelay
                );
        }

        if (pickupCollider != null)
        {
            pickupCollider.enabled =
                true;
        }
    }

    // =====================================================
    // LIFETIME
    // =====================================================

    private IEnumerator LifeRoutine()
    {
        float safeLife =
            Mathf.Max(
                0.1f,
                lifeTime
            );

        float safeBlink =
            Mathf.Clamp(
                blinkDuration,
                0f,
                safeLife
            );

        float normalTime =
            safeLife -
            safeBlink;

        if (normalTime > 0f)
        {
            yield return
                new WaitForSeconds(
                    normalTime
                );
        }

        if (safeBlink <= 0f)
        {
            Destroy(gameObject);
            yield break;
        }

        yield return
            StartCoroutine(
                BlinkRoutine(
                    safeBlink
                )
            );

        Destroy(gameObject);
    }

    // =====================================================
    // BLINK
    // =====================================================

    private IEnumerator BlinkRoutine(
        float duration)
    {
        float timer = 0f;

        bool visible = true;

        while (timer < duration)
        {
            float progress =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            float interval =
                Mathf.Lerp(
                    blinkStartInterval,
                    blinkEndInterval,
                    progress
                );

            interval =
                Mathf.Max(
                    0.01f,
                    interval
                );

            visible =
                !visible;

            SetVisible(
                visible
            );

            yield return
                new WaitForSeconds(
                    interval
                );

            timer +=
                interval;
        }

        SetVisible(false);
    }

    // =====================================================
    // VISUAL
    // =====================================================

    private void SetVisible(
        bool visible)
    {
        if (spriteRenderer == null)
            return;

        Color color =
            originalColor;

        color.a =
            visible
                ? originalColor.a
                : 0f;

        spriteRenderer.color =
            color;
    }

    private void ResetVisual()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color =
            originalColor;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnValidate()
    {
        scatterDistance =
            Mathf.Max(
                0f,
                scatterDistance
            );

        arcHeight =
            Mathf.Max(
                0f,
                arcHeight
            );

        dropDuration =
            Mathf.Max(
                0.01f,
                dropDuration
            );

        pickupDelay =
            Mathf.Max(
                0f,
                pickupDelay
            );

        lifeTime =
            Mathf.Max(
                0.1f,
                lifeTime
            );

        blinkDuration =
            Mathf.Clamp(
                blinkDuration,
                0f,
                lifeTime
            );

        blinkStartInterval =
            Mathf.Max(
                0.01f,
                blinkStartInterval
            );

        blinkEndInterval =
            Mathf.Max(
                0.01f,
                blinkEndInterval
            );
    }
}