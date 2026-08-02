using System;
using UnityEngine;

public class ItemShortcutManager : MonoBehaviour
{
    public static ItemShortcutManager Instance
    {
        get;
        private set;
    }

    [Header("Shortcut")]
    [SerializeField]
    private ItemData equippedItem;

    [SerializeField]
    private KeyCode shortcutKey =
        KeyCode.Z;

    public ItemData EquippedItem =>
        equippedItem;

    public KeyCode ShortcutKey =>
        shortcutKey;

    public static event Action
        OnShortcutChanged;

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
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (Input.GetKeyDown(
                shortcutKey))
        {
            UseShortcut();
        }
    }

    public bool EquipShortcut(
        ItemData itemData)
    {
        if (itemData == null)
            return false;

        if (!itemData.Usable)
        {
            Debug.Log(
                $"{itemData.ItemName} không thể sử dụng."
            );

            return false;
        }

        if (itemData.ItemType !=
            ItemType.Consumable)
        {
            Debug.Log(
                "Chỉ item Consumable mới được gắn Shortcut."
            );

            return false;
        }

        equippedItem = itemData;

        Debug.Log(
            $"Đã gắn {itemData.ItemName} " +
            $"vào phím {shortcutKey}."
        );

        OnShortcutChanged?.Invoke();

        return true;
    }

    public void RemoveShortcut()
    {
        equippedItem = null;

        OnShortcutChanged?.Invoke();
    }

    public bool UseShortcut()
    {
        if (equippedItem == null)
        {
            Debug.Log(
                "Chưa có item trong Shortcut."
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

        if (!InventoryManager.Instance.HasItem(
                equippedItem,
                1))
        {
            Debug.Log(
                $"Đã hết {equippedItem.ItemName}."
            );

            OnShortcutChanged?.Invoke();

            return false;
        }

        bool used =
            InventoryManager.Instance.UseItem(
                equippedItem
            );

        if (used)
        {
            OnShortcutChanged?.Invoke();
        }

        return used;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}