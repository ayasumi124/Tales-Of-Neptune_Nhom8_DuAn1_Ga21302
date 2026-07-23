using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public CoinUI coinUI;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Coin coin = other.GetComponent<Coin>();

        if (coin == null)
            return;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.coinPickupSound);

        coinUI.AddCoin(coin.value);

        Destroy(other.gameObject);
    }
}