using UnityEngine;

public class MerchantNPC : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField]
    private GameObject interactionPrompt;

    [Header("Merchant Audio")]
    [SerializeField]
    private AudioSource merchantAudioSource;

    [SerializeField]
    private AudioClip merchantLoopSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float merchantVolume = 1f;

    private bool playerNearby;

    private void Start()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        if (merchantAudioSource != null)
        {
            merchantAudioSource.playOnAwake = false;
            merchantAudioSource.loop = true;
            merchantAudioSource.volume =
                merchantVolume;
        }
    }

    private void Update()
    {
        if (!playerNearby)
            return;

        if (ShopManager.Instance == null)
            return;

        if (ShopManager.Instance.IsShopOpen)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        ShopManager.Instance.OpenShop();
    }

    private void PlayMerchantSound()
    {
        if (merchantAudioSource == null ||
            merchantLoopSound == null)
        {
            return;
        }

        merchantAudioSource.clip =
            merchantLoopSound;

        merchantAudioSource.loop = true;

        merchantAudioSource.volume =
            merchantVolume;

        if (!merchantAudioSource.isPlaying)
        {
            merchantAudioSource.Play();
        }
    }

    private void StopMerchantSound()
    {
        if (merchantAudioSource == null)
            return;

        merchantAudioSource.Stop();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }

        PlayMerchantSound();
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }

        StopMerchantSound();

        if (ShopManager.Instance != null &&
            ShopManager.Instance.IsShopOpen)
        {
            ShopManager.Instance.CloseShop();
        }
    }

    private void OnDisable()
    {
        StopMerchantSound();
    }
}