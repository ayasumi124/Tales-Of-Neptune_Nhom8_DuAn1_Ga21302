using UnityEngine;

public class ElementEquipmentManager : MonoBehaviour
{
    public static ElementEquipmentManager Instance
    {
        get;
        private set;
    }

    [Header("Element Slot")]
    [SerializeField]
    private ElementSlotUI slot5;

    [Header("Slot Key")]
    [SerializeField]
    private KeyCode slot5Key =
        KeyCode.Q;

    [Header("Equipped Element")]
    [SerializeField]
    private ElementData equippedElement;

    public ElementData EquippedElement =>
        equippedElement;

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
        RefreshSlot();
    }

    private void Update()
    {
        if (Input.GetKeyDown(slot5Key))
        {
            TryOpenElementSlot();
        }
    }

    private void TryOpenElementSlot()
    {
        if (slot5 == null ||
            !slot5.gameObject.activeInHierarchy ||
            slot5.IsEmpty)
        {
            return;
        }

        OpenElement(
            slot5.ElementData
        );
    }

    public bool EquipElement(
        ElementData element)
    {
        return EquipElementToSlot(
            element,
            5
        );
    }

    public bool EquipElementToSlot(
        ElementData element,
        int slotNumber)
    {
        if (element == null)
        {
            Debug.LogError(
                "ElementData truyền vào đang null."
            );

            return false;
        }

        if (slotNumber != 5)
        {
            Debug.LogError(
                "Element chỉ được trang bị vào Slot 5."
            );

            return false;
        }

        if (slot5 == null)
        {
            Debug.LogError(
                "ElementEquipmentManager chưa gán Slot 5."
            );

            return false;
        }

        equippedElement =
            element;

        ActivateParents(
            slot5.transform
        );

        slot5.gameObject.SetActive(true);

        slot5.Setup(
            element,
            slot5Key
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }

        Debug.Log(
            $"Đã trang bị Element " +
            $"{element.elementName} " +
            "vào Slot 5."
        );

        return true;
    }

    public void UnequipElement()
    {
        ElementData removedElement =
            equippedElement;

        equippedElement = null;

        if (slot5 != null)
        {
            slot5.Clear();
            slot5.gameObject.SetActive(false);
        }

        if (ElementSkillBarUI.Instance != null &&
            ElementSkillBarUI.Instance
                .CurrentElement == removedElement)
        {
            ElementSkillBarUI.Instance.Hide();
        }

        Debug.Log(
            "Đã tháo Element khỏi Slot 5."
        );
    }

    public void UnequipElement(
        int slotNumber)
    {
        if (slotNumber != 5)
        {
            Debug.LogError(
                "Element chỉ tồn tại ở Slot 5."
            );

            return;
        }

        UnequipElement();
    }

    public bool IsElementEquipped(
        ElementData element)
    {
        return element != null &&
               equippedElement == element;
    }

    public void RefreshSlot()
    {
        if (slot5 == null)
            return;

        if (equippedElement == null)
        {
            slot5.Clear();
            slot5.gameObject.SetActive(false);
            return;
        }

        ActivateParents(
            slot5.transform
        );

        slot5.gameObject.SetActive(true);

        slot5.Setup(
            equippedElement,
            slot5Key
        );
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

    public void ClearElement()
    {
        equippedElement = null;

        if (slot5 != null)
        {
            slot5.Clear();
            slot5.gameObject.SetActive(false);
        }

        if (ElementSkillBarUI.Instance != null)
        {
            ElementSkillBarUI.Instance.Hide();
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