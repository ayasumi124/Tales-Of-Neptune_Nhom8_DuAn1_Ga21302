using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private GameObject selectedFrame;

    private InventoryUI inventoryUI;
    private int slotIndex = -1;

    public int SlotIndex => slotIndex;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
        }

        Clear();
    }

    public void Initialize(
        InventoryUI owner,
        int index)
    {
        inventoryUI = owner;
        slotIndex = index;
    }

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

        if (itemIcon != null)
        {
            itemIcon.sprite = data.Icon;
            itemIcon.enabled = data.Icon != null;

            Color color = itemIcon.color;
            color.a = 1f;
            itemIcon.color = color;
        }

        if (quantityText != null)
        {
            bool showQuantity =
                inventoryItem.quantity > 1;

            quantityText.gameObject.SetActive(
                showQuantity
            );

            quantityText.text =
                showQuantity
                    ? inventoryItem.quantity.ToString()
                    : "";
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(
                selected
            );
        }
    }

    public void Clear()
    {
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.gameObject.SetActive(false);
        }

        SetSelected(false);
    }

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