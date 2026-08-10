using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySkillButton : MonoBehaviour
{
    public enum InventorySkillType
    {
        None,
        Ability,
        Element
    }

    // =====================================================
    // UI
    // =====================================================

    [Header("Main UI")]
    [SerializeField]
    private Button selectButton;

    [SerializeField]
    private Image icon;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [Header("Selection")]
    [SerializeField]
    private GameObject selectedFrame;

    [Header("Status")]
    [SerializeField]
    private GameObject equippedBadge;

    [SerializeField]
    private TextMeshProUGUI equippedText;

    // =====================================================
    // DATA
    // =====================================================

    private InventorySkillType skillType =
        InventorySkillType.None;

    private AbilityData abilityData;

    private ElementData elementData;

    public InventorySkillType SkillType =>
        skillType;

    public AbilityData AbilityData =>
        abilityData;

    public ElementData ElementData =>
        elementData;

    // =====================================================
    // UNITY
    // =====================================================

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

        SetSelected(
            false
        );
    }

    // =====================================================
    // SETUP ABILITY
    // =====================================================

    public void SetupAbility(
        AbilityData data)
    {
        ClearData();

        if (data == null)
        {
            ClearVisual();
            return;
        }

        skillType =
            InventorySkillType.Ability;

        abilityData =
            data;

        SetVisual(
            data.icon,
            data.skillName
        );

        SetSelected(
            false
        );

        RefreshStatus();
    }

    // =====================================================
    // SETUP ELEMENT
    // =====================================================

    public void SetupElement(
        ElementData data)
    {
        ClearData();

        if (data == null)
        {
            ClearVisual();
            return;
        }

        skillType =
            InventorySkillType.Element;

        elementData =
            data;

        SetVisual(
            data.elementIcon,
            data.elementName
        );

        SetSelected(
            false
        );

        RefreshStatus();
    }

    // =====================================================
    // VISUAL
    // =====================================================

    private void SetVisual(
        Sprite newIcon,
        string displayName)
    {
        if (icon != null)
        {
            icon.sprite =
                newIcon;

            icon.enabled =
                newIcon != null;

            icon.preserveAspect =
                true;

            Color color =
                icon.color;

            color.a = 1f;

            icon.color =
                color;
        }

        if (nameText != null)
        {
            nameText.text =
                displayName ?? "";
        }
    }

    // =====================================================
    // SELECT
    // =====================================================

    private void Select()
    {
        if (skillType ==
            InventorySkillType.None)
        {
            return;
        }

        if (SkillInventoryUI.Instance != null)
        {
            SkillInventoryUI.Instance
                .SelectButton(
                    this
                );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventorySelect();
        }
    }

    public void SetSelected(
        bool selected)
    {
        if (selectedFrame != null)
        {
            selectedFrame.SetActive(
                selected
            );
        }
    }

    // =====================================================
    // STATUS
    // =====================================================

    public void RefreshStatus()
    {
        bool equipped = false;

        string text = "";

        // =============================================
        // ABILITY
        // =============================================

        if (skillType ==
                InventorySkillType.Ability &&
            abilityData != null &&
            EquipmentManager.Instance != null)
        {
            int slot =
                EquipmentManager.Instance
                    .GetEquippedSlot(
                        abilityData
                    );

            if (slot >= 0)
            {
                equipped = true;

                text =
                    $"Slot {slot}";
            }
        }

        // =============================================
        // ELEMENT
        // =============================================

        else if (
            skillType ==
                InventorySkillType.Element &&
            elementData != null &&
            ElementEquipmentManager.Instance != null)
        {
            equipped =
                ElementEquipmentManager.Instance
                    .IsElementEquipped(
                        elementData
                    );

            if (equipped)
            {
                text =
                    "Slot 5";
            }
        }

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(
                equipped
            );
        }

        if (equippedText != null)
        {
            equippedText.text =
                equipped
                    ? text
                    : "";
        }
    }

    // =====================================================
    // CLEAR
    // =====================================================

    private void ClearData()
    {
        skillType =
            InventorySkillType.None;

        abilityData = null;

        elementData = null;
    }

    private void ClearVisual()
    {
        if (icon != null)
        {
            icon.sprite = null;

            icon.enabled = false;
        }

        if (nameText != null)
        {
            nameText.text = "";
        }

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(
                false
            );
        }

        if (equippedText != null)
        {
            equippedText.text = "";
        }

        SetSelected(
            false
        );
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick
                .RemoveListener(
                    Select
                );
        }
    }
}