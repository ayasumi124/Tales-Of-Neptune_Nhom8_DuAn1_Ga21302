using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryElementButton :
    MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Selection")]
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject actionPanel;

    [Header("Equip")]
    [SerializeField] private Button equipButton;

    private ElementData elementData;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton =
                GetComponent<Button>();
        }

        if (selectButton != null)
        {
            selectButton.onClick
                .RemoveListener(
                    Select
                );

            selectButton.onClick
                .AddListener(
                    Select
                );
        }

        if (equipButton != null)
        {
            equipButton.onClick
                .RemoveListener(
                    EquipElement
                );

            equipButton.onClick
                .AddListener(
                    EquipElement
                );
        }

        SetSelected(false);
    }

    public void Setup(
        ElementData data)
    {
        elementData = data;

        if (data == null)
        {
            Clear();
            return;
        }

        if (icon != null)
        {
            icon.sprite =
                data.elementIcon;

            icon.enabled =
                data.elementIcon != null;

            icon.preserveAspect =
                true;

            Color color =
                icon.color;

            color.a = 1f;
            icon.color = color;
        }

        if (nameText != null)
        {
            nameText.text =
                data.elementName;
        }

        SetSelected(false);
    }

    public void Select()
    {
        if (elementData == null)
            return;

        SetSelected(
            actionPanel == null ||
            !actionPanel.activeSelf
        );

        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInventorySelect();
            }
        }
    }

    private void SetSelected(
        bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(
                selected
            );
        }

        if (actionPanel != null)
        {
            actionPanel.SetActive(
                selected
            );
        }
    }

    public void EquipElement()
    {
        if (elementData == null)
            return;

        if (ElementEquipmentManager.Instance ==
            null)
        {
            Debug.LogError(
                "Không tìm thấy " +
                "ElementEquipmentManager."
            );

            return;
        }

        ElementEquipmentManager.Instance
            .EquipElementToSlot(
                elementData,
                5
            );

        Debug.Log(
            $"Đã trang bị Element " +
            $"{elementData.elementName} " +
            "vào Slot 5."
        );

        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayInventorySelect();
            }
        }

        SetSelected(false);
    }

    private void Clear()
    {
        elementData = null;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = "";
        }

        SetSelected(false);
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick
                .RemoveListener(
                    Select
                );
        }

        if (equipButton != null)
        {
            equipButton.onClick
                .RemoveListener(
                    EquipElement
                );
        }
    }
}