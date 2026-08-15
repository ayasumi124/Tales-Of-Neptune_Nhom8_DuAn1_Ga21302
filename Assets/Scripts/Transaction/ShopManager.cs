using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance
    {
        get;
        private set;
    }

    [Header("UI")]
    [SerializeField]
    private GameObject shopPanel;

    [SerializeField]
    private TextMeshProUGUI coinText;

    [SerializeField]
    private ShopNotificationUI notificationUI;

    private bool shopOpen;

    private Players player;

    public bool IsShopOpen =>
        shopOpen;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        FindPlayer();

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }

        UpdateCoinUI();
    }

    private void Update()
    {
        if (!shopOpen)
            return;

        if (Input.GetKeyDown(
                KeyCode.Escape))
        {
            CloseShop();
        }
    }

    // =====================================================
    // OPEN / CLOSE
    // =====================================================

    public void OpenShop()
    {
        if (shopOpen)
            return;

        shopOpen = true;

        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }

        FindPlayer();

        if (player != null)
        {
            player.LockControl();
        }

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }

        UpdateCoinUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayShopOpen();
        }
    }

    public void CloseShop()
    {
        if (!shopOpen)
            return;

        shopOpen = false;

        if (notificationUI != null)
        {
            notificationUI.HideImmediate();
        }

        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        FindPlayer();

        if (player != null)
        {
            player.UnlockControl();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayShopClose();
        }
    }

    // =====================================================
    // BUY
    // =====================================================

    public bool BuyItem(
        ShopItemData shopItem)
    {
        if (shopItem == null ||
            shopItem.ItemData == null)
        {
            Debug.LogWarning(
                "Shop Item chưa được gán ItemData."
            );

            return false;
        }

        if (CoinUI.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy CoinUI."
            );

            return false;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy InventoryManager."
            );

            return false;
        }

        ItemData itemData =
            shopItem.ItemData;

        int amount =
            Mathf.Max(
                1,
                shopItem.Amount
            );

        int price =
            Mathf.Max(
                0,
                shopItem.Price
            );

        // =================================================
        // INVENTORY FULL
        // =================================================

        if (!InventoryManager.Instance
                .CanAddItem(
                    itemData,
                    amount))
        {
            if (notificationUI != null)
            {
                notificationUI
                    .ShowInventoryFull();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance
                    .PlayShopError();
            }

            Debug.Log(
                $"Inventory đầy. " +
                $"Không thể mua " +
                $"{itemData.ItemName}."
            );

            return false;
        }

        // =================================================
        // NOT ENOUGH COIN
        // =================================================

        if (!CoinUI.Instance
                .HasEnoughCoin(
                    price))
        {
            if (notificationUI != null)
            {
                notificationUI
                    .ShowNotEnoughCoin();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance
                    .PlayShopError();
            }

            Debug.Log(
                $"Không đủ Coin. " +
                $"Cần {price}, " +
                $"hiện có " +
                $"{CoinUI.Instance.Coin}."
            );

            return false;
        }

        // =================================================
        // SPEND COIN
        // =================================================

        if (!CoinUI.Instance
                .SpendCoin(
                    price))
        {
            if (notificationUI != null)
            {
                notificationUI
                    .ShowNotEnoughCoin();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance
                    .PlayShopError();
            }

            return false;
        }

        // =================================================
        // ADD ITEM
        // =================================================

        bool added =
            InventoryManager.Instance
                .AddItem(
                    itemData,
                    amount
                );

        if (!added)
        {
            // Nếu xảy ra lỗi bất thường
            // sau khi đã trừ coin thì hoàn tiền.
            CoinUI.Instance.AddCoin(
                price
            );

            if (notificationUI != null)
            {
                notificationUI
                    .ShowInventoryFull();
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance
                    .PlayShopError();
            }

            UpdateCoinUI();

            Debug.LogError(
                $"Không thêm được " +
                $"{itemData.ItemName}. " +
                $"Đã hoàn lại " +
                $"{price} Coin."
            );

            return false;
        }

        // =================================================
        // SUCCESS
        // =================================================

        if (notificationUI != null)
        {
            notificationUI.ShowBought(
                itemData.ItemName
            );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayShopBuy();
        }

        UpdateCoinUI();

        Debug.Log(
            $"Đã mua " +
            $"{itemData.ItemName} " +
            $"x{amount}. " +
            $"Giá: {price} Coin."
        );

        return true;
    }

    // =====================================================
    // COIN UI
    // =====================================================

    public void UpdateCoinUI()
    {
        if (coinText == null)
            return;

        if (CoinUI.Instance == null)
        {
            coinText.text = "0";
            return;
        }

        coinText.text =
            CoinUI.Instance
                .Coin
                .ToString();
    }

    // =====================================================
    // PLAYER
    // =====================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player =
                GameManager.Instance.Player
                    .GetComponent<Players>();
        }

        if (player == null)
        {
            player =
                FindFirstObjectByType<Players>();
        }
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        if (shopOpen &&
            player != null)
        {
            player.UnlockControl();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}