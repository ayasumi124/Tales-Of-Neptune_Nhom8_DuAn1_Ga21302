using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;

    [Header("Life")]
    public float lifeTime = 8f;
    public float blinkTime = 2f;




    [Header("Magnet")]
    public float pickupDelay = 0.5f;
    public float magnetRange = 1.5f;
    public float flySpeed = 8f;



    private SpriteRenderer sr;

    private Transform player;

    private Transform pickupPoint;

    private bool canPickup;
    private bool flying;




    void Start()
    {
        
        sr = GetComponent<SpriteRenderer>();

        if (GameManager.Instance != null)
        {
            pickupPoint = GameManager.Instance.PickupPoint;
        }

        StartCoroutine(FindPickupPoint());
        // Không dùng physics để nảy
        

        
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.coinDropSound);

        Debug.Log("PickupPoint = " + pickupPoint);

        StartCoroutine(PickupDelay());


        StartCoroutine(BlinkRoutine());


        Destroy(gameObject, lifeTime);
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