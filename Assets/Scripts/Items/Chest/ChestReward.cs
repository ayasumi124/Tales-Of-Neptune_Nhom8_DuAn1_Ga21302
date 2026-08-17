using UnityEngine;

public class ChestReward : MonoBehaviour
{
    public enum RewardType
    {
        Coin,
        Item,
        DungeonKey
    }

    [Header("Reward Type")]
    [SerializeField]
    private RewardType rewardType;

    [Header("Coin Reward")]
    [SerializeField]
    private Sprite coinIcon;

    [Min(1)]
    [SerializeField]
    private int coinAmount = 100;

    [Header("Dungeon Key Reward")]
    [SerializeField]
    private Sprite dungeonKeyIcon;

    [Min(1)]
    [SerializeField]
    private int dungeonKeyAmount = 1;


    [Header("Item Reward")]
    [SerializeField]
    private ItemData itemData;

    [Min(1)]
    [SerializeField]
    private int itemQuantity = 1;

    private bool rewardClaimed;

    public bool RewardClaimed =>
        rewardClaimed;

    public bool ClaimReward()
    {
        if (rewardClaimed)
        {
            Debug.LogWarning(
                $"{name}: phần thưởng đã được nhận."
            );

            return false;
        }

        bool success = false;

        switch (rewardType)
        {
            case RewardType.Coin:
                success =
                    ClaimCoinReward();
                break;

            case RewardType.Item:
                success =
                    ClaimItemReward();
                break;

            case RewardType.DungeonKey:
                success =
                    ClaimDungeonKey();
                break;
        }

        if (success)
        {
            rewardClaimed = true;
        }

        return success;
    }

    private bool ClaimCoinReward()
    {
        int amount =
            Mathf.Max(
                1,
                coinAmount
            );

        if (CoinUI.Instance == null)
        {
            Debug.LogError(
                $"{name}: không tìm thấy CoinUI."
            );

            return false;
        }

        CoinUI.Instance.AddCoin(
            amount
        );

        if (RewardPopupUI.Instance != null)
        {
            RewardPopupUI.Instance.ShowCoin(
                coinIcon,
                amount
            );
        }

        Debug.Log(
            $"Nhận Coin x{amount}."
        );

        return true;
    }

    private bool ClaimItemReward()
    {
        if (itemData == null)
        {
            Debug.LogError(
                $"{name}: chưa gán ItemData."
            );

            return false;
        }

        int quantity;

        if (itemData.Equippable)
        {
            quantity = 1;
        }
        else
        {
            quantity =
                Mathf.Max(
                    1,
                    itemQuantity
                );
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                $"{name}: không tìm thấy InventoryManager."
            );

            return false;
        }

        if (!InventoryManager.Instance.CanAddItem(
                itemData,
                quantity))
        {
            Debug.LogWarning(
                $"Inventory không đủ chỗ cho " +
                $"{itemData.ItemName} x{quantity}."
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
            $"Nhận {itemData.ItemName} x{quantity}."
        );

        return true;
    }

    private bool ClaimDungeonKey()
    {
        if (DungeonKeyManager.Instance == null)
        {
            Debug.LogError(
                $"{name}: không tìm thấy DungeonKeyManager."
            );

            return false;
        }

        int amount =
            Mathf.Max(
                1,
                dungeonKeyAmount
            );

        DungeonKeyManager.Instance.GiveKey(
            amount
        );

        if (RewardPopupUI.Instance != null)
        {
            RewardPopupUI.Instance
                .ShowDungeonKey(
                    dungeonKeyIcon,
                    amount
                );
        }
        else
        {
            Debug.LogWarning(
                "Không tìm thấy RewardPopupUI."
            );
        }

        Debug.Log(
            $"Nhận Dungeon Key x{amount}."
        );

        return true;
    }

    public void RestoreClaimedState()
    {
        rewardClaimed = true;
    }

    public void ResetReward()
    {
        rewardClaimed = false;
    }

    private void OnValidate()
    {
        coinAmount =
            Mathf.Max(
                1,
                coinAmount
            );

        itemQuantity =
            Mathf.Max(
                1,
                itemQuantity
            );

        dungeonKeyAmount =
            Mathf.Max(
                1,
                dungeonKeyAmount
            );
    }
}