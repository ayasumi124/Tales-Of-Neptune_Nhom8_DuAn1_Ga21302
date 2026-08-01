using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Inventory/Item Data"
)]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemID;

    [SerializeField] private string itemName;

    [TextArea(3, 6)]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;

    [Header("Classification")]
    [SerializeField] private ItemType itemType;

    [SerializeField] private ItemEffectType effectType;

    [Header("Stack")]
    [SerializeField] private bool stackable = true;

    [Min(1)]
    [SerializeField] private int maxStack = 99;

    [Header("Effect Value")]
    [Tooltip(
        "Ví dụ: hồi 20 HP hoặc tăng 2 Max HP."
    )]
    [SerializeField] private float effectValue = 10f;

    [Header("Use")]
    [Tooltip(
        "Vật phẩm có thể bấm Use trong Inventory hay không."
    )]
    [SerializeField] private bool usable = true;

    [Tooltip(
        "Thời gian animation sử dụng vật phẩm."
    )]
    [SerializeField] private float useDuration = 0.8f;

    [Tooltip(
        "Thời điểm áp dụng hiệu ứng trong animation."
    )]
    [SerializeField] private float effectDelay = 0.4f;

    [Header("Audio")]
    [SerializeField] private AudioClip useSound;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;

    public ItemType ItemType => itemType;
    public ItemEffectType EffectType => effectType;

    public bool Stackable => stackable;
    public int MaxStack => maxStack;

    public float EffectValue => effectValue;

    public bool Usable => usable;
    public float UseDuration => useDuration;
    public float EffectDelay => effectDelay;

    public AudioClip UseSound => useSound;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemID))
        {
            itemID =
                name.Trim()
                    .ToLower()
                    .Replace(" ", "_");
        }

        if (!stackable)
            maxStack = 1;

        maxStack =
            Mathf.Max(
                1,
                maxStack
            );

        useDuration =
            Mathf.Max(
                0.05f,
                useDuration
            );

        effectDelay =
            Mathf.Clamp(
                effectDelay,
                0f,
                useDuration
            );
    }
}