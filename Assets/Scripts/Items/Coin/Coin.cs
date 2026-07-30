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
    private Transform pickupPoint;

    private bool canPickup;
    private bool flying;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        TryGetPickupPoint();

        if (pickupPoint == null)
            StartCoroutine(FindPickupPoint());

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.coinDropSound
            );
        }

        StartCoroutine(PickupDelay());
        StartCoroutine(BlinkRoutine());

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!canPickup)
            return;

        if (pickupPoint == null)
            TryGetPickupPoint();

        if (pickupPoint == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            pickupPoint.position
        );

        if (!flying && distance <= magnetRange)
            flying = true;

        if (!flying)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            pickupPoint.position,
            flySpeed * Time.deltaTime
        );
    }

    void TryGetPickupPoint()
    {
        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.Player == null)
            return;

        Players players =
            GameManager.Instance.Player.GetComponent<Players>();

        if (players != null)
            pickupPoint = players.pickupPoint;
    }

    IEnumerator FindPickupPoint()
    {
        while (pickupPoint == null)
        {
            TryGetPickupPoint();
            yield return null;
        }
    }

    IEnumerator PickupDelay()
    {
        yield return new WaitForSeconds(pickupDelay);
        canPickup = true;
    }

    IEnumerator BlinkRoutine()
    {
        float waitTime = Mathf.Max(0f, lifeTime - blinkTime);

        yield return new WaitForSeconds(waitTime);

        while (true)
        {
            if (sr != null)
                sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.15f);
        }
    }
}