using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Taskbar")]
    [SerializeField] private GameObject skillTaskbar;

    [Header("Skill Slots")]
    [SerializeField] private SkillSlotUI slot3;
    [SerializeField] private SkillSlotUI slot4;
    [SerializeField] private SkillSlotUI slot5;

    private bool initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeSlots();
    }

    private void InitializeSlots()
    {
        if (initialized)
            return;

        initialized = true;

        InitializeSlot(slot3);
        InitializeSlot(slot4);
        InitializeSlot(slot5);
    }

    private void InitializeSlot(SkillSlotUI slot)
    {
        if (slot == null)
            return;

        slot.Clear();
        slot.gameObject.SetActive(false);
    }

    public void EquipSkill(AbilityData data, int slotNumber)
    {
        if (data == null)
        {
            Debug.LogError("AbilityData truyền vào EquipSkill đang null.");
            return;
        }

        SkillSlotUI targetSlot = GetSlot(slotNumber);

        if (targetSlot == null)
        {
            Debug.LogError(
                $"Không tìm thấy SkillSlotUI của slot {slotNumber}."
            );
            return;
        }

        // Bật Taskbar trước.
        if (skillTaskbar != null)
            skillTaskbar.SetActive(true);

        // Bật toàn bộ cha của Slot nếu đang bị tắt.
        ActivateParents(targetSlot.transform);

        targetSlot.gameObject.SetActive(true);
        targetSlot.Setup(data);

        Debug.Log(
            $"Đã trang bị {data.skillName} vào Slot {slotNumber}. " +
            $"Slot active: {targetSlot.gameObject.activeInHierarchy}"
        );
    }

    private void ActivateParents(Transform target)
    {
        Transform current = target.parent;

        while (current != null && current != transform.root)
        {
            if (!current.gameObject.activeSelf)
                current.gameObject.SetActive(true);

            current = current.parent;
        }
    }

    public void UnequipSkill(int slotNumber)
    {
        SkillSlotUI targetSlot = GetSlot(slotNumber);

        if (targetSlot == null)
            return;

        targetSlot.Clear();
        targetSlot.gameObject.SetActive(false);
    }

    private SkillSlotUI GetSlot(int slotNumber)
    {
        switch (slotNumber)
        {
            case 3:
                return slot3;

            case 4:
                return slot4;

            case 5:
                return slot5;

            default:
                Debug.LogError("Số slot không hợp lệ: " + slotNumber);
                return null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}