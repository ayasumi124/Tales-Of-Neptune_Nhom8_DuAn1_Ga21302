using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShortcutItemUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField]
    private GameObject shortcutPanel;

    [Header("UI")]
    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TextMeshProUGUI quantityText;

    [SerializeField]
    private TextMeshProUGUI keyText;

    private void OnEnable()
    {
        ItemShortcutManager
            .OnShortcutChanged += Refresh;

        InventoryManager
            .OnInventoryChanged += Refresh;
    }

    private void Start()
    {
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
        if (ItemShortcutManager.Instance == null)
        {
            HidePanel();
            return;
        }

        ItemData item =
            ItemShortcutManager.Instance
                .EquippedItem;

        if (item == null)
        {
            HidePanel();
            return;
        }

        int quantity = 0;

        if (InventoryManager.Instance != null)
        {
            quantity =
                InventoryManager.Instance
                    .GetQuantity(item);
        }

        /*
         * Hết item thì ẩn HUD.
         * Item vẫn đang được ghi nhớ trong Manager,
         * nhặt thêm item thì HUD tự hiện lại.
         */
        if (quantity <= 0)
        {
            ItemShortcutManager.Instance.ClearShortcut();
            HidePanel();
            return;
        }

        if (shortcutPanel != null)
            shortcutPanel.SetActive(true);

        if (itemIcon != null)
        {
            itemIcon.sprite =
                item.Icon;

            itemIcon.enabled =
                item.Icon != null;
        }

        if (quantityText != null)
        {
            quantityText.text =
                quantity.ToString();
        }

        if (keyText != null)
        {
            keyText.text =
                ItemShortcutManager.Instance
                    .ShortcutKey
                    .ToString();
        }
    }

    private void HidePanel()
    {
        if (shortcutPanel != null)
        {
            shortcutPanel.SetActive(false);
        }
    }
}