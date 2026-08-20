using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance
    {
        get;
        private set;
    }

    // =====================================================
    // MAIN
    // =====================================================

    [Header("Main")]
    [SerializeField]
    private GameObject inventoryPanel;

    [SerializeField]
    private KeyCode inventoryKey =
        KeyCode.I;

    // =====================================================
    // GRID
    // =====================================================

    [Header("Grid")]
    [SerializeField]
    private Transform slotContainer;

    [SerializeField]
    private InventorySlotUI slotPrefab;

    // =====================================================
    // ITEM INFORMATION
    // =====================================================

    [Header("Item Information")]
    [SerializeField]
    private Image selectedItemIcon;

    [SerializeField]
    private TextMeshProUGUI selectedItemName;

    [SerializeField]
    private TextMeshProUGUI selectedItemDescription;

    [SerializeField]
    private TextMeshProUGUI selectedItemQuantity;

    // =====================================================
    // NORMAL BUTTONS
    // =====================================================

    [Header("Buttons")]
    [SerializeField]
    private Button useButton;

    [SerializeField]
    private Button equipShortcutButton;

    [SerializeField]
    private Button sortButton;

    [SerializeField]
    private Button dropOneButton;

    [SerializeField]
    private Button dropStackButton;

    // =====================================================
    // EQUIPMENT BUTTONS
    // =====================================================

    [Header("Equipment Buttons")]
    [SerializeField]
    private Button equipmentButton;

    [SerializeField]
    private Button unequipEquipmentButton;

    // =====================================================
    // EQUIPMENT ICONS
    // =====================================================

    [Header("Equipment Icons")]
    [Tooltip(
        "Kéo SwordSlot/EquipmentIcon vào đây."
    )]
    [SerializeField]
    private Image weaponEquipmentIcon;

    [Tooltip(
        "Kéo HelmetSlot/EquipmentIcon vào đây."
    )]
    [SerializeField]
    private Image helmetEquipmentIcon;

    [Tooltip(
        "Kéo ArmorSlot/EquipmentIcon vào đây."
    )]
    [SerializeField]
    private Image armorEquipmentIcon;

    [Header("Equipment Type Icons")]
    [SerializeField]
    private GameObject weaponTypeIcon;

    [SerializeField]
    private GameObject helmetTypeIcon;

    [SerializeField]
    private GameObject armorTypeIcon;

    // =====================================================
    // PLAYER
    // =====================================================

    [Header("Player")]
    [SerializeField]
    private Players player;

    // =====================================================
    // RUNTIME
    // =====================================================

    private readonly List<InventorySlotUI>
        slotUIs =
            new List<InventorySlotUI>();

    private int selectedSlotIndex =
        -1;

    private bool isOpen;

    private float previousTimeScale =
        1f;

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

        SetupButtonListeners();

        HideImmediate();
    }

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged +=
            Refresh;

        PlayerEquipmentManager
            .OnEquipmentChanged +=
            HandleEquipmentChanged;
    }

    private void Start()
    {
        FindPlayer();

        CreateSlots();

        Refresh();

        RefreshEquipmentUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(
                inventoryKey))
        {
            ToggleInventory();
        }
    }

    // =====================================================
    // BUTTON LISTENERS
    // =====================================================

    private void SetupButtonListeners()
    {
        if (useButton != null)
        {
            useButton.onClick.RemoveListener(
                UseSelectedItem
            );

            useButton.onClick.AddListener(
                UseSelectedItem
            );
        }

        if (equipShortcutButton != null)
        {
            equipShortcutButton.onClick
                .RemoveListener(
                    EquipSelectedItemToShortcut
                );

            equipShortcutButton.onClick
                .AddListener(
                    EquipSelectedItemToShortcut
                );
        }

        if (equipmentButton != null)
        {
            equipmentButton.onClick
                .RemoveListener(
                    EquipSelectedEquipment
                );

            equipmentButton.onClick
                .AddListener(
                    EquipSelectedEquipment
                );
        }

        if (unequipEquipmentButton != null)
        {
            unequipEquipmentButton.onClick
                .RemoveListener(
                    UnequipSelectedEquipment
                );

            unequipEquipmentButton.onClick
                .AddListener(
                    UnequipSelectedEquipment
                );
        }

        if (sortButton != null)
        {
            sortButton.onClick.RemoveListener(
                SortInventory
            );

            sortButton.onClick.AddListener(
                SortInventory
            );
        }

        if (dropOneButton != null)
        {
            dropOneButton.onClick
                .RemoveListener(
                    DropOneSelectedItem
                );

            dropOneButton.onClick
                .AddListener(
                    DropOneSelectedItem
                );
        }

        if (dropStackButton != null)
        {
            dropStackButton.onClick
                .RemoveListener(
                    DropSelectedStack
                );

            dropStackButton.onClick
                .AddListener(
                    DropSelectedStack
                );
        }
    }

    // =====================================================
    // OPEN / CLOSE
    // =====================================================

    public void ToggleInventory()
    {
        if (isOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        if (isOpen)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy InventoryManager."
            );

            return;
        }

        isOpen = true;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(
                true
            );
        }

        FindPlayer();

        if (player != null)
        {
            player.LockControl();
        }

        previousTimeScale =
            Time.timeScale;

        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryOpen();
        }

        Refresh();

        RefreshEquipmentUI();
    }

    public void CloseInventory(
        bool playCloseSound = true)
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (playCloseSound &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryClose();
        }

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(
                false
            );
        }

        Time.timeScale =
            previousTimeScale;

        if (player != null)
        {
            player.UnlockControl();
        }
    }

    // =====================================================
    // SORT
    // =====================================================

    public void SortInventory()
    {
        if (InventoryManager.Instance == null)
            return;

        selectedSlotIndex = -1;

        InventoryManager.Instance
            .SortInventory();

        ClearItemInfo();

        Refresh();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryMove();
        }
    }

    // =====================================================
    // DROP
    // =====================================================

    public void DropOneSelectedItem()
    {
        DropSelectedItem(
            false
        );
    }

    public void DropSelectedStack()
    {
        DropSelectedItem(
            true
        );
    }

    private void DropSelectedItem(
        bool entireStack)
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            return;
        }

        ItemData itemData =
            selectedItem.itemData;

        if (itemData == null)
            return;

        // =================================================
        // EQUIPMENT ĐANG MẶC
        // -> UNEQUIP TRƯỚC KHI DROP
        // =================================================

        if (itemData.Equippable &&
            PlayerEquipmentManager.Instance != null &&
            PlayerEquipmentManager.Instance
                .IsEquipped(
                    itemData
                ))
        {
            PlayerEquipmentManager.Instance
                .Unequip(
                    itemData.EquipmentType
                );

            Debug.Log(
                $"{itemData.ItemName} đang được equip. " +
                "Đã tự Unequip trước khi Drop."
            );
        }

        // =================================================
        // DROP
        // =================================================

        bool dropped =
            entireStack
                ? InventoryManager.Instance
                    .DropEntireStack(
                        selectedSlotIndex
                    )
                : InventoryManager.Instance
                    .DropItemAt(
                        selectedSlotIndex,
                        1
                    );

        if (!dropped)
            return;

        selectedSlotIndex =
            -1;

        ClearItemInfo();

        Refresh();

        RefreshEquipmentUI();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryDrop();
        }
    }

    // =====================================================
    // ITEM SHORTCUT
    // =====================================================

    public void EquipSelectedItemToShortcut()
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            return;
        }

        ItemData itemData =
            selectedItem.itemData;

        if (itemData == null)
            return;

        if (ItemShortcutManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy ItemShortcutManager."
            );

            return;
        }

        bool equipped =
            ItemShortcutManager.Instance
                .EquipShortcut(
                    itemData
                );

        if (!equipped)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryShortcut();
        }

        Debug.Log(
            $"Đã trang bị Shortcut: " +
            $"{itemData.ItemName}"
        );

        RefreshSelectedItem();
    }

    // =====================================================
    // EQUIP EQUIPMENT
    // =====================================================

    public void EquipSelectedEquipment()
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            return;
        }

        ItemData data =
            selectedItem.itemData;

        if (data == null)
            return;

        if (!data.Equippable)
        {
            Debug.Log(
                $"{data.ItemName} không phải Equipment."
            );

            return;
        }

        if (PlayerEquipmentManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy PlayerEquipmentManager."
            );

            return;
        }

        bool success =
            PlayerEquipmentManager.Instance
                .Equip(
                    data
                );

        if (!success)
            return;

        Debug.Log(
            $"InventoryUI: Equip thành công " +
            $"{data.ItemName}"
        );

        /*
         * PlayerEquipmentManager sẽ
         * gọi OnEquipmentChanged.
         *
         * Nhưng gọi trực tiếp thêm một lần
         * cũng không gây vấn đề.
         */
        RefreshEquipmentUI();

        /*
         * EQUIP biến mất,
         * UNEQUIP xuất hiện.
         */
        RefreshSelectedItem();
    }

    // =====================================================
    // UNEQUIP EQUIPMENT
    // =====================================================

    public void UnequipSelectedEquipment()
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            return;
        }

        ItemData data =
            selectedItem.itemData;

        if (data == null ||
            !data.Equippable)
        {
            return;
        }

        if (PlayerEquipmentManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy PlayerEquipmentManager."
            );

            return;
        }

        if (!PlayerEquipmentManager.Instance
                .IsEquipped(
                    data
                ))
        {
            return;
        }

        PlayerEquipmentManager.Instance
            .Unequip(
                data.EquipmentType
            );

        Debug.Log(
            $"InventoryUI: Unequip " +
            $"{data.ItemName}"
        );

        RefreshEquipmentUI();

        /*
         * UNEQUIP biến mất,
         * EQUIP xuất hiện.
         */
        RefreshSelectedItem();
    }

    // =====================================================
    // EQUIPMENT EVENT
    // =====================================================

    private void HandleEquipmentChanged()
    {
        /*
         * Refresh toàn Inventory để:
         *
         * - chữ E trong InventorySlotUI cập nhật
         * - EquipmentIcon bên trái cập nhật
         * - nút Equip/Unequip cập nhật
         */
        Refresh();
    }

    // =====================================================
    // CREATE INVENTORY SLOTS
    // =====================================================

    private void CreateSlots()
    {
        if (InventoryManager.Instance == null ||
            slotContainer == null ||
            slotPrefab == null)
        {
            return;
        }

        foreach (
            Transform child
            in slotContainer)
        {
            Destroy(
                child.gameObject
            );
        }

        slotUIs.Clear();

        int slotCount =
            InventoryManager.Instance
                .InventorySize;

        for (int i = 0;
             i < slotCount;
             i++)
        {
            InventorySlotUI newSlot =
                Instantiate(
                    slotPrefab,
                    slotContainer,
                    false
                );

            RectTransform slotRect =
                newSlot.GetComponent<
                    RectTransform
                >();

            if (slotRect != null)
            {
                slotRect.localScale =
                    Vector3.one;

                slotRect.localRotation =
                    Quaternion.identity;

                slotRect.sizeDelta =
                    new Vector2(
                        64f,
                        64f
                    );
            }

            newSlot.Initialize(
                this,
                i
            );

            slotUIs.Add(
                newSlot
            );
        }
    }

    // =====================================================
    // REFRESH INVENTORY
    // =====================================================

    public void Refresh()
    {
        if (InventoryManager.Instance == null)
            return;

        if (slotUIs.Count !=
            InventoryManager.Instance
                .InventorySize)
        {
            CreateSlots();
        }

        for (int i = 0;
             i < slotUIs.Count;
             i++)
        {
            InventoryItem item =
                InventoryManager.Instance
                    .GetItemAt(i);

            slotUIs[i].Display(
                item
            );

            slotUIs[i].SetSelected(
                i ==
                selectedSlotIndex
            );
        }

        RefreshSelectedItem();

        RefreshEquipmentUI();
    }

    // =====================================================
    // SELECT SLOT
    // =====================================================

    public void SelectSlot(
        int slotIndex)
    {
        if (InventoryManager.Instance == null)
            return;

        int previousSelected =
            selectedSlotIndex;

        /*
         * Click lại slot đang chọn:
         * giữ nguyên selection.
         */
        if (selectedSlotIndex ==
            slotIndex)
        {
            RefreshSelectedItem();
            return;
        }

        InventoryItem item =
            InventoryManager.Instance
                .GetItemAt(
                    slotIndex
                );

        if (item == null ||
            item.IsEmpty())
        {
            selectedSlotIndex =
                -1;

            ClearItemInfo();

            RefreshSlotSelection();

            return;
        }

        selectedSlotIndex =
            slotIndex;

        if (previousSelected !=
                selectedSlotIndex &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventorySelect();
        }

        RefreshSlotSelection();

        RefreshSelectedItem();
    }

    private void RefreshSlotSelection()
    {
        for (int i = 0;
             i < slotUIs.Count;
             i++)
        {
            slotUIs[i].SetSelected(
                i ==
                selectedSlotIndex
            );
        }
    }

    // =====================================================
    // SELECTED ITEM INFO
    // =====================================================

    private void RefreshSelectedItem()
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            ClearItemInfo();
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            selectedSlotIndex =
                -1;

            ClearItemInfo();

            RefreshSlotSelection();

            return;
        }

        ItemData data =
            selectedItem.itemData;

        if (data == null)
        {
            ClearItemInfo();
            return;
        }

        // =================================================
        // DROP BUTTONS
        // =================================================

        if (dropOneButton != null)
        {
            dropOneButton.gameObject
                .SetActive(
                    true
                );

            dropOneButton.interactable =
                true;
        }

        if (dropStackButton != null)
        {
            bool canDropStack =
                selectedItem.quantity > 1;

            dropStackButton.gameObject
                .SetActive(
                    canDropStack
                );

            dropStackButton.interactable =
                canDropStack;
        }

        // =================================================
        // ICON
        // =================================================

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite =
                data.Icon;

            selectedItemIcon.enabled =
                data.Icon != null;

            Color color =
                selectedItemIcon.color;

            color.a = 1f;

            selectedItemIcon.color =
                color;
        }

        // =================================================
        // NAME
        // =================================================

        if (selectedItemName != null)
        {
            selectedItemName.text =
                data.ItemName;
        }

        // =================================================
        // DESCRIPTION
        // =================================================

        if (selectedItemDescription != null)
        {
            selectedItemDescription.text =
                data.Description;
        }

        // =================================================
        // QUANTITY
        // =================================================

        if (selectedItemQuantity != null)
        {
            selectedItemQuantity.text =
                $"Quantity: " +
                $"{selectedItem.quantity}";
        }

        // =================================================
        // USE
        // =================================================

        if (useButton != null)
        {
            bool canUse =
                data.Usable;

            useButton.gameObject
                .SetActive(
                    canUse
                );

            useButton.interactable =
                canUse;
        }

        // =================================================
        // SHORTCUT
        // =================================================

        if (equipShortcutButton != null)
        {
            bool canEquipShortcut =
                data.Usable &&
                data.ItemType ==
                ItemType.Consumable;

            equipShortcutButton.gameObject
                .SetActive(
                    canEquipShortcut
                );

            equipShortcutButton.interactable =
                canEquipShortcut;
        }

        // =================================================
        // EQUIPMENT STATE
        // =================================================

        bool isEquipment =
            data.Equippable;

        bool isEquipped =
            false;

        if (isEquipment &&
            PlayerEquipmentManager.Instance != null)
        {
            isEquipped =
                PlayerEquipmentManager.Instance
                    .IsEquipped(
                        data
                    );
        }

        // =================================================
        // EQUIP BUTTON
        // =================================================

        if (equipmentButton != null)
        {
            bool showEquip =
                isEquipment &&
                !isEquipped;

            equipmentButton.gameObject
                .SetActive(
                    showEquip
                );

            equipmentButton.interactable =
                showEquip;
        }

        // =================================================
        // UNEQUIP BUTTON
        // =================================================

        if (unequipEquipmentButton != null)
        {
            bool showUnequip =
                isEquipment &&
                isEquipped;

            unequipEquipmentButton.gameObject
                .SetActive(
                    showUnequip
                );

            unequipEquipmentButton.interactable =
                showUnequip;
        }
    }

    // =====================================================
    // USE ITEM
    // =====================================================

    public void UseSelectedItem()
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

        InventoryItem selectedItem =
            InventoryManager.Instance
                .GetItemAt(
                    selectedSlotIndex
                );

        if (selectedItem == null ||
            selectedItem.IsEmpty())
        {
            return;
        }

        bool used =
            InventoryManager.Instance
                .UseItemAt(
                    selectedSlotIndex
                );

        if (!used)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryUse();
        }

        CloseInventory();

        Refresh();
    }

    // =====================================================
    // EQUIPMENT UI
    // =====================================================

    private void RefreshEquipmentUI()
    {
        if (PlayerEquipmentManager.Instance == null)
        {
            SetEquipmentSlot(
                weaponEquipmentIcon,
                weaponTypeIcon,
                null
            );

            SetEquipmentSlot(
                helmetEquipmentIcon,
                helmetTypeIcon,
                null
            );

            SetEquipmentSlot(
                armorEquipmentIcon,
                armorTypeIcon,
                null
            );

            return;
        }

        SetEquipmentSlot(
            weaponEquipmentIcon,
            weaponTypeIcon,
            PlayerEquipmentManager.Instance
                .EquippedWeapon
        );

        SetEquipmentSlot(
            helmetEquipmentIcon,
            helmetTypeIcon,
            PlayerEquipmentManager.Instance
                .EquippedHelmet
        );

        SetEquipmentSlot(
            armorEquipmentIcon,
            armorTypeIcon,
            PlayerEquipmentManager.Instance
                .EquippedArmor
        );
    }

    private void SetEquipmentSlot(
        Image equipmentIcon,
        GameObject typeIcon,
        ItemData item)
    {
        bool equipped =
            item != null &&
            item.Icon != null;

        // =========================
        // TYPE ICON
        // =========================

        if (typeIcon != null)
        {
            typeIcon.SetActive(
                !equipped
            );
        }

        // =========================
        // EQUIPMENT ICON
        // =========================

        if (equipmentIcon == null)
            return;

        if (!equipped)
        {
            equipmentIcon.sprite = null;

            equipmentIcon.enabled = false;

            equipmentIcon.gameObject
                .SetActive(false);

            return;
        }

        equipmentIcon.gameObject
            .SetActive(true);

        equipmentIcon.sprite =
            item.Icon;

        equipmentIcon.enabled =
            true;

        equipmentIcon.preserveAspect =
            true;

        Color color =
            equipmentIcon.color;

        color.a = 1f;

        equipmentIcon.color =
            color;
    }

    private void SetEquipmentIcon(
        Image image,
        ItemData data)
    {
        if (image == null)
        {
            Debug.LogWarning(
                "Có EquipmentIcon chưa được " +
                "gán vào InventoryUI."
            );

            return;
        }

        /*
         * Không có Equipment:
         * tắt EquipmentIcon.
         *
         * TypeIcon phía dưới vẫn hiện.
         */
        if (data == null ||
            data.Icon == null)
        {
            image.sprite = null;

            image.enabled = false;

            image.gameObject.SetActive(
                false
            );

            return;
        }

        /*
         * Đây là fix quan trọng:
         *
         * Nếu EquipmentIcon trước đó inactive,
         * phải bật GameObject trở lại.
         */
        image.gameObject.SetActive(
            true
        );

        image.sprite =
            data.Icon;

        image.enabled =
            true;

        image.preserveAspect =
            true;

        Color color =
            image.color;

        color.a = 1f;

        image.color =
            color;

        /*
         * Đảm bảo icon nằm trên TypeIcon.
         */
        image.transform.SetAsLastSibling();

        Debug.Log(
            $"Equipment UI: " +
            $"{data.EquipmentType} -> " +
            $"{data.ItemName}"
        );
    }

    private void ClearEquipmentIcon(
        Image image)
    {
        if (image == null)
            return;

        image.sprite = null;

        image.enabled = false;

        image.gameObject.SetActive(
            false
        );
    }

    // =====================================================
    // CLEAR ITEM INFO
    // =====================================================

    private void ClearItemInfo()
    {
        if (dropOneButton != null)
        {
            dropOneButton.gameObject
                .SetActive(
                    false
                );
        }

        if (dropStackButton != null)
        {
            dropStackButton.gameObject
                .SetActive(
                    false
                );
        }

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite =
                null;

            selectedItemIcon.enabled =
                false;
        }

        if (selectedItemName != null)
        {
            selectedItemName.text =
                "";
        }

        if (selectedItemDescription != null)
        {
            selectedItemDescription.text =
                "";
        }

        if (selectedItemQuantity != null)
        {
            selectedItemQuantity.text =
                "";
        }

        if (useButton != null)
        {
            useButton.gameObject
                .SetActive(
                    false
                );
        }

        if (equipShortcutButton != null)
        {
            equipShortcutButton.gameObject
                .SetActive(
                    false
                );
        }

        if (equipmentButton != null)
        {
            equipmentButton.gameObject
                .SetActive(
                    false
                );
        }

        if (unequipEquipmentButton != null)
        {
            unequipEquipmentButton.gameObject
                .SetActive(
                    false
                );
        }
    }

    // =====================================================
    // PLAYER
    // =====================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player =
                GameManager.Instance.Player
                    .GetComponent<Players>();
        }

        if (player == null)
        {
            player =
                FindFirstObjectByType<
                    Players
                >();
        }
    }

    // =====================================================
    // HIDE
    // =====================================================

    private void HideImmediate()
    {
        isOpen = false;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(
                false
            );
        }
    }

    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -=
            Refresh;

        PlayerEquipmentManager
            .OnEquipmentChanged -=
            HandleEquipmentChanged;

        if (isOpen)
        {
            isOpen = false;

            Time.timeScale =
                previousTimeScale;

            if (player != null)
            {
                player.UnlockControl();
            }
        }
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -=
            Refresh;

        PlayerEquipmentManager
            .OnEquipmentChanged -=
            HandleEquipmentChanged;

        if (useButton != null)
        {
            useButton.onClick.RemoveListener(
                UseSelectedItem
            );
        }

        if (equipShortcutButton != null)
        {
            equipShortcutButton.onClick
                .RemoveListener(
                    EquipSelectedItemToShortcut
                );
        }

        if (equipmentButton != null)
        {
            equipmentButton.onClick
                .RemoveListener(
                    EquipSelectedEquipment
                );
        }

        if (unequipEquipmentButton != null)
        {
            unequipEquipmentButton.onClick
                .RemoveListener(
                    UnequipSelectedEquipment
                );
        }

        if (sortButton != null)
        {
            sortButton.onClick.RemoveListener(
                SortInventory
            );
        }

        if (dropOneButton != null)
        {
            dropOneButton.onClick
                .RemoveListener(
                    DropOneSelectedItem
                );
        }

        if (dropStackButton != null)
        {
            dropStackButton.onClick
                .RemoveListener(
                    DropSelectedStack
                );
        }

        if (isOpen)
        {
            Time.timeScale =
                previousTimeScale;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}