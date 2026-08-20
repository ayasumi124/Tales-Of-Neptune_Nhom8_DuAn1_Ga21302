using System;
using UnityEngine;

public class PlayerEquipmentManager : MonoBehaviour
{
    public static PlayerEquipmentManager Instance
    {
        get;
        private set;
    }

    [Header("Equipped Items")]
    [SerializeField]
    private ItemData equippedWeapon;

    [SerializeField]
    private ItemData equippedHelmet;

    [SerializeField]
    private ItemData equippedArmor;

    public ItemData EquippedWeapon =>
        equippedWeapon;

    public ItemData EquippedHelmet =>
        equippedHelmet;

    public ItemData EquippedArmor =>
        equippedArmor;

    public static event Action
        OnEquipmentChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(
            gameObject
        );
    }

    // =====================================================
    // EQUIP
    // =====================================================

    public bool Equip(
        ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (!item.Equippable)
        {
            Debug.Log(
                $"{item.ItemName} không phải Equipment."
            );

            return false;
        }

        switch (
            item.EquipmentType)
        {
            case EquipmentType.Weapon:

                equippedWeapon =
                    item;

                break;

            case EquipmentType.Helmet:

                equippedHelmet =
                    item;

                break;

            case EquipmentType.Armor:

                equippedArmor =
                    item;

                break;

            default:

                return false;
        }

        Debug.Log(
            $"Đã trang bị " +
            $"{item.ItemName}."
        );

        OnEquipmentChanged?.Invoke();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }

        return true;
    }

    // =====================================================
    // UNEQUIP
    // =====================================================

    public void Unequip(
        EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon:

                equippedWeapon = null;

                break;

            case EquipmentType.Helmet:

                equippedHelmet = null;

                break;

            case EquipmentType.Armor:

                equippedArmor = null;

                break;
        }

        OnEquipmentChanged?.Invoke();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }
    }

    // =====================================================
    // GET
    // =====================================================

    public ItemData GetEquipment(
        EquipmentType type)
    {
        switch (type)
        {
            case EquipmentType.Weapon:
                return equippedWeapon;

            case EquipmentType.Helmet:
                return equippedHelmet;

            case EquipmentType.Armor:
                return equippedArmor;

            default:
                return null;
        }
    }

    public bool IsEquipped(
        ItemData item)
    {
        if (item == null)
            return false;

        return
            equippedWeapon == item ||
            equippedHelmet == item ||
            equippedArmor == item;
    }

    // =====================================================
    // STATS
    // =====================================================

    public float GetAttackBonus()
    {
        float bonus = 0f;

        AddAttackBonus(
            equippedWeapon,
            ref bonus
        );

        AddAttackBonus(
            equippedHelmet,
            ref bonus
        );

        AddAttackBonus(
            equippedArmor,
            ref bonus
        );

        return bonus;
    }

    private void AddAttackBonus(
        ItemData item,
        ref float value)
    {
        if (item != null)
        {
            value +=
                item.AttackBonus;
        }
    }

    public float GetDefenseBonus()
    {
        float bonus = 0f;

        AddDefenseBonus(
            equippedWeapon,
            ref bonus
        );

        AddDefenseBonus(
            equippedHelmet,
            ref bonus
        );

        AddDefenseBonus(
            equippedArmor,
            ref bonus
        );

        return bonus;
    }

    private void AddDefenseBonus(
        ItemData item,
        ref float value)
    {
        if (item != null)
        {
            value +=
                item.DefenseBonus;
        }
    }

    public int GetMaxHealthBonus()
    {
        int bonus = 0;

        if (equippedWeapon != null)
        {
            bonus +=
                equippedWeapon
                    .MaxHealthBonus;
        }

        if (equippedHelmet != null)
        {
            bonus +=
                equippedHelmet
                    .MaxHealthBonus;
        }

        if (equippedArmor != null)
        {
            bonus +=
                equippedArmor
                    .MaxHealthBonus;
        }

        return bonus;
    }

    public float GetMaxManaBonus()
    {
        float bonus = 0f;

        if (equippedWeapon != null)
        {
            bonus +=
                equippedWeapon
                    .MaxManaBonus;
        }

        if (equippedHelmet != null)
        {
            bonus +=
                equippedHelmet
                    .MaxManaBonus;
        }

        if (equippedArmor != null)
        {
            bonus +=
                equippedArmor
                    .MaxManaBonus;
        }

        return bonus;
    }

    // =====================================================
    // CLEAR
    // =====================================================

    public void ClearAllEquipment()
    {
        equippedWeapon = null;
        equippedHelmet = null;
        equippedArmor = null;

        OnEquipmentChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}