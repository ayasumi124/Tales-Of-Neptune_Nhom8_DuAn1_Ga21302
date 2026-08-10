using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillInventoryUI : MonoBehaviour
{
    public static SkillInventoryUI Instance
    {
        get;
        private set;
    }

    // =====================================================
    // PANEL
    // =====================================================

    [Header("Panel")]
    [SerializeField]
    private GameObject panel;

    // =====================================================
    // CONTENT
    // =====================================================

    [Header("Content")]
    [SerializeField]
    private Transform content;

    [SerializeField]
    private InventorySkillButton skillButtonPrefab;

    // =====================================================
    // LEFT PANEL
    // =====================================================

    [Header("Selected Skill Detail")]
    [SerializeField]
    private GameObject leftInventorySkill;

    [SerializeField]
    private Image selectedIcon;

    [SerializeField]
    private TextMeshProUGUI selectedNameText;

    // =====================================================
    // ABILITY ACTION
    // =====================================================

    [Header("Ability Actions")]
    [SerializeField]
    private GameObject abilityActions;

    [SerializeField]
    private Button slot3Button;

    [SerializeField]
    private Button slot4Button;

    // =====================================================
    // ELEMENT ACTION
    // =====================================================

    [Header("Element Actions")]
    [SerializeField]
    private GameObject elementActions;

    [SerializeField]
    private Button equipElementButton;

    // =====================================================
    // UNEQUIP
    // =====================================================

    [Header("Unequip")]
    [SerializeField]
    private Button unequipButton;

    // =====================================================
    // INPUT
    // =====================================================

    [Header("Input")]
    [SerializeField]
    private KeyCode openKey =
        KeyCode.B;

    [SerializeField]
    private bool pauseGameWhenOpen = true;

    // =====================================================
    // OWNED SKILLS
    // =====================================================

    private readonly List<AbilityData>
        ownedAbilities =
            new List<AbilityData>();

    private readonly List<ElementData>
        ownedElements =
            new List<ElementData>();

    // =====================================================
    // RUNTIME
    // =====================================================

    private bool isOpen;

    private float previousTimeScale = 1f;

    private InventorySkillButton currentSelected;

    public bool IsOpen =>
        isOpen;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupButtons();
    }

    private void Start()
    {
        if (panel == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán Panel.",
                this
            );

            return;
        }

        if (panel == gameObject)
        {
            Debug.LogError(
                "Panel không được là object chứa SkillInventoryUI.",
                this
            );

            return;
        }

        ClearSelectedSkillPanel();

        isOpen = false;

        panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(
                openKey))
        {
            TogglePanel();
        }
    }

    // =====================================================
    // BUTTON LISTENERS
    // =====================================================

    private void SetupButtons()
    {
        if (slot3Button != null)
        {
            slot3Button.onClick.RemoveListener(
                EquipSelectedSlot3
            );

            slot3Button.onClick.AddListener(
                EquipSelectedSlot3
            );
        }

        if (slot4Button != null)
        {
            slot4Button.onClick.RemoveListener(
                EquipSelectedSlot4
            );

            slot4Button.onClick.AddListener(
                EquipSelectedSlot4
            );
        }

        if (equipElementButton != null)
        {
            equipElementButton.onClick.RemoveListener(
                EquipSelectedElement
            );

            equipElementButton.onClick.AddListener(
                EquipSelectedElement
            );
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                UnequipSelected
            );

            unequipButton.onClick.AddListener(
                UnequipSelected
            );
        }
    }

    // =====================================================
    // OPEN / CLOSE
    // =====================================================

    public void TogglePanel()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        if (isOpen)
            return;

        if (panel == null)
            return;

        isOpen = true;

        panel.SetActive(true);

        ClearSelection();

        RefreshButtons();

        if (pauseGameWhenOpen)
        {
            previousTimeScale =
                Time.timeScale;

            Time.timeScale = 0f;
        }

        Players player =
            FindPlayer();

        if (player != null)
        {
            player.LockControl();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryOpen();
        }
    }

    public void ClosePanel()
    {
        if (!isOpen)
            return;

        isOpen = false;

        ClearSelection();

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        UnlockPlayer();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryClose();
        }
    }

    // =====================================================
    // SELECT
    // =====================================================

    public void SelectButton(
        InventorySkillButton button)
    {
        if (button == null)
            return;

        if (currentSelected != null &&
            currentSelected != button)
        {
            currentSelected.SetSelected(
                false
            );
        }

        currentSelected =
            button;

        currentSelected.SetSelected(
            true
        );

        RefreshSelectedSkillPanel();
    }

    public void ClearSelection()
    {
        if (currentSelected != null)
        {
            currentSelected.SetSelected(
                false
            );
        }

        currentSelected = null;

        ClearSelectedSkillPanel();
    }

    // =====================================================
    // LEFT PANEL
    // =====================================================

    private void RefreshSelectedSkillPanel()
    {
        if (currentSelected == null)
        {
            ClearSelectedSkillPanel();
            return;
        }

        AbilityData ability =
            currentSelected.AbilityData;

        ElementData element =
            currentSelected.ElementData;

        // =============================================
        // ABILITY
        // =============================================

        if (ability != null)
        {
            if (leftInventorySkill != null)
            {
                leftInventorySkill.SetActive(
                    true
                );
            }

            if (selectedIcon != null)
            {
                selectedIcon.sprite =
                    ability.icon;

                selectedIcon.enabled =
                    ability.icon != null;

                selectedIcon.preserveAspect =
                    true;
            }

            if (selectedNameText != null)
            {
                selectedNameText.text =
                    ability.skillName;
            }

            if (abilityActions != null)
            {
                abilityActions.SetActive(
                    true
                );
            }

            if (elementActions != null)
            {
                elementActions.SetActive(
                    false
                );
            }

            RefreshUnequipButton();

            return;
        }

        // =============================================
        // ELEMENT
        // =============================================

        if (element != null)
        {
            if (leftInventorySkill != null)
            {
                leftInventorySkill.SetActive(
                    true
                );
            }

            if (selectedIcon != null)
            {
                selectedIcon.sprite =
                    element.elementIcon;

                selectedIcon.enabled =
                    element.elementIcon != null;

                selectedIcon.preserveAspect =
                    true;
            }

            if (selectedNameText != null)
            {
                selectedNameText.text =
                    element.elementName;
            }

            if (abilityActions != null)
            {
                abilityActions.SetActive(
                    false
                );
            }

            if (elementActions != null)
            {
                elementActions.SetActive(
                    true
                );
            }

            RefreshUnequipButton();

            return;
        }

        ClearSelectedSkillPanel();
    }

    private void ClearSelectedSkillPanel()
    {
        if (selectedIcon != null)
        {
            selectedIcon.sprite = null;

            selectedIcon.enabled = false;
        }

        if (selectedNameText != null)
        {
            selectedNameText.text = "";
        }

        if (abilityActions != null)
        {
            abilityActions.SetActive(
                false
            );
        }

        if (elementActions != null)
        {
            elementActions.SetActive(
                false
            );
        }

        if (unequipButton != null)
        {
            unequipButton.gameObject
                .SetActive(
                    false
                );
        }
    }

    // =====================================================
    // EQUIP ABILITY
    // =====================================================

    public void EquipSelectedSlot3()
    {
        EquipSelectedAbility(
            3
        );
    }

    public void EquipSelectedSlot4()
    {
        EquipSelectedAbility(
            4
        );
    }

    private void EquipSelectedAbility(
        int slot)
    {
        if (currentSelected == null)
            return;

        AbilityData ability =
            currentSelected.AbilityData;

        if (ability == null)
            return;

        if (EquipmentManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy EquipmentManager."
            );

            return;
        }

        bool success =
            EquipmentManager.Instance
                .EquipSkill(
                    ability,
                    slot
                );

        if (!success)
            return;

        PlayEquipSound();

        RefreshAllButtonStatus();

        RefreshSelectedSkillPanel();
    }

    // =====================================================
    // EQUIP ELEMENT
    // =====================================================

    public void EquipSelectedElement()
    {
        if (currentSelected == null)
            return;

        ElementData element =
            currentSelected.ElementData;

        if (element == null)
            return;

        if (ElementEquipmentManager.Instance ==
            null)
        {
            Debug.LogError(
                "Không tìm thấy ElementEquipmentManager."
            );

            return;
        }

        bool success =
            ElementEquipmentManager.Instance
                .EquipElementToSlot(
                    element,
                    5
                );

        if (!success)
            return;

        PlayEquipSound();

        RefreshAllButtonStatus();

        RefreshSelectedSkillPanel();
    }

    // =====================================================
    // UNEQUIP
    // =====================================================

    public void UnequipSelected()
    {
        if (currentSelected == null)
            return;

        bool changed = false;

        AbilityData ability =
            currentSelected.AbilityData;

        ElementData element =
            currentSelected.ElementData;

        // =============================================
        // ABILITY
        // =============================================

        if (ability != null &&
            EquipmentManager.Instance != null)
        {
            int slot =
                EquipmentManager.Instance
                    .GetEquippedSlot(
                        ability
                    );

            if (slot >= 0)
            {
                EquipmentManager.Instance
                    .UnequipSkill(
                        slot
                    );

                changed = true;
            }
        }

        // =============================================
        // ELEMENT
        // =============================================

        else if (
            element != null &&
            ElementEquipmentManager.Instance != null)
        {
            if (ElementEquipmentManager.Instance
                .IsElementEquipped(
                    element))
            {
                ElementEquipmentManager.Instance
                    .UnequipElement();

                changed = true;
            }
        }

        if (!changed)
            return;

        PlayEquipSound();

        RefreshAllButtonStatus();

        RefreshSelectedSkillPanel();
    }

    // =====================================================
    // UNEQUIP BUTTON STATE
    // =====================================================

    private void RefreshUnequipButton()
    {
        if (unequipButton == null)
            return;

        bool equipped = false;

        if (currentSelected != null)
        {
            AbilityData ability =
                currentSelected.AbilityData;

            ElementData element =
                currentSelected.ElementData;

            if (ability != null &&
                EquipmentManager.Instance != null)
            {
                equipped =
                    EquipmentManager.Instance
                        .GetEquippedSlot(
                            ability
                        ) >= 0;
            }
            else if (
                element != null &&
                ElementEquipmentManager.Instance !=
                null)
            {
                equipped =
                    ElementEquipmentManager.Instance
                        .IsElementEquipped(
                            element
                        );
            }
        }

        unequipButton.gameObject
            .SetActive(
                equipped
            );
    }

    // =====================================================
    // OWNED ABILITY
    // =====================================================

    public bool AddAbility(
        AbilityData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "AbilityData truyền vào AddAbility đang null."
            );

            return false;
        }

        if (!ownedAbilities.Contains(
                data))
        {
            ownedAbilities.Add(
                data
            );

            Debug.Log(
                $"Đã thêm Ability: {data.skillName}"
            );
        }

        if (isOpen)
        {
            RefreshButtons();
        }

        return true;
    }

    public void AddSkill(
        AbilityData data)
    {
        AddAbility(
            data
        );
    }

    public bool HasAbility(
        AbilityData data)
    {
        return data != null &&
               ownedAbilities.Contains(
                   data
               );
    }

    // =====================================================
    // OWNED ELEMENT
    // =====================================================

    public bool AddElement(
        ElementData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "ElementData truyền vào AddElement đang null."
            );

            return false;
        }

        if (!ownedElements.Contains(
                data))
        {
            ownedElements.Add(
                data
            );

            Debug.Log(
                $"Đã thêm Element: {data.elementName}"
            );
        }

        if (isOpen)
        {
            RefreshButtons();
        }

        return true;
    }

    public bool HasElement(
        ElementData data)
    {
        return data != null &&
               ownedElements.Contains(
                   data
               );
    }

    // =====================================================
    // REFRESH BUTTONS
    // =====================================================

    public void RefreshButtons()
    {
        if (content == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán Content."
            );

            return;
        }

        if (skillButtonPrefab == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán Skill Button Prefab."
            );

            return;
        }

        ClearSelection();

        ClearButtons();

        foreach (
            AbilityData ability
            in ownedAbilities)
        {
            if (ability == null)
                continue;

            InventorySkillButton button =
                Instantiate(
                    skillButtonPrefab,
                    content
                );

            button.SetupAbility(
                ability
            );
        }

        foreach (
            ElementData element
            in ownedElements)
        {
            if (element == null)
                continue;

            InventorySkillButton button =
                Instantiate(
                    skillButtonPrefab,
                    content
                );

            button.SetupElement(
                element
            );
        }
    }

    private void ClearButtons()
    {
        if (content == null)
            return;

        for (int i =
                 content.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                content.GetChild(i)
                    .gameObject
            );
        }
    }

    private void RefreshAllButtonStatus()
    {
        if (content == null)
            return;

        InventorySkillButton[] buttons =
            content.GetComponentsInChildren<
                InventorySkillButton
            >(
                true
            );

        foreach (
            InventorySkillButton button
            in buttons)
        {
            if (button != null)
            {
                button.RefreshStatus();
            }
        }
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayEquipSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryEquip();
        }
    }

    // =====================================================
    // PLAYER
    // =====================================================

    private Players FindPlayer()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            return GameManager.Instance.Player
                .GetComponent<Players>();
        }

        return FindFirstObjectByType<
            Players
        >();
    }

    private void UnlockPlayer()
    {
        bool sceneLoading =
            SceneLoader.Instance != null &&
            SceneLoader.Instance.IsLoading;

        if (sceneLoading)
            return;

        Players player =
            FindPlayer();

        if (player == null)
            return;

        Health health =
            player.GetComponent<Health>();

        if (health != null &&
            health.IsDead)
        {
            return;
        }

        player.UnlockControl();
    }

    // =====================================================
    // DISABLE / DESTROY
    // =====================================================

    private void OnDisable()
    {
        if (isOpen &&
            pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        isOpen = false;
    }

    private void OnDestroy()
    {
        if (slot3Button != null)
        {
            slot3Button.onClick.RemoveListener(
                EquipSelectedSlot3
            );
        }

        if (slot4Button != null)
        {
            slot4Button.onClick.RemoveListener(
                EquipSelectedSlot4
            );
        }

        if (equipElementButton != null)
        {
            equipElementButton.onClick.RemoveListener(
                EquipSelectedElement
            );
        }

        if (unequipButton != null)
        {
            unequipButton.onClick.RemoveListener(
                UnequipSelected
            );
        }

        if (isOpen &&
            pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}