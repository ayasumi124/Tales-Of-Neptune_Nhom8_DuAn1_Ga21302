using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]
    [SerializeField]
    private Button button;

    [SerializeField]
    private Image itemIcon;

    [SerializeField]
    private TextMeshProUGUI quantityText;

    [SerializeField]
    private GameObject selectedFrame;

    [Header("Equipment")]
    [Tooltip(
        "Chữ E hiện khi item đang được trang bị."
    )]
    [SerializeField]
    private TextMeshProUGUI equippedText;

    // =====================================================
    // RUNTIME
    // =====================================================

    private InventoryUI inventoryUI;

    private int slotIndex = -1;

    public int SlotIndex =>
        slotIndex;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(
                OnClick
            );

            button.onClick.AddListener(
                OnClick
            );
        }

        Clear();
    }

    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        InventoryUI owner,
        int index)
    {
        inventoryUI =
            owner;

        slotIndex =
            index;
    }

    // =====================================================
    // DISPLAY
    // =====================================================

    public void Display(
        InventoryItem inventoryItem)
    {
        if (inventoryItem == null ||
            inventoryItem.IsEmpty())
        {
            Clear();
            return;
        }

        ItemData data =
            inventoryItem.itemData;

        if (data == null)
        {
            Clear();
            return;
        }

        // =================================================
        // ITEM ICON
        // =================================================

        if (itemIcon != null)
        {
            itemIcon.sprite =
                data.Icon;

            itemIcon.enabled =
                data.Icon != null;

            Color color =
                itemIcon.color;

            color.a = 1f;

            itemIcon.color =
                color;
        }

        // =================================================
        // QUANTITY
        // =================================================

        if (quantityText != null)
        {
            bool showQuantity =
                inventoryItem.quantity > 1;

            quantityText.gameObject
                .SetActive(
                    showQuantity
                );

            quantityText.text =
                showQuantity
                    ? inventoryItem
                        .quantity
                        .ToString()
                    : "";
        }

        // =================================================
        // EQUIPPED "E"
        // =================================================

        RefreshEquippedState(
            data
        );
    }

    // =====================================================
    // EQUIPMENT STATE
    // =====================================================

    private void RefreshEquippedState(
        ItemData data)
    {
        if (equippedText == null)
            return;

        bool equipped =
            false;

        /*
         * Chỉ Equipment mới có thể
         * hiện chữ E.
         */
        if (data != null &&
            data.Equippable &&
            PlayerEquipmentManager.Instance != null)
        {
            equipped =
                PlayerEquipmentManager.Instance
                    .IsEquipped(
                        data
                    );
        }

        equippedText.gameObject
            .SetActive(
                equipped
            );

        equippedText.text =
            equipped
                ? "E"
                : "";
    }

    // =====================================================
    // SELECT
    // =====================================================

    public void SetSelected(
        bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(
                selected
            );
        }
    }

    // =====================================================
    // CLEAR
    // =====================================================

    public void Clear()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite =
                null;

            itemIcon.enabled =
                false;
        }

        if (quantityText != null)
        {
            quantityText.text =
                "";

            quantityText.gameObject
                .SetActive(
                    false
                );
        }

        if (equippedText != null)
        {
            equippedText.text =
                "";

            equippedText.gameObject
                .SetActive(
                    false
                );
        }

        SetSelected(
            false
        );
    }

    // =====================================================
    // CLICK
    // =====================================================

    private void OnClick()
    {
        if (inventoryUI == null ||
            slotIndex < 0)
        {
            return;
        }

        inventoryUI.SelectSlot(
            slotIndex
        );
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(
                OnClick
            );
        }
    }
}