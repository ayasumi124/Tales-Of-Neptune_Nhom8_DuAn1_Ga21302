using UnityEngine;

public class ElementEquipmentManager : MonoBehaviour
{
    public static ElementEquipmentManager Instance
    {
        get;
        private set;
    }

    [Header("Element Slots")]
    [SerializeField] private ElementSlotUI slot4;
    [SerializeField] private ElementSlotUI slot5;

    [Header("Slot Keys")]
    [SerializeField] private KeyCode slot4Key =
        KeyCode.F;

    [SerializeField] private KeyCode slot5Key =
        KeyCode.R;

    private bool initialized;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
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

        InitializeSlot(slot4);
        InitializeSlot(slot5);
    }

    private void InitializeSlot(
        ElementSlotUI slot)
    {
        if (slot == null)
            return;

        slot.Clear();
        slot.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(slot4Key))
        {
            TryOpenElementSlot(slot4);
        }

        if (Input.GetKeyDown(slot5Key))
        {
            TryOpenElementSlot(slot5);
        }
    }

    private void TryOpenElementSlot(
        ElementSlotUI slot)
    {
        if (slot == null ||
            !slot.gameObject.activeInHierarchy ||
            slot.IsEmpty)
        {
            return;
        }

        OpenElement(
            slot.ElementData
        );
    }

    public bool EquipElement(
        ElementData element)
    {
        if (element == null)
        {
            Debug.LogError(
                "ElementData truyền vào EquipElement đang null."
            );

            return false;
        }

        if (IsElementEquipped(element))
        {
            Debug.Log(
                $"{element.elementName} đã được trang bị."
            );

            return true;
        }

        if (slot4 != null &&
            slot4.IsEmpty)
        {
            ActivateSlot(
                slot4,
                element,
                slot4Key
            );

            return true;
        }

        if (slot5 != null &&
            slot5.IsEmpty)
        {
            ActivateSlot(
                slot5,
                element,
                slot5Key
            );

            return true;
        }

        Debug.Log(
            $"Slot4 và Slot5 đã đầy. " +
            $"{element.elementName} sẽ được đưa vào Inventory."
        );

        // Giai đoạn sau:
        // ElementInventoryUI.Instance.AddElement(element);

        return false;
    }

    public void EquipElementToSlot(
        ElementData element,
        int slotNumber)
    {
        if (element == null)
            return;

        switch (slotNumber)
        {
            case 4:
                ActivateSlot(
                    slot4,
                    element,
                    slot4Key
                );
                break;

            case 5:
                ActivateSlot(
                    slot5,
                    element,
                    slot5Key
                );
                break;

            default:
                Debug.LogError(
                    "Element chỉ được trang bị vào Slot4 hoặc Slot5."
                );
                break;
        }
    }

    private void ActivateSlot(
        ElementSlotUI slot,
        ElementData element,
        KeyCode key)
    {
        if (slot == null ||
            element == null)
        {
            return;
        }

        ActivateParents(
            slot.transform
        );

        slot.gameObject.SetActive(true);

        slot.Setup(
            element,
            key
        );

        Debug.Log(
            $"Đã trang bị {element.elementName} " +
            $"vào {slot.gameObject.name}."
        );
    }

    private void ActivateParents(
        Transform target)
    {
        if (target == null)
            return;

        Transform current =
            target.parent;

        while (current != null &&
               current != transform.root)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);
            }

            current = current.parent;
        }
    }

    private void OpenElement(
        ElementData element)
    {
        if (element == null)
            return;

        if (ElementSkillBarUI.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy ElementSkillBarUI."
            );

            return;
        }

        if (ElementSkillBarUI.Instance.IsOpen &&
            ElementSkillBarUI.Instance
                .CurrentElement == element)
        {
            ElementSkillBarUI.Instance.Hide();
            return;
        }

        ElementSkillBarUI.Instance
            .ShowElement(element);
    }

    private bool IsElementEquipped(
        ElementData element)
    {
        bool inSlot4 =
            slot4 != null &&
            slot4.ElementData == element;

        bool inSlot5 =
            slot5 != null &&
            slot5.ElementData == element;

        return inSlot4 || inSlot5;
    }

    public void UnequipElement(
        int slotNumber)
    {
        ElementSlotUI targetSlot = null;

        switch (slotNumber)
        {
            case 4:
                targetSlot = slot4;
                break;

            case 5:
                targetSlot = slot5;
                break;

            default:
                return;
        }

        if (targetSlot == null)
            return;

        ElementData removedElement =
            targetSlot.ElementData;

        targetSlot.Clear();
        targetSlot.gameObject.SetActive(false);

        if (ElementSkillBarUI.Instance != null &&
            ElementSkillBarUI.Instance
                .CurrentElement == removedElement)
        {
            ElementSkillBarUI.Instance.Hide();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}