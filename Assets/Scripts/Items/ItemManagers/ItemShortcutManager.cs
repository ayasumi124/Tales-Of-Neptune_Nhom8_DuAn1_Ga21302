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

    public static event Action OnShortcutChanged;

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
        if (itemData == null ||
            !itemData.Usable ||
            itemData.ItemType !=
            ItemType.Consumable)
        {
            return false;
        }

        equippedItem = itemData;

        OnShortcutChanged?.Invoke();

        return true;
    }

    public void ClearShortcut()
    {
        equippedItem = null;

        OnShortcutChanged?.Invoke();
    }

    public void RemoveShortcut()
    {
        ClearShortcut();
    }

    public void RestoreShortcut(
        ItemData itemData)
    {
        equippedItem = itemData;

        OnShortcutChanged?.Invoke();
    }

    public bool UseShortcut()
    {
        if (equippedItem == null ||
            InventoryManager.Instance == null)
        {
            return false;
        }

        if (!InventoryManager.Instance.HasItem(
                equippedItem,
                1))
        {
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

    public static void
        OnInventoryItemQuantityChanged()
    {
        OnShortcutChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}