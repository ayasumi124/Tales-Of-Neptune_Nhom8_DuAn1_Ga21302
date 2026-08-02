using UnityEngine;

public class WorldItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemData itemData;

    [Min(1)]
    [SerializeField] private int quantity = 1;

    [Header("Interaction")]
    [SerializeField] private bool requireKey = true;

    [SerializeField]
    private KeyCode pickupKey =
        KeyCode.E;

    [SerializeField] private GameObject keyIcon;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;

    private bool playerInside;
    private bool pickedUp;

    private void Start()
    {
        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    private void Update()
    {
        if (pickedUp ||
            !playerInside ||
            !requireKey)
        {
            return;
        }

        if (Input.GetKeyDown(pickupKey))
            TryPickup();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (keyIcon != null)
            keyIcon.SetActive(true);

        if (!requireKey)
            TryPickup();
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    private void TryPickup()
    {
        if (pickedUp)
            return;

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

        if (!InventoryManager.Instance.CanAddItem(
        itemData,
        quantity))
        {
            Debug.Log(
                "Inventory không đủ chỗ."
            );

            return;
        }
        bool added =
            InventoryManager.Instance.AddItem(
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

        if (keyIcon != null)
            keyIcon.SetActive(false);

        if (AudioManager.Instance != null &&
     itemData.pickupSound != null)
        {
            AudioManager.Instance.PlaySFX(
                itemData.pickupSound,
                itemData.pickupVolume
            );
        }

        Debug.Log(
            $"Đã nhặt {itemData.ItemName} x{quantity}"
        );

        Destroy(gameObject);
    }

    private void OnValidate()
    {
        quantity =
            Mathf.Max(
                1,
                quantity
            );
    }
}