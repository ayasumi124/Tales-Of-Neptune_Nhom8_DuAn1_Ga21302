using System;

[Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;

    // Unity cần constructor rỗng để serialize ổn định.
    public InventoryItem()
    {
        itemData = null;
        quantity = 0;
    }

    public InventoryItem(
        ItemData data,
        int amount)
    {
        itemData = data;
        quantity = Math.Max(0, amount);
    }

    public bool IsEmpty()
    {
        return itemData == null ||
               quantity <= 0;
    }

    public bool CanStackWith(
        ItemData data)
    {
        if (itemData == null ||
            data == null)
        {
            return false;
        }

        return itemData == data &&
               itemData.Stackable &&
               quantity < itemData.MaxStack;
    }

    public int RemainingSpace()
    {
        if (itemData == null)
            return 0;

        if (!itemData.Stackable)
        {
            return quantity <= 0
                ? 1
                : 0;
        }

        return Math.Max(
            0,
            itemData.MaxStack - quantity
        );
    }

    public int AddAmount(
        int amount)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return amount;
        }

        int addableAmount =
            Math.Min(
                amount,
                RemainingSpace()
            );

        quantity += addableAmount;

        // Trả về lượng còn dư chưa thêm được.
        return amount - addableAmount;
    }

    public bool RemoveAmount(
        int amount)
    {
        if (amount <= 0 ||
            quantity < amount)
        {
            return false;
        }

        quantity -= amount;

        if (quantity <= 0)
        {
            Clear();
        }

        return true;
    }

    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }
}