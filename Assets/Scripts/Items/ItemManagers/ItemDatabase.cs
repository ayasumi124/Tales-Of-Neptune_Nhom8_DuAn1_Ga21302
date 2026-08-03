using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Item Database",
    menuName = "Inventory/Item Database"
)]
public class ItemDatabase : ScriptableObject
{
    [SerializeField]
    private List<ItemData> items =
        new List<ItemData>();

    private Dictionary<string, ItemData>
        itemLookup;

    public ItemData GetItemByID(
        string itemID)
    {
        if (string.IsNullOrWhiteSpace(itemID))
            return null;

        BuildLookup();

        itemLookup.TryGetValue(
            itemID,
            out ItemData result
        );

        return result;
    }

    private void BuildLookup()
    {
        if (itemLookup != null)
            return;

        itemLookup =
            new Dictionary<string, ItemData>();

        foreach (ItemData item in items)
        {
            if (item == null ||
                string.IsNullOrWhiteSpace(
                    item.ItemID))
            {
                continue;
            }

            if (itemLookup.ContainsKey(
                    item.ItemID))
            {
                Debug.LogError(
                    $"Item ID bị trùng: {item.ItemID}"
                );

                continue;
            }

            itemLookup.Add(
                item.ItemID,
                item
            );
        }
    }

    private void OnValidate()
    {
        itemLookup = null;
    }
}