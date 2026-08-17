using UnityEngine;

[CreateAssetMenu(
    fileName = "New Item",
    menuName = "Inventory/Item Data"
)]
public class ItemData : ScriptableObject
{
    // =====================================================
    // IDENTITY
    // =====================================================

    [Header("Identity")]
    [SerializeField]
    private string itemID;

    [SerializeField]
    private string itemName;

    [TextArea(3, 6)]
    [SerializeField]
    private string description;

    [SerializeField]
    private Sprite icon;

    // =====================================================
    // CLASSIFICATION
    // =====================================================

    [Header("Classification")]
    [SerializeField]
    private ItemType itemType;

    [SerializeField]
    private ItemEffectType effectType;

    // =====================================================
    // EQUIPMENT
    // =====================================================

    [Header("Equipment")]
    [Tooltip(
        "Bật nếu item này là trang bị."
    )]
    [SerializeField]
    private bool equippable;

    [Tooltip(
        "Loại trang bị. Chỉ dùng khi Equippable = true."
    )]
    [SerializeField]
    private EquipmentType equipmentType;

    [Header("Equipment Stats")]
    [Tooltip(
        "Bonus damage khi trang bị."
    )]
    [SerializeField]
    private float attackBonus;

    [Tooltip(
        "Bonus defense khi trang bị."
    )]
    [SerializeField]
    private float defenseBonus;

    [Tooltip(
        "Bonus Max HP khi trang bị."
    )]
    [SerializeField]
    private int maxHealthBonus;

    [Tooltip(
        "Bonus Max Mana khi trang bị."
    )]
    [SerializeField]
    private float maxManaBonus;

    // =====================================================
    // STACK
    // =====================================================

    [Header("Stack")]
    [SerializeField]
    private bool stackable = true;

    [Min(1)]
    [SerializeField]
    private int maxStack = 99;

    // =====================================================
    // EFFECT
    // =====================================================

    [Header("Effect Value")]
    [Tooltip(
        "Ví dụ: hồi 20 HP hoặc tăng 2 Max HP."
    )]
    [SerializeField]
    private float effectValue = 10f;

    // =====================================================
    // USE
    // =====================================================

    [Header("Use")]
    [Tooltip(
        "Vật phẩm có thể bấm Use trong Inventory hay không."
    )]
    [SerializeField]
    private bool usable = true;

    [Tooltip(
        "Thời gian animation sử dụng vật phẩm."
    )]
    [SerializeField]
    private float useDuration = 0.8f;

    [Tooltip(
        "Thời điểm áp dụng hiệu ứng trong animation."
    )]
    [SerializeField]
    private float effectDelay = 0.4f;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField]
    private AudioClip useSound;

    public AudioClip pickupSound;

    [Range(0f, 20f)]
    public float pickupVolume = 3f;

    // =====================================================
    // GETTERS
    // =====================================================

    public string ItemID =>
        itemID;

    public string ItemName =>
        itemName;

    public string Description =>
        description;

    public Sprite Icon =>
        icon;

    public ItemType ItemType =>
        itemType;

    public ItemEffectType EffectType =>
        effectType;

    public bool Stackable =>
        stackable;

    public int MaxStack =>
        maxStack;

    public float EffectValue =>
        effectValue;

    public bool Usable =>
        usable;

    public float UseDuration =>
        useDuration;

    public float EffectDelay =>
        effectDelay;

    public AudioClip UseSound =>
        useSound;

    // =====================================================
    // EQUIPMENT GETTERS
    // =====================================================

    public bool Equippable =>
        equippable;

    public EquipmentType EquipmentType =>
        equipmentType;

    public float AttackBonus =>
        attackBonus;

    public float DefenseBonus =>
        defenseBonus;

    public int MaxHealthBonus =>
        maxHealthBonus;

    public float MaxManaBonus =>
        maxManaBonus;

    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(
                itemID))
        {
            itemID =
                name.Trim()
                    .ToLower()
                    .Replace(
                        " ",
                        "_"
                    );
        }

        /*
         * Equipment không stack.
         */
        if (equippable)
        {
            stackable = false;
            maxStack = 1;

            /*
             * Equipment không dùng bằng
             * hệ thống potion Use.
             */
            usable = false;
        }

        if (!stackable)
        {
            maxStack = 1;
        }

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

        attackBonus =
            Mathf.Max(
                0f,
                attackBonus
            );

        defenseBonus =
            Mathf.Max(
                0f,
                defenseBonus
            );

        maxHealthBonus =
            Mathf.Max(
                0,
                maxHealthBonus
            );

        maxManaBonus =
            Mathf.Max(
                0f,
                maxManaBonus
            );
    }
}