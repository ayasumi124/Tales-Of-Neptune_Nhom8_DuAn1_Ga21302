using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    [Header("Data")]
    [SerializeField]
    private ShopItemData shopItem;

    [Header("UI")]
    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI itemNameText;

    [SerializeField]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    private TextMeshProUGUI priceText;

    [SerializeField]
    private TextMeshProUGUI amountText;

    [SerializeField]
    private Button buyButton;

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                Buy
            );

            buyButton.onClick.AddListener(
                Buy
            );
        }
    }

    private void Start()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (shopItem == null ||
            shopItem.ItemData == null)
        {
            ClearUI();
            return;
        }

        ItemData itemData =
            shopItem.ItemData;

        if (icon != null)
        {
            icon.sprite =
                itemData.Icon;

            icon.enabled =
                itemData.Icon != null;
        }

        if (itemNameText != null)
        {
            itemNameText.text =
                itemData.ItemName;
        }

        if (descriptionText != null)
        {
            descriptionText.text =
                itemData.Description;
        }

        if (priceText != null)
        {
            priceText.text =
                $"{shopItem.Price} Coin";
        }

        if (amountText != null)
        {
            amountText.text =
                shopItem.Amount > 1
                    ? $"x{shopItem.Amount}"
                    : "";
        }

        if (buyButton != null)
        {
            buyButton.interactable =
                true;
        }
    }

    public void Buy()
    {
        if (ShopManager.Instance == null)
            return;

        if (shopItem == null)
            return;

        ShopManager.Instance.BuyItem(
            shopItem
        );
    }

    private void ClearUI()
    {
        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (itemNameText != null)
            itemNameText.text = "";

        if (descriptionText != null)
            descriptionText.text = "";

        if (priceText != null)
            priceText.text = "";

        if (amountText != null)
            amountText.text = "";

        if (buyButton != null)
            buyButton.interactable = false;
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(
                Buy
            );
        }
    }
}