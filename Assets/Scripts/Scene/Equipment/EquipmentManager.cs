using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance
    {
        get;
        private set;
    }

    [Header("Ability Slots")]
    [Tooltip("Ability tự trang bị ở Slot 3.")]
    [SerializeField]
    private SkillSlotUI slot3;

    [Tooltip("Ability tự trang bị ở Slot 4.")]
    [SerializeField]
    private SkillSlotUI slot4;

    [Header("Equipped Data")]
    [SerializeField]
    private AbilityData equippedSlot3;

    [SerializeField]
    private AbilityData equippedSlot4;

    public AbilityData EquippedSlot3 =>
        equippedSlot3;

    public AbilityData EquippedSlot4 =>
        equippedSlot4;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        RefreshAllSlots();
    }

    public bool EquipSkill(
        AbilityData data,
        int slotNumber)
    {
        if (data == null)
        {
            Debug.LogError(
                "AbilityData truyền vào EquipSkill đang null."
            );

            return false;
        }

        if (slotNumber != 3 &&
            slotNumber != 4)
        {
            Debug.LogError(
                $"Ability chỉ được trang bị vào Slot 3 hoặc Slot 4. " +
                $"Slot nhận được: {slotNumber}"
            );

            return false;
        }

        SkillSlotUI targetSlot =
            GetSlot(slotNumber);

        if (targetSlot == null)
        {
            Debug.LogError(
                $"Chưa gán SkillSlotUI cho Slot {slotNumber}."
            );

            return false;
        }

        RemoveDuplicateFromOtherSlot(
            data,
            slotNumber
        );

        SetEquippedData(
            slotNumber,
            data
        );

        ActivateParents(
            targetSlot.transform
        );

        targetSlot.gameObject.SetActive(true);
        targetSlot.Setup(data);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }

        Debug.Log(
            $"Đã trang bị {data.skillName} " +
            $"vào Slot {slotNumber}."
        );

        return true;
    }

    public bool EquipToFirstAvailableSlot(
        AbilityData data)
    {
        if (data == null)
            return false;

        if (equippedSlot3 == null)
        {
            return EquipSkill(
                data,
                3
            );
        }

        if (equippedSlot4 == null)
        {
            return EquipSkill(
                data,
                4
            );
        }

        Debug.LogWarning(
            "Slot 3 và Slot 4 đều đã đầy."
        );

        return false;
    }

    public void UnequipSkill(
        int slotNumber)
    {
        SkillSlotUI targetSlot =
            GetSlot(slotNumber);

        if (targetSlot == null)
            return;

        SetEquippedData(
            slotNumber,
            null
        );

        targetSlot.Clear();
        targetSlot.gameObject.SetActive(false);

        Debug.Log(
            $"Đã tháo Ability khỏi Slot {slotNumber}."
        );
    }

    public bool IsAbilityEquipped(
        AbilityData data)
    {
        if (data == null)
            return false;

        return equippedSlot3 == data ||
               equippedSlot4 == data;
    }

    public int GetEquippedSlot(
        AbilityData data)
    {
        if (data == null)
            return -1;

        if (equippedSlot3 == data)
            return 3;

        if (equippedSlot4 == data)
            return 4;

        return -1;
    }

    public AbilityData GetEquippedAbility(
        int slotNumber)
    {
        switch (slotNumber)
        {
            case 3:
                return equippedSlot3;

            case 4:
                return equippedSlot4;

            default:
                return null;
        }
    }

    public void RefreshAllSlots()
    {
        RefreshSlot(
            slot3,
            equippedSlot3
        );

        RefreshSlot(
            slot4,
            equippedSlot4
        );
    }

    private void RefreshSlot(
        SkillSlotUI slot,
        AbilityData data)
    {
        if (slot == null)
            return;

        if (data == null)
        {
            slot.Clear();
            slot.gameObject.SetActive(false);
            return;
        }

        ActivateParents(
            slot.transform
        );

        slot.gameObject.SetActive(true);
        slot.Setup(data);
    }

    private void RemoveDuplicateFromOtherSlot(
        AbilityData data,
        int targetSlotNumber)
    {
        if (targetSlotNumber != 3 &&
            equippedSlot3 == data)
        {
            equippedSlot3 = null;

            if (slot3 != null)
            {
                slot3.Clear();
                slot3.gameObject.SetActive(false);
            }
        }

        if (targetSlotNumber != 4 &&
            equippedSlot4 == data)
        {
            equippedSlot4 = null;

            if (slot4 != null)
            {
                slot4.Clear();
                slot4.gameObject.SetActive(false);
            }
        }
    }

    private void SetEquippedData(
        int slotNumber,
        AbilityData data)
    {
        switch (slotNumber)
        {
            case 3:
                equippedSlot3 = data;
                break;

            case 4:
                equippedSlot4 = data;
                break;
        }
    }

    private SkillSlotUI GetSlot(
        int slotNumber)
    {
        switch (slotNumber)
        {
            case 3:
                return slot3;

            case 4:
                return slot4;

            default:
                return null;
        }
    }

    private void ActivateParents(
        Transform target)
    {
        if (target == null)
            return;

        Transform current =
            target.parent;

        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    public void ClearAllEquipment()
    {
        equippedSlot3 = null;
        equippedSlot4 = null;

        if (slot3 != null)
        {
            slot3.Clear();
            slot3.gameObject.SetActive(false);
        }

        if (slot4 != null)
        {
            slot4.Clear();
            slot4.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}