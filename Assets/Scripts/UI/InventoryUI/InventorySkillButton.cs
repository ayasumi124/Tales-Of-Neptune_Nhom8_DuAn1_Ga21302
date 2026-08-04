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

    [Header("Main UI")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Selection")]
    [SerializeField] private GameObject selectedFrame;
    [SerializeField] private GameObject actionPanel;

    [Header("Ability Actions")]
    [SerializeField] private GameObject abilityActionGroup;
    [SerializeField] private Button equipSlot3Button;
    [SerializeField] private Button equipSlot4Button;

    [Header("Element Actions")]
    [SerializeField] private GameObject elementActionGroup;
    [SerializeField] private Button equipElementButton;

    [Header("Unequip")]
    [SerializeField] private Button unequipButton;

    [Header("Status")]
    [SerializeField] private GameObject equippedBadge;
    [SerializeField] private TextMeshProUGUI equippedText;

    private InventorySkillType skillType =
        InventorySkillType.None;

    private AbilityData abilityData;
    private ElementData elementData;

    public InventorySkillType SkillType =>
        skillType;

    private void Awake()
    {
        if (selectButton == null)
        {
            selectButton =
                GetComponent<Button>();
        }

        AddListeners();
        SetSelected(false);
        RefreshStatus();
    }

    private void OnEnable()
    {
        RefreshStatus();
    }

    private void AddListeners()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(
                ToggleSelection
            );

            selectButton.onClick.AddListener(
                ToggleSelection
            );
        }

        if (equipSlot3Button != null)
        {
            equipSlot3Button.onClick.RemoveListener(
                EquipAbilityToSlot3
            );

            equipSlot3Button.onClick.AddListener(
                EquipAbilityToSlot3
            );
        }

        if (equipSlot4Button != null)
        {
            equipSlot4Button.onClick.RemoveListener(
                EquipAbilityToSlot4
            );

            equipSlot4Button.onClick.AddListener(
                EquipAbilityToSlot4
            );
        }

        if (equipElementButton != null)
        {
            equipElementButton.onClick.RemoveListener(
                EquipElement
            );

            equipElementButton.onClick.AddListener(
                EquipElement
            );
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                Unequip
            );

            unequipButton.onClick.AddListener(
                Unequip
            );
        }
    }

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

        abilityData = data;

        SetVisual(
            data.icon,
            data.skillName
        );

        if (abilityActionGroup != null)
        {
            abilityActionGroup.SetActive(
                true
            );
        }

        if (elementActionGroup != null)
        {
            elementActionGroup.SetActive(
                false
            );
        }

        SetSelected(false);
        RefreshStatus();
    }

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

        elementData = data;

        SetVisual(
            data.elementIcon,
            data.elementName
        );

        if (abilityActionGroup != null)
        {
            abilityActionGroup.SetActive(
                false
            );
        }

        if (elementActionGroup != null)
        {
            elementActionGroup.SetActive(
                true
            );
        }

        SetSelected(false);
        RefreshStatus();
    }

    private void SetVisual(
        Sprite newIcon,
        string displayName)
    {
        if (icon != null)
        {
            icon.sprite = newIcon;
            icon.enabled = newIcon != null;
            icon.preserveAspect = true;

            Color color =
                icon.color;

            color.a = 1f;
            icon.color = color;
        }

        if (nameText != null)
        {
            nameText.text =
                displayName ?? "";
        }
    }

    private void ToggleSelection()
    {
        if (skillType ==
            InventorySkillType.None)
        {
            return;
        }

        if (SkillInventoryUI.Instance != null)
        {
            SkillInventoryUI.Instance
                .SelectButton(this);
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

        if (actionPanel != null)
        {
            actionPanel.SetActive(
                selected
            );
        }
    }

    private void EquipAbilityToSlot3()
    {
        EquipAbility(3);
    }

    private void EquipAbilityToSlot4()
    {
        EquipAbility(4);
    }

    private void EquipAbility(
        int slotNumber)
    {
        if (skillType !=
                InventorySkillType.Ability ||
            abilityData == null)
        {
            return;
        }

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy EquipmentManager."
            );

            return;
        }

        bool equipped =
            EquipmentManager.Instance
                .EquipSkill(
                    abilityData,
                    slotNumber
                );

        if (!equipped)
            return;

        PlayEquipSound();

        SetSelected(false);

        RefreshAllInventoryButtons();
    }

    private void EquipElement()
    {
        if (skillType !=
                InventorySkillType.Element ||
            elementData == null)
        {
            return;
        }

        if (ElementEquipmentManager.Instance ==
            null)
        {
            Debug.LogError(
                "Không tìm thấy ElementEquipmentManager."
            );

            return;
        }

        bool equipped =
            ElementEquipmentManager.Instance
                .EquipElementToSlot(
                    elementData,
                    5
                );

        if (!equipped)
            return;

        PlayEquipSound();

        SetSelected(false);

        RefreshAllInventoryButtons();
    }

    private void Unequip()
    {
        switch (skillType)
        {
            case InventorySkillType.Ability:
                UnequipAbility();
                break;

            case InventorySkillType.Element:
                UnequipElement();
                break;
        }

        SetSelected(false);

        RefreshAllInventoryButtons();
    }

    private void UnequipAbility()
    {
        if (abilityData == null ||
            EquipmentManager.Instance == null)
        {
            return;
        }

        int equippedSlot =
            EquipmentManager.Instance
                .GetEquippedSlot(
                    abilityData
                );

        if (equippedSlot < 0)
            return;

        EquipmentManager.Instance
            .UnequipSkill(
                equippedSlot
            );

        PlayEquipSound();
    }

    private void UnequipElement()
    {
        if (elementData == null ||
            ElementEquipmentManager.Instance ==
            null)
        {
            return;
        }

        if (!ElementEquipmentManager.Instance
                .IsElementEquipped(
                    elementData))
        {
            return;
        }

        ElementEquipmentManager.Instance
            .UnequipElement();

        PlayEquipSound();
    }

    public void RefreshStatus()
    {
        bool isEquipped = false;
        string status = "";

        switch (skillType)
        {
            case InventorySkillType.Ability:
                RefreshAbilityStatus(
                    ref isEquipped,
                    ref status
                );
                break;

            case InventorySkillType.Element:
                RefreshElementStatus(
                    ref isEquipped,
                    ref status
                );
                break;
        }

        if (equippedBadge != null)
        {
            equippedBadge.SetActive(
                isEquipped
            );
        }

        if (equippedText != null)
        {
            equippedText.text =
                isEquipped
                    ? status
                    : "";
        }

        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(
                isEquipped
            );
        }
    }

    private void RefreshAbilityStatus(
        ref bool isEquipped,
        ref string status)
    {
        if (abilityData == null ||
            EquipmentManager.Instance == null)
        {
            return;
        }

        int equippedSlot =
            EquipmentManager.Instance
                .GetEquippedSlot(
                    abilityData
                );

        if (equippedSlot < 0)
            return;

        isEquipped = true;
        status =
            $"Equipped - Slot {equippedSlot}";
    }

    private void RefreshElementStatus(
        ref bool isEquipped,
        ref string status)
    {
        if (elementData == null ||
            ElementEquipmentManager.Instance ==
            null)
        {
            return;
        }

        isEquipped =
            ElementEquipmentManager.Instance
                .IsElementEquipped(
                    elementData
                );

        if (isEquipped)
        {
            status =
                "Equipped - Slot 5";
        }
    }

    private void RefreshAllInventoryButtons()
    {
        InventorySkillButton[] buttons =
            FindObjectsByType<
                InventorySkillButton
            >(
                FindObjectsSortMode.None
            );

        foreach (InventorySkillButton button
                 in buttons)
        {
            if (button != null)
            {
                button.RefreshStatus();
            }
        }
    }

    private void PlayEquipSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }
    }

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

        if (abilityActionGroup != null)
        {
            abilityActionGroup.SetActive(
                false
            );
        }

        if (elementActionGroup != null)
        {
            elementActionGroup.SetActive(
                false
            );
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

        if (unequipButton != null)
        {
            unequipButton.gameObject.SetActive(
                false
            );
        }

        SetSelected(false);
    }

    private void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(
                ToggleSelection
            );
        }

        if (equipSlot3Button != null)
        {
            equipSlot3Button.onClick.RemoveListener(
                EquipAbilityToSlot3
            );
        }

        if (equipSlot4Button != null)
        {
            equipSlot4Button.onClick.RemoveListener(
                EquipAbilityToSlot4
            );
        }

        if (equipElementButton != null)
        {
            equipElementButton.onClick.RemoveListener(
                EquipElement
            );
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                Unequip
            );
        }
    }
}