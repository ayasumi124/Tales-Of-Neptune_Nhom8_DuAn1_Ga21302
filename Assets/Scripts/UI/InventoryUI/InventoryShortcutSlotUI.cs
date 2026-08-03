using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryShortcutSlotUI :
    MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image equippedItemIcon;

    [SerializeField]
    private GameObject placeholderIcon;

    [SerializeField]
    private GameObject keyIcon;

    [SerializeField]
    private TextMeshProUGUI keyText;

    private void OnEnable()
    {
        ItemShortcutManager
            .OnShortcutChanged += Refresh;

        InventoryManager
            .OnInventoryChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        ItemShortcutManager
            .OnShortcutChanged -= Refresh;

        InventoryManager
            .OnInventoryChanged -= Refresh;
    }

    public void Refresh()
    {
        ItemData equippedItem = null;

        if (ItemShortcutManager.Instance != null)
        {
            equippedItem =
                ItemShortcutManager.Instance
                    .EquippedItem;
        }

        bool hasEquippedItem =
            equippedItem != null;

        if (equippedItemIcon != null)
        {
            equippedItemIcon.sprite =
                hasEquippedItem
                    ? equippedItem.Icon
                    : null;

            equippedItemIcon.enabled =
                hasEquippedItem &&
                equippedItem.Icon != null;
        }

        if (placeholderIcon != null)
        {
            placeholderIcon.SetActive(
                !hasEquippedItem
            );
        }

        if (keyIcon != null)
        {
            keyIcon.SetActive(
                hasEquippedItem
            );
        }

        if (keyText != null &&
            ItemShortcutManager.Instance != null)
        {
            keyText.text =
                ItemShortcutManager.Instance
                    .ShortcutKey
                    .ToString();
        }
    }
}