using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance
    {
        get;
        private set;
    }

    [Header("Inventory")]
    [Min(1)]
    [SerializeField]
    private int inventorySize = 30;

    [SerializeField]
    private List<InventoryItem> items =
        new List<InventoryItem>();

    [Header("Player")]
    [SerializeField]
    private PlayerItemUser playerItemUser;

    [Header("Drop")]
    [Tooltip(
        "Prefab chung có WorldItemPickup."
    )]
    [SerializeField]
    private WorldItemPickup worldItemPrefab;

    [SerializeField]
    private float dropDistance = 0.8f;

    [Header("Save")]
    [SerializeField]
    private ItemDatabase itemDatabase;

    [SerializeField]
    private bool loadInventoryOnStart;

    [SerializeField]
    private bool saveOnApplicationQuit = true;

    [Header("Test")]
    [SerializeField]
    private bool addTestItemsOnStart;

    [SerializeField]
    private ItemData testHealthPotion;

    [SerializeField]
    private ItemData testManaPotion;

    [SerializeField]
    private ItemData testHeartContainer;

    [SerializeField]
    private ItemData testWeapon;

    [SerializeField]
    private ItemData testHelmet;

    [SerializeField]
    private ItemData testArmor;
    [Header("Test Input")]
    [SerializeField] private bool enableTestInput = true;

    [SerializeField]
    private KeyCode addTestItemKey =
        KeyCode.R;

    [SerializeField] private int testAddAmount = 10;

    public static event Action OnInventoryChanged;

    public IReadOnlyList<InventoryItem> Items =>
        items;

    public int InventorySize =>
        inventorySize;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        InitializeInventory();
    }

    private void Start()
    {
        FindPlayerItemUser();

        if (loadInventoryOnStart)
        {
            LoadInventory();
        }
        else if (addTestItemsOnStart)
        {
            AddItem(testHealthPotion, 5);
            AddItem(testManaPotion, 5);
            AddItem(testHeartContainer, 1);

            AddItem(testWeapon, 1);
            AddItem(testHelmet, 1);
            AddItem(testArmor, 1);
        }
    }
    private void Update()
    {
        if (!enableTestInput)
            return;

        if (!Input.GetKeyDown(addTestItemKey))
            return;

        AddTestItems();
    }

    private void AddTestItems()
    {
        int amount =
            Mathf.Max(
                1,
                testAddAmount
            );

        if (testHealthPotion != null)
        {
            AddItem(
                testHealthPotion,
                amount
            );
        }

        if (testManaPotion != null)
        {
            AddItem(
                testManaPotion,
                amount
            );
        }

        if (testHeartContainer != null)
        {
            AddItem(
                testHeartContainer,
                amount
            );
        }

        Debug.Log(
            $"Đã thêm item test x{amount}."
        );
    }

    private void InitializeInventory()
    {
        inventorySize =
            Mathf.Max(
                1,
                inventorySize
            );

        if (items == null)
        {
            items =
                new List<InventoryItem>();
        }

        while (items.Count <
               inventorySize)
        {
            items.Add(
                new InventoryItem()
            );
        }

        while (items.Count >
               inventorySize)
        {
            items.RemoveAt(
                items.Count - 1
            );
        }

        for (int i = 0;
             i < items.Count;
             i++)
        {
            if (items[i] == null)
            {
                items[i] =
                    new InventoryItem();
            }
        }
    }

    // =====================================================
    // ADD
    // =====================================================

    public bool CanAddItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return false;
        }

        int availableSpace = 0;

        foreach (InventoryItem slot in items)
        {
            if (slot == null ||
                slot.IsEmpty())
            {
                availableSpace +=
                    itemData.Stackable
                        ? itemData.MaxStack
                        : 1;
            }
            else if (slot.CanStackWith(
                         itemData))
            {
                availableSpace +=
                    slot.RemainingSpace();
            }

            if (availableSpace >= amount)
                return true;
        }

        return false;
    }

    public bool AddItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return false;
        }

        /*
         * Ngăn tình trạng thêm được một phần
         * rồi báo Inventory đầy.
         */
        if (!CanAddItem(
                itemData,
                amount))
        {
            Debug.Log(
                $"Không đủ chỗ cho " +
                $"{itemData.ItemName} x{amount}."
            );

            return false;
        }

        AddItemInternal(
            itemData,
            amount
        );

        OnInventoryChanged?.Invoke();

        Debug.Log(
            $"Đã thêm {itemData.ItemName} " +
            $"x{amount}."
        );

        return true;
    }

    private void AddItemInternal(
        ItemData itemData,
        int amount)
    {
        int remainingAmount =
            amount;

        if (itemData.Stackable)
        {
            foreach (InventoryItem slot
                     in items)
            {
                if (!slot.CanStackWith(
                        itemData))
                {
                    continue;
                }

                remainingAmount =
                    slot.AddAmount(
                        remainingAmount
                    );

                if (remainingAmount <= 0)
                    return;
            }
        }

        while (remainingAmount > 0)
        {
            int emptyIndex =
                FindEmptySlotIndex();

            if (emptyIndex < 0)
                return;

            int amountForSlot =
                itemData.Stackable
                    ? Mathf.Min(
                        remainingAmount,
                        itemData.MaxStack
                    )
                    : 1;

            items[emptyIndex] =
                new InventoryItem(
                    itemData,
                    amountForSlot
                );

            remainingAmount -=
                amountForSlot;
        }
    }

    // =====================================================
    // REMOVE
    // =====================================================

    public bool RemoveItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null ||
            amount <= 0 ||
            GetQuantity(itemData) < amount)
        {
            return false;
        }

        int remainingAmount =
            amount;

        for (int i = items.Count - 1;
             i >= 0;
             i--)
        {
            InventoryItem slot =
                items[i];

            if (slot == null ||
                slot.IsEmpty() ||
                slot.itemData != itemData)
            {
                continue;
            }

            int removeAmount =
                Mathf.Min(
                    remainingAmount,
                    slot.quantity
                );

            slot.RemoveAmount(
                removeAmount
            );

            remainingAmount -=
                removeAmount;

            if (remainingAmount <= 0)
                break;
        }

        OnInventoryChanged?.Invoke();

        NotifyShortcutIfNeeded(
            itemData
        );

        return true;
    }

    public bool RemoveItemAt(
        int slotIndex,
        int amount)
    {
        InventoryItem slot =
            GetItemAt(slotIndex);

        if (slot == null ||
            slot.IsEmpty() ||
            amount <= 0 ||
            slot.quantity < amount)
        {
            return false;
        }

        ItemData removedData =
            slot.itemData;

        slot.RemoveAmount(amount);

        OnInventoryChanged?.Invoke();

        NotifyShortcutIfNeeded(
            removedData
        );

        return true;
    }

    // =====================================================
    // DROP
    // =====================================================

    public bool DropItemAt(
        int slotIndex,
        int amount = 1)
    {
        InventoryItem slot =
            GetItemAt(slotIndex);

        if (slot == null ||
            slot.IsEmpty())
        {
            return false;
        }

        if (worldItemPrefab == null)
        {
            Debug.LogError(
                "InventoryManager chưa gán World Item Prefab."
            );

            return false;
        }

        int dropAmount =
            Mathf.Clamp(
                amount,
                1,
                slot.quantity
            );

        ItemData droppedItem =
            slot.itemData;

        Transform playerTransform =
            FindPlayerTransform();

        if (playerTransform == null)
        {
            Debug.LogError(
                "Không tìm thấy Player để Drop item."
            );

            return false;
        }

        Vector2 direction =
            Vector2.down;

        Players players =
            playerTransform.GetComponent<Players>();

        if (players != null &&
            players.LastDirection.sqrMagnitude >
            0.001f)
        {
            direction =
                players.LastDirection.normalized;
        }

        Vector3 spawnPosition =
            playerTransform.position +
            (Vector3)(
                direction *
                Mathf.Max(
                    0.2f,
                    dropDistance
                )
            );

        WorldItemPickup worldItem =
            Instantiate(
                worldItemPrefab,
                spawnPosition,
                Quaternion.identity
            );

        worldItem.Setup(
            droppedItem,
            dropAmount
        );

        if (!RemoveItemAt(
                slotIndex,
                dropAmount))
        {
            Destroy(
                worldItem.gameObject
            );

            return false;
        }

        Debug.Log(
            $"Đã vứt {droppedItem.ItemName} " +
            $"x{dropAmount}."
        );

        return true;
    }

    public bool DropEntireStack(
        int slotIndex)
    {
        InventoryItem slot =
            GetItemAt(slotIndex);

        if (slot == null ||
            slot.IsEmpty())
        {
            return false;
        }

        return DropItemAt(
            slotIndex,
            slot.quantity
        );
    }

    // =====================================================
    // SORT
    // =====================================================

    public void SortInventory()
    {
        /*
         * Gộp tất cả quantity theo ItemData trước.
         */
        Dictionary<ItemData, int>
            totals =
                new Dictionary<ItemData, int>();

        foreach (InventoryItem slot
                 in items)
        {
            if (slot == null ||
                slot.IsEmpty())
            {
                continue;
            }

            if (!totals.ContainsKey(
                    slot.itemData))
            {
                totals.Add(
                    slot.itemData,
                    0
                );
            }

            totals[slot.itemData] +=
                slot.quantity;
        }

        List<ItemData> sortedItems =
            new List<ItemData>(
                totals.Keys
            );

        sortedItems.Sort(
            CompareItems
        );

        foreach (InventoryItem slot
                 in items)
        {
            slot.Clear();
        }

        foreach (ItemData data
                 in sortedItems)
        {
            AddItemInternal(
                data,
                totals[data]
            );
        }

        OnInventoryChanged?.Invoke();

        Debug.Log(
            "Đã sắp xếp Inventory."
        );
    }

    private int CompareItems(
        ItemData a,
        ItemData b)
    {
        int typeComparison =
            ((int)a.ItemType).CompareTo(
                (int)b.ItemType
            );

        if (typeComparison != 0)
            return typeComparison;

        return string.Compare(
            a.ItemName,
            b.ItemName,
            StringComparison.OrdinalIgnoreCase
        );
    }

    // =====================================================
    // USE
    // =====================================================

    public bool UseItemAt(
        int slotIndex)
    {
        InventoryItem slot =
            GetItemAt(slotIndex);

        if (slot == null ||
            slot.IsEmpty())
        {
            return false;
        }

        ItemData itemData =
            slot.itemData;

        if (!itemData.Usable)
            return false;

        FindPlayerItemUser();

        if (playerItemUser == null)
        {
            Debug.LogError(
                "Không tìm thấy PlayerItemUser."
            );

            return false;
        }

        bool accepted =
            playerItemUser.TryUse(
                itemData
            );

        if (!accepted)
            return false;

        slot.RemoveAmount(1);

        OnInventoryChanged?.Invoke();

        NotifyShortcutIfNeeded(
            itemData
        );

        return true;
    }

    public bool UseItem(
        ItemData itemData)
    {
        int slotIndex =
            FindItemSlotIndex(
                itemData
            );

        return slotIndex >= 0 &&
               UseItemAt(slotIndex);
    }

    // =====================================================
    // GET
    // =====================================================

    public bool HasItem(
        ItemData itemData,
        int amount = 1)
    {
        return itemData != null &&
               amount > 0 &&
               GetQuantity(itemData) >= amount;
    }

    public int GetQuantity(
        ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int total = 0;

        foreach (InventoryItem slot
                 in items)
        {
            if (slot != null &&
                !slot.IsEmpty() &&
                slot.itemData == itemData)
            {
                total += slot.quantity;
            }
        }

        return total;
    }

    public InventoryItem GetItemAt(
        int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= items.Count)
        {
            return null;
        }

        return items[slotIndex];
    }

    // =====================================================
    // SAVE / LOAD
    // =====================================================

    public bool SaveInventory()
    {
        InventorySaveData saveData =
            new InventorySaveData();

        foreach (InventoryItem slot
                 in items)
        {
            if (slot == null ||
                slot.IsEmpty() ||
                string.IsNullOrWhiteSpace(
                    slot.itemData.ItemID))
            {
                continue;
            }

            saveData.items.Add(
                new InventoryItemSaveData(
                    slot.itemData.ItemID,
                    slot.quantity
                )
            );
        }

        if (ItemShortcutManager.Instance != null &&
            ItemShortcutManager.Instance
                .EquippedItem != null)
        {
            saveData.shortcutItemID =
                ItemShortcutManager.Instance
                    .EquippedItem.ItemID;
        }

        return InventorySaveSystem.Save(
            saveData
        );
    }

    public bool LoadInventory()
    {
        if (itemDatabase == null)
        {
            Debug.LogError(
                "InventoryManager chưa gán Item Database."
            );

            return false;
        }

        InventorySaveData saveData =
            InventorySaveSystem.Load();

        if (saveData == null)
            return false;

        ClearInventory(false);

        foreach (InventoryItemSaveData entry
                 in saveData.items)
        {
            ItemData data =
                itemDatabase.GetItemByID(
                    entry.itemID
                );

            if (data == null)
            {
                Debug.LogWarning(
                    $"Không tìm thấy Item ID: " +
                    $"{entry.itemID}"
                );

                continue;
            }

            if (CanAddItem(
                    data,
                    entry.quantity))
            {
                AddItemInternal(
                    data,
                    entry.quantity
                );
            }
        }

        if (ItemShortcutManager.Instance != null)
        {
            ItemData shortcutItem =
                itemDatabase.GetItemByID(
                    saveData.shortcutItemID
                );

            ItemShortcutManager.Instance
                .RestoreShortcut(
                    shortcutItem
                );
        }

        OnInventoryChanged?.Invoke();

        Debug.Log(
            "Đã load Inventory."
        );

        return true;
    }

    public void DeleteInventorySave()
    {
        InventorySaveSystem.DeleteSave();
    }

    // =====================================================
    // CLEAR
    // =====================================================

    public void ClearInventory(
        bool notify = true)
    {
        foreach (InventoryItem slot
                 in items)
        {
            slot.Clear();
        }

        if (notify)
        {
            OnInventoryChanged?.Invoke();
        }
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private void NotifyShortcutIfNeeded(
        ItemData itemData)
    {
        if (itemData == null ||
            ItemShortcutManager.Instance == null)
        {
            return;
        }

        ItemShortcutManager
            .OnInventoryItemQuantityChanged();
    }

    private int FindEmptySlotIndex()
    {
        for (int i = 0;
             i < items.Count;
             i++)
        {
            if (items[i] == null ||
                items[i].IsEmpty())
            {
                return i;
            }
        }

        return -1;
    }

    private int FindItemSlotIndex(
        ItemData itemData)
    {
        if (itemData == null)
            return -1;

        for (int i = 0;
             i < items.Count;
             i++)
        {
            InventoryItem slot =
                items[i];

            if (slot != null &&
                !slot.IsEmpty() &&
                slot.itemData == itemData)
            {
                return i;
            }
        }

        return -1;
    }

    private Transform FindPlayerTransform()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            return GameManager.Instance.Player
                .transform;
        }

        Players players =
            FindFirstObjectByType<Players>();

        return players != null
            ? players.transform
            : null;
    }

    private void FindPlayerItemUser()
    {
        if (playerItemUser != null)
            return;

        Transform playerTransform =
            FindPlayerTransform();

        if (playerTransform != null)
        {
            playerItemUser =
                playerTransform.GetComponent<
                    PlayerItemUser
                >();
        }
    }

    private void OnValidate()
    {
        inventorySize =
            Mathf.Max(
                1,
                inventorySize
            );
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
        {
            SaveInventory();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}