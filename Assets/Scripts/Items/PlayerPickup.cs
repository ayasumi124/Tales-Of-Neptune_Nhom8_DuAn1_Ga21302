using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(
        Collider2D other)
    {
        Coin coin =
            other.GetComponent<Coin>();

        if (coin == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.coinPickupSound
            );
        }

        if (CoinUI.Instance != null)
        {
            CoinUI.Instance.AddCoin(
                coin.value
            );
        }

        Destroy(other.gameObject);
    }
}