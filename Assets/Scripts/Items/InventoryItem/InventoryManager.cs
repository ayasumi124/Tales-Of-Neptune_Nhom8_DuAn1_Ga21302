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
    [SerializeField] private int inventorySize = 30;

    [SerializeField]
    private List<InventoryItem> items =
        new List<InventoryItem>();

    [Header("Player")]
    [SerializeField]
    private PlayerItemUser playerItemUser;

    [Header("Test")]
    [SerializeField]
    private bool addTestItemsOnStart;

    [SerializeField]
    private ItemData testHealthPotion;

    [SerializeField]
    private ItemData testManaPotion;

    [SerializeField]
    private ItemData testHeartContainer;

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

        InitializeInventory();
    }

    private void Start()
    {
        FindPlayerItemUser();

        if (addTestItemsOnStart)
        {
            AddItem(
                testHealthPotion,
                5
            );

            AddItem(
                testManaPotion,
                3
            );

            AddItem(
                testHeartContainer,
                1
            );
        }
    }

    private void Update()
{
    if (Input.GetKeyDown(KeyCode.R))
    {
        AddItem(testHealthPotion, 10);
        AddItem(testManaPotion, 10);
        AddItem(testHeartContainer, 10);
    }
    // Dùng item ở ô đầu tiên.
    if (Input.GetKeyDown(KeyCode.Z))
    {
        UseItemAt(0);
    }

    // Dùng item ở ô thứ hai.
    if (Input.GetKeyDown(KeyCode.X))
    {
        UseItemAt(1);
    }

    // Dùng Heart ở ô thứ ba.
    if (Input.GetKeyDown(KeyCode.C))
    {
        UseItemAt(2);
    }
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

        /*
         * Thêm các ô trống cho đủ số lượng.
         */
        while (items.Count <
               inventorySize)
        {
            items.Add(
                new InventoryItem()
            );
        }

        /*
         * Nếu giảm Inventory Size trong Inspector,
         * xóa các ô dư phía cuối.
         */
        while (items.Count >
               inventorySize)
        {
            items.RemoveAt(
                items.Count - 1
            );
        }

        /*
         * Tránh phần tử null trong danh sách.
         */
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

    public bool AddItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null)
        {
            Debug.LogError(
                "Không thể thêm ItemData null."
            );

            return false;
        }

        if (amount <= 0)
            return false;

        int remainingAmount =
            amount;

        /*
         * Trước tiên tìm các stack cùng loại
         * chưa đạt giới hạn.
         */
        if (itemData.Stackable)
        {
            for (int i = 0;
                 i < items.Count;
                 i++)
            {
                InventoryItem slot =
                    items[i];

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
                    break;
            }
        }

        /*
         * Nếu vẫn còn item, tìm ô trống.
         */
        while (remainingAmount > 0)
        {
            int emptyIndex =
                FindEmptySlotIndex();

            if (emptyIndex < 0)
            {
                Debug.Log(
                    $"Inventory đã đầy. " +
                    $"Còn dư {remainingAmount} " +
                    $"{itemData.ItemName}."
                );

                OnInventoryChanged?.Invoke();

                return false;
            }

            int amountForNewSlot =
                itemData.Stackable
                    ? Mathf.Min(
                        remainingAmount,
                        itemData.MaxStack
                    )
                    : 1;

            items[emptyIndex] =
                new InventoryItem(
                    itemData,
                    amountForNewSlot
                );

            remainingAmount -=
                amountForNewSlot;
        }

        Debug.Log(
            $"Đã thêm {amount} " +
            $"{itemData.ItemName}."
        );

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool RemoveItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return false;
        }

        if (GetQuantity(itemData) <
            amount)
        {
            Debug.Log(
                $"Không đủ {itemData.ItemName}."
            );

            return false;
        }

        int remainingAmount =
            amount;

        /*
         * Trừ từ các stack phía cuối trước.
         */
        for (int i = items.Count - 1;
             i >= 0;
             i--)
        {
            InventoryItem slot =
                items[i];

            if (slot.IsEmpty() ||
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

        Debug.Log(
            $"Đã trừ {amount} " +
            $"{itemData.ItemName}."
        );

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool UseItemAt(
        int slotIndex)
    {
        if (slotIndex < 0 ||
            slotIndex >= items.Count)
        {
            return false;
        }

        InventoryItem slot =
            items[slotIndex];

        if (slot == null ||
            slot.IsEmpty())
        {
            return false;
        }

        ItemData itemData =
            slot.itemData;

        if (!itemData.Usable)
        {
            Debug.Log(
                $"{itemData.ItemName} không thể sử dụng."
            );

            return false;
        }

        FindPlayerItemUser();

        if (playerItemUser == null)
        {
            Debug.LogError(
                "Không tìm thấy PlayerItemUser."
            );

            return false;
        }

        /*
         * TryUse trả về true nghĩa là Player
         * đã chấp nhận bắt đầu sử dụng item.
         */
        bool accepted =
            playerItemUser.TryUse(
                itemData
            );

        if (!accepted)
            return false;

        slot.RemoveAmount(1);

        Debug.Log(
            $"Đã dùng {itemData.ItemName}."
        );

        OnInventoryChanged?.Invoke();

        return true;
    }

    public bool UseItem(
        ItemData itemData)
    {
        int slotIndex =
            FindItemSlotIndex(
                itemData
            );

        if (slotIndex < 0)
            return false;

        return UseItemAt(
            slotIndex
        );
    }

    public bool HasItem(
        ItemData itemData,
        int amount = 1)
    {
        if (itemData == null ||
            amount <= 0)
        {
            return false;
        }

        return GetQuantity(itemData) >=
               amount;
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
            if (slot == null ||
                slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemData ==
                itemData)
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

    public void ClearInventory()
    {
        for (int i = 0;
             i < items.Count;
             i++)
        {
            items[i].Clear();
        }

        OnInventoryChanged?.Invoke();
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

            if (slot == null ||
                slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemData ==
                itemData)
            {
                return i;
            }
        }

        return -1;
    }

    private void FindPlayerItemUser()
    {
        if (playerItemUser != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            playerItemUser =
                GameManager.Instance.Player
                    .GetComponent<
                        PlayerItemUser
                    >();
        }

        if (playerItemUser == null)
        {
            playerItemUser =
                FindFirstObjectByType<
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

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}