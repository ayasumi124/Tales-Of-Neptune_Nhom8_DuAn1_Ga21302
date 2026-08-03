using UnityEngine;

public class ChestReward : MonoBehaviour
{
    public enum RewardType
    {
        Coin,
        Item
    }

    [Header("Reward Type")]
    [SerializeField] private RewardType rewardType;

    [Header("Coin Reward")]
    [SerializeField] private int coinAmount = 100;
    [SerializeField] private Sprite coinIcon;

    [Header("Item Reward")]
    [SerializeField] private ItemData itemData;

    [Min(1)]
    [SerializeField] private int itemQuantity = 1;

    private bool rewardClaimed;

    public bool GiveReward()
    {
        if (rewardClaimed)
            return false;

        bool success = false;

        switch (rewardType)
        {
            case RewardType.Coin:
                success =
                    GiveCoinReward();
                break;

            case RewardType.Item:
                success =
                    GiveItemReward();
                break;
        }

        if (success)
        {
            rewardClaimed = true;
        }

        return success;
    }

    private bool GiveCoinReward()
    {
        if (coinAmount <= 0)
            return false;

        if (CoinUI.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy CoinUI."
            );

            return false;
        }

        CoinUI.Instance.AddCoin(
            coinAmount
        );

        if (RewardPopupUI.Instance != null)
        {
            RewardPopupUI.Instance.ShowCoin(
                coinIcon,
                coinAmount
            );
        }

        Debug.Log(
            $"Nhận {coinAmount} Coin."
        );

        return true;
    }

    private bool GiveItemReward()
    {
        if (itemData == null)
        {
            Debug.LogError(
                $"{name}: chưa gán ItemData."
            );

            return false;
        }

        int quantity =
            Mathf.Max(
                1,
                itemQuantity
            );

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy InventoryManager."
            );

            return false;
        }

        if (!InventoryManager.Instance.CanAddItem(
                itemData,
                quantity))
        {
            Debug.Log(
                "Inventory không đủ chỗ."
            );

            return false;
        }

        bool added =
            InventoryManager.Instance.AddItem(
                itemData,
                quantity
            );

        if (!added)
            return false;

        if (RewardPopupUI.Instance != null)
        {
            RewardPopupUI.Instance.ShowItem(
                itemData,
                quantity
            );
        }

        Debug.Log(
            $"Nhận {itemData.ItemName} " +
            $"x{quantity}."
        );

        return true;
    }
}