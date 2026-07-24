using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;

    [Header("Life")]
    public float lifeTime = 8f;
    public float blinkTime = 2f;


    [Header("Bounce")]
    public float bounceHeight = 5f;
    public float bounceDuration = 10f;
    public float scatterDistance = 0.7f;


    [Header("Magnet")]
    public float pickupDelay = 0.5f;
    public float magnetRange = 1.5f;
    public float flySpeed = 8f;


    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private Transform player;

    private Transform pickupPoint;

    private bool canPickup;
    private bool flying;
    private Coroutine bounceCoroutine;




    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (GameManager.Instance != null)
        {
            pickupPoint = GameManager.Instance.PickupPoint;
        }

        StartCoroutine(FindPickupPoint());
        // Không dùng physics để nảy
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }


        // Vị trí coin sau khi văng ra
        Vector2 random =
            Random.insideUnitCircle * scatterDistance;


        Vector3 target =
            transform.position + (Vector3)random;


        bounceCoroutine = StartCoroutine(BounceRoutine(target));
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.coinDropSound);

        Debug.Log("PickupPoint = " + pickupPoint);

        StartCoroutine(PickupDelay());


        StartCoroutine(BlinkRoutine());


        Destroy(gameObject, lifeTime);
    }



    IEnumerator BounceRoutine(Vector3 target)
    {
        Vector3 start = transform.position;

        float[] heights = { 0.80f, 0.70f, 0.60f, 0.50f, 0.40f, 0.30f, 0.20f, 0.10f, 0.05f };
        float[] durations = { 0.80f, 0.70f, 0.60f, 0.50f, 0.40f, 0.30f, 0.20f, 0.10f, 0.05f };

        Vector3 currentStart = start;

        for (int bounce = 0; bounce < heights.Length; bounce++)
        {
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / durations[bounce];

                Vector3 pos = Vector3.Lerp(currentStart, target, t);

                float height =
                    Mathf.Sin(t * Mathf.PI) * heights[bounce];

                pos.y += height;

                transform.position = pos;

                yield return null;
            }

            transform.position = target;

            // Phát âm thanh khi chạm đất
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.coinBounceSound);

            currentStart = target;
        }
    }


    IEnumerator FindPickupPoint()
    {
        while (pickupPoint == null)
        {
            if (GameManager.Instance != null)
            {
                pickupPoint = GameManager.Instance.PickupPoint;
            }

            yield return null;
        }

        Debug.Log("Đã tìm thấy PickupPoint!");
    }

    void Update()
    {

        if (!canPickup)
            return;
        if (pickupPoint == null && GameManager.Instance != null)
            pickupPoint = GameManager.Instance.PickupPoint;

        if (pickupPoint == null)
            return;

        float distance = Vector2.Distance(transform.position, pickupPoint.position);

        // Chỉ bật hút khi ở trong phạm vi
        if (!flying && distance <= magnetRange)
        {
            flying = true;

            if (bounceCoroutine != null)
                StopCoroutine(bounceCoroutine);
        }

        if (!flying)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            pickupPoint.position,
            flySpeed * Time.deltaTime);
    }





    IEnumerator PickupDelay()
    {
        yield return new WaitForSeconds(pickupDelay);

        canPickup = true;
    }





    IEnumerator BlinkRoutine()
    {
        yield return new WaitForSeconds(lifeTime - blinkTime);


        while (true)
        {
            sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.15f);
        }
    }
}