using UnityEngine;

[CreateAssetMenu(
    fileName = "Shop Item",
    menuName = "Shop/Shop Item"
)]
public class ShopItemData : ScriptableObject
{
    [Header("Item")]
    [SerializeField]
    private ItemData itemData;

    [Header("Shop")]
    [Min(0)]
    [SerializeField]
    private int price = 10;

    [Min(1)]
    [SerializeField]
    private int amount = 1;

    public ItemData ItemData =>
        itemData;

    public int Price =>
        price;

    public int Amount =>
        amount;
}