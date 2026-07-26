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

    if (GameManager.Instance != null &&
        GameManager.Instance.player != null)
    {
        Players p = GameManager.Instance.player.GetComponent<Players>();

        if (p != null)
            pickupPoint = p.pickupPoint;
    }

    StartCoroutine(FindPickupPoint());

    if (AudioManager.Instance != null)
        AudioManager.Instance.PlaySFX(AudioManager.Instance.coinDropSound);

    StartCoroutine(PickupDelay());
    StartCoroutine(BlinkRoutine());

    Destroy(gameObject, lifeTime);
}




    IEnumerator FindPickupPoint()
{
    while (pickupPoint == null)
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.player != null)
        {
            Players p = GameManager.Instance.player.GetComponent<Players>();

            if (p != null)
                pickupPoint = p.pickupPoint;
        }

        yield return null;
    }
}

    void Update()
{
    if (!canPickup)
        return;

    if (pickupPoint == null &&
        GameManager.Instance != null &&
        GameManager.Instance.player != null)
    {
        Players p = GameManager.Instance.player.GetComponent<Players>();

        if (p != null)
            pickupPoint = p.pickupPoint;
    }

    if (pickupPoint == null)
        return;

    float distance = Vector2.Distance(transform.position, pickupPoint.position);

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