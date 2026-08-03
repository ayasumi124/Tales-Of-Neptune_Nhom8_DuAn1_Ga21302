using System;
using System.Collections.Generic;

[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData>
        items =
            new List<InventoryItemSaveData>();

    public string shortcutItemID;
}

[Serializable]
public class InventoryItemSaveData
{
    public string itemID;
    public int quantity;

    public InventoryItemSaveData(
        string id,
        int amount)
    {
        itemID = id;
        quantity = amount;
    }
}