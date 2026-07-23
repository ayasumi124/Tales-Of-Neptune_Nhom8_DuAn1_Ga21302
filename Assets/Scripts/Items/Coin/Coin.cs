using UnityEngine;
using System.Collections;

public class Coin : MonoBehaviour
{
    public int value = 1;

    [Header("Life")]
    public float lifeTime = 8f;
    public float blinkTime = 2f;

    [Header("Jump")]
    public float jumpForce = 3f;

    [Header("Magnet")]
    public float pickupDelay = 0.5f;
    public float magnetRange = 1.2f;
    public float flySpeed = 8f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    Transform player;

    bool canPickup;
    bool flying;

    void Start()
{
    rb = GetComponent<Rigidbody2D>();
    sr = GetComponent<SpriteRenderer>();

    player = GameObject.FindGameObjectWithTag("Player").transform;

    Vector2 force = new Vector2(
        Random.Range(-1f,1f),
        1f).normalized;

    rb.AddForce(force * jumpForce, ForceMode2D.Impulse);

    AudioManager.Instance.PlaySFX(
        AudioManager.Instance.coinDropSound);

    StartCoroutine(PickupDelay());

    StartCoroutine(BlinkRoutine());

    Destroy(gameObject, lifeTime);
}

void Update()
{
    if(!canPickup)
        return;

    if(player==null)
        return;

    if(!flying)
    {
        float dis =
            Vector2.Distance(
                transform.position,
                player.position);

        if(dis < magnetRange)
        {
            flying = true;

            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }
    }

    if(flying)
    {
        transform.position =
            Vector3.MoveTowards(
                transform.position,
                player.position,
                flySpeed * Time.deltaTime);

        if(Vector2.Distance(
            transform.position,
            player.position) < 0.15f)
        {
            Pickup();
        }
    }
}

IEnumerator PickupDelay()
{
    yield return new WaitForSeconds(pickupDelay);

    canPickup = true;
}

void Pickup()
{
    CoinUI.Instance.AddCoin(value);

    AudioManager.Instance.PlaySFX(
        AudioManager.Instance.coinPickupSound);

    Destroy(gameObject);
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