using System.Collections;
using UnityEngine;

public class WorldItemPickup : MonoBehaviour
{
    // =====================================================
    // ITEM
    // =====================================================

    [Header("Item")]
    [SerializeField]
    private ItemData itemData;

    [Min(1)]
    [SerializeField]
    private int quantity = 1;

    // =====================================================
    // INTERACTION
    // =====================================================

    [Header("Interaction")]
    [SerializeField]
    private bool requireKey = true;

    [SerializeField]
    private KeyCode pickupKey =
        KeyCode.E;

    [SerializeField]
    private GameObject keyIcon;

    // =====================================================
    // DROP MOTION
    // =====================================================

    [Header("Drop Motion")]
    [Tooltip(
        "Khoảng cách item văng khỏi Enemy."
    )]
    [Min(0f)]
    [SerializeField]
    private float scatterDistance = 0.8f;

    [Tooltip(
        "Độ cao cú văng."
    )]
    [Min(0f)]
    [SerializeField]
    private float jumpHeight = 0.8f;

    [Tooltip(
        "Thời gian từ lúc văng đến lúc đáp đất."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float jumpDuration = 0.45f;

    [Tooltip(
        "Tốc độ xoay khi đang văng."
    )]
    [Min(0f)]
    [SerializeField]
    private float rotateSpeed = 360f;

    [Header("Drop Audio")]
    [SerializeField]
    private AudioClip dropSound;

    [Range(0f, 2f)]
    [SerializeField]
    private float dropVolume = 1f;
    // =====================================================
    // PICKUP DELAY
    // =====================================================

    [Header("Pickup Delay")]
    [Min(0f)]
    [SerializeField]
    private float pickupDelay = 0.35f;

    // =====================================================
    // LIFE
    // =====================================================

    [Header("Life")]
    [Tooltip(
        "Item tồn tại bao lâu SAU KHI đáp đất."
    )]
    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 8f;

    [Tooltip(
        "Bao nhiêu giây cuối item bắt đầu nhấp nháy."
    )]
    [Min(0f)]
    [SerializeField]
    private float blinkTime = 2f;

    [Tooltip(
        "Tốc độ nhấp nháy giống Coin."
    )]
    [Min(0.01f)]
    [SerializeField]
    private float blinkInterval = 0.15f;

    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Collider2D pickupCollider;

    // =====================================================
    // RUNTIME
    // =====================================================

    private bool playerInside;
    private bool pickedUp;
    private bool canPickup;
    private bool landed;

    private Coroutine dropCoroutine;
    private Coroutine lifeCoroutine;

    // =====================================================
    // UNITY
    // =====================================================

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

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }

    private void Start()
    {
        /*
         * Không cho nhặt ngay khi vừa spawn.
         */
        canPickup = false;
        landed = false;

        if (pickupCollider != null)
        {
            pickupCollider.enabled = false;
        }

        BeginDrop();
    }

    private void Update()
    {
        if (pickedUp ||
            !canPickup ||
            !playerInside ||
            !requireKey)
        {
            return;
        }

        if (Input.GetKeyDown(
                pickupKey))
        {
            TryPickup();
        }
    }

    // =====================================================
    // SETUP FROM ENERMY ITEM DROP
    // =====================================================

    public void Setup(
        ItemData data,
        int amount)
    {
        if (data == null)
            return;

        itemData = data;

        quantity =
            Mathf.Max(
                1,
                amount
            );

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<
                    SpriteRenderer
                >();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite =
                data.Icon;

            spriteRenderer.enabled =
                true;
        }
    }

    // =====================================================
    // DROP
    // =====================================================

    private void BeginDrop()
    {
        if (dropCoroutine != null)
        {
            StopCoroutine(
                dropCoroutine
            );
        }

        // Âm thanh item vừa rơi ra
        if (AudioManager.Instance != null &&
            dropSound != null)
        {
            AudioManager.Instance.PlaySFX(
                dropSound,
                dropVolume
            );
        }

        Vector2 direction =
            Random.insideUnitCircle;

        direction.y *= 0.55f;

        if (direction.sqrMagnitude <
            0.05f)
        {
            direction =
                Random.value < 0.5f
                    ? Vector2.left
                    : Vector2.right;
        }

        direction.Normalize();

        float minDistance =
            scatterDistance * 0.5f;

        float distance =
            Random.Range(
                minDistance,
                Mathf.Max(
                    minDistance,
                    scatterDistance
                )
            );

        Vector3 start =
            transform.position;

        Vector3 target =
            start +
            (Vector3)(
                direction *
                distance
            );

        dropCoroutine =
            StartCoroutine(
                DropRoutine(
                    start,
                    target
                )
            );
    }

    private IEnumerator DropRoutine(
        Vector3 start,
        Vector3 target)
    {
        float timer = 0f;

        float duration =
            Mathf.Max(
                0.01f,
                jumpDuration
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

            float ease =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            /*
             * Di chuyển ngang tới điểm đáp.
             */
            Vector3 position =
                Vector3.Lerp(
                    start,
                    target,
                    ease
                );

            /*
             * Một cú văng theo cung.
             *
             * KHÔNG BOUNCE.
             */
            position.y +=
                Mathf.Sin(
                    ease *
                    Mathf.PI
                ) *
                jumpHeight;

            transform.position =
                position;

            /*
             * Xoay nhẹ trong lúc bay.
             */
            if (spriteRenderer != null)
            {
                spriteRenderer.transform.Rotate(
                    0f,
                    0f,
                    rotateSpeed *
                    Time.deltaTime
                );
            }

            yield return null;
        }

        transform.position =
            target;

        /*
         * Trả sprite về thẳng.
         */
        if (spriteRenderer != null)
        {
            spriteRenderer.transform.rotation =
                Quaternion.identity;
        }

        landed = true;

        dropCoroutine = null;

        /*
         * Item bắt đầu tính thời gian tồn tại
         * sau khi chạm đất.
         */
        lifeCoroutine =
            StartCoroutine(
                LifeRoutine()
            );

        /*
         * Delay một chút trước khi cho nhặt.
         */
        if (pickupDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    pickupDelay
                );
        }

        if (pickedUp)
            yield break;

        canPickup = true;

        if (pickupCollider != null)
        {
            pickupCollider.enabled =
                true;
        }
    }

    // =====================================================
    // LIFE / BLINK
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
                blinkTime,
                0f,
                safeLife
            );

        float waitTime =
            safeLife -
            safeBlink;

        /*
         * Nằm bình thường trên mặt đất.
         */
        if (waitTime > 0f)
        {
            yield return
                new WaitForSeconds(
                    waitTime
                );
        }

        /*
         * Giống Coin:
         * vài giây cuối bắt đầu nhấp nháy.
         */
        if (safeBlink > 0f)
        {
            float timer = 0f;

            while (timer < safeBlink)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled =
                        !spriteRenderer.enabled;
                }

                float interval =
                    Mathf.Max(
                        0.01f,
                        blinkInterval
                    );

                yield return
                    new WaitForSeconds(
                        interval
                    );

                timer +=
                    interval;
            }
        }

        Destroy(
            gameObject
        );
    }

    // =====================================================
    // PLAYER TRIGGER
    // =====================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!canPickup ||
            pickedUp)
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null)
        {
            keyIcon.SetActive(
                true
            );
        }

        if (!requireKey)
        {
            TryPickup();
        }
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!canPickup ||
            pickedUp)
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null &&
            !keyIcon.activeSelf)
        {
            keyIcon.SetActive(
                true
            );
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(
                false
            );
        }
    }

    // =====================================================
    // PICKUP
    // =====================================================

    private void TryPickup()
    {
        if (pickedUp ||
            !canPickup)
        {
            return;
        }

        if (itemData == null)
        {
            Debug.LogError(
                $"{gameObject.name} chưa được gán ItemData."
            );

            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy InventoryManager."
            );

            return;
        }

        if (!InventoryManager.Instance
                .CanAddItem(
                    itemData,
                    quantity
                ))
        {
            Debug.Log(
                "Inventory không đủ chỗ."
            );

            return;
        }

        bool added =
            InventoryManager.Instance
                .AddItem(
                    itemData,
                    quantity
                );

        if (!added)
        {
            Debug.Log(
                "Không thể nhặt vì Inventory đã đầy."
            );

            return;
        }

        pickedUp = true;
        canPickup = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(
                false
            );
        }

        if (AudioManager.Instance != null &&
            itemData.pickupSound != null)
        {
            AudioManager.Instance
                .PlayItemSFX(
                    itemData.pickupSound,
                    itemData.pickupVolume
                );
        }

        Debug.Log(
            $"Đã nhặt " +
            $"{itemData.ItemName} " +
            $"x{quantity}"
        );

        Destroy(
            gameObject
        );
    }

    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        quantity =
            Mathf.Max(
                1,
                quantity
            );

        scatterDistance =
            Mathf.Max(
                0f,
                scatterDistance
            );
        dropVolume =
    Mathf.Max(
        0f,
        dropVolume
    );

        jumpHeight =
            Mathf.Max(
                0f,
                jumpHeight
            );

        jumpDuration =
            Mathf.Max(
                0.01f,
                jumpDuration
            );

        rotateSpeed =
            Mathf.Max(
                0f,
                rotateSpeed
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

        blinkTime =
            Mathf.Clamp(
                blinkTime,
                0f,
                lifeTime
            );

        blinkInterval =
            Mathf.Max(
                0.01f,
                blinkInterval
            );
    }
}