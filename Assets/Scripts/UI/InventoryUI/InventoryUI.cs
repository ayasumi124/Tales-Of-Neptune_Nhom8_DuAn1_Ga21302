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

    [Header("Main")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField]
    private KeyCode inventoryKey =
        KeyCode.I;

    [Header("Grid")]
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI slotPrefab;

    [Header("Item Information")]
    [SerializeField] private Image selectedItemIcon;
    [SerializeField] private TextMeshProUGUI selectedItemName;
    [SerializeField] private TextMeshProUGUI selectedItemDescription;
    [SerializeField] private TextMeshProUGUI selectedItemQuantity;

    [Header("Buttons")]
    [SerializeField] private Button useButton;
    [SerializeField]
    private Button equipShortcutButton;
    [SerializeField]
    private Button sortButton;

    [SerializeField]
    private Button dropOneButton;

    [SerializeField]
    private Button dropStackButton;

    [Header("Player")]
    [SerializeField] private Players player;

    private readonly List<InventorySlotUI>
        slotUIs = new List<InventorySlotUI>();
    private float previousTimeScale = 1f;

    private int selectedSlotIndex = -1;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {

        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


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
            dropOneButton.onClick.RemoveListener(
                DropOneSelectedItem
            );

            dropOneButton.onClick.AddListener(
                DropOneSelectedItem
            );
        }

        if (dropStackButton != null)
        {
            dropStackButton.onClick.RemoveListener(
                DropSelectedStack
            );

            dropStackButton.onClick.AddListener(
                DropSelectedStack
            );
        }

        HideImmediate();
    }

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged +=
            Refresh;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -=
            Refresh;

        /*
         * Tránh game bị kẹt Time.timeScale = 0
         * nếu InventoryUI bị disable khi đang mở.
         */
        if (isOpen)
        {
            isOpen = false;

            Time.timeScale = previousTimeScale;

            if (player != null)
                player.UnlockControl();
        }
    }

    private void Start()
    {
        FindPlayer();
        CreateSlots();
        Refresh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(
                inventoryKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (isOpen)
            CloseInventory();
        else
            OpenInventory();
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
            inventoryPanel.SetActive(true);

        FindPlayer();

        if (player != null)
            player.LockControl();

        /*
         * Lưu Time Scale trước đó để không phá
         * trạng thái pause của hệ thống khác.
         */
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInventoryOpen();
        }
        Refresh();
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
            inventoryPanel.SetActive(false);

        Time.timeScale = previousTimeScale;

        if (player != null)
            player.UnlockControl();
    }

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

    public void DropOneSelectedItem()
    {
        DropSelectedItem(false);
    }

    public void DropSelectedStack()
    {
        DropSelectedItem(true);
    }

    private void DropSelectedItem(
        bool entireStack)
    {
        if (InventoryManager.Instance == null ||
            selectedSlotIndex < 0)
        {
            return;
        }

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

        selectedSlotIndex = -1;

        ClearItemInfo();
        Refresh();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryDrop();
        }
    }

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
    private void CreateSlots()
    {
        if (InventoryManager.Instance == null ||
            slotContainer == null ||
            slotPrefab == null)
        {
            return;
        }

        foreach (Transform child
                 in slotContainer)
        {
            Destroy(child.gameObject);
        }

        slotUIs.Clear();

        int slotCount =
            InventoryManager.Instance.InventorySize;

        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI newSlot =
                Instantiate(
                    slotPrefab,
                    slotContainer,
                    false
                );

            RectTransform slotRect =
                newSlot.GetComponent<RectTransform>();

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

    public void Refresh()
    {
        if (InventoryManager.Instance == null)
            return;

        if (slotUIs.Count !=
            InventoryManager.Instance.InventorySize)
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

            slotUIs[i].Display(item);

            slotUIs[i].SetSelected(
                i == selectedSlotIndex
            );
        }

        RefreshSelectedItem();
    }

    public void SelectSlot(
        int slotIndex)
    {
        int previousSelected = selectedSlotIndex;
        if (InventoryManager.Instance == null)
            return;
        if (selectedSlotIndex == slotIndex)
            return;
        InventoryItem item =
            InventoryManager.Instance
                .GetItemAt(slotIndex);

        if (item == null ||
            item.IsEmpty())
        {
            selectedSlotIndex = -1;
            ClearItemInfo();
            RefreshSlotSelection();
            return;
        }

        selectedSlotIndex = slotIndex;

        if (previousSelected != selectedSlotIndex &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayInventorySelect();
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
                i == selectedSlotIndex
            );
        }
    }
    private void HideImmediate()
    {
        isOpen = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }
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
            selectedSlotIndex = -1;

            ClearItemInfo();
            RefreshSlotSelection();

            return;
        }
        if (dropOneButton != null)
        {
            dropOneButton.gameObject.SetActive(true);
            dropOneButton.interactable = true;
        }

        if (dropStackButton != null)
        {
            dropStackButton.gameObject.SetActive(
                selectedItem.quantity > 1
            );

            dropStackButton.interactable =
                selectedItem.quantity > 1;
        }
        ItemData data =
            selectedItem.itemData;

        if (data == null)
        {
            ClearItemInfo();
            return;
        }

        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite =
                data.Icon;

            selectedItemIcon.enabled =
                data.Icon != null;
        }

        if (selectedItemName != null)
        {
            selectedItemName.text =
                data.ItemName;
        }

        if (selectedItemDescription != null)
        {
            selectedItemDescription.text =
                data.Description;
        }

        if (selectedItemQuantity != null)
        {
            selectedItemQuantity.text =
                $"Quantity: {selectedItem.quantity}";
        }

        if (useButton != null)
        {
            useButton.gameObject.SetActive(
                data.Usable
            );

            useButton.interactable =
                data.Usable;
        }

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
    }
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
            AudioManager.Instance.PlayInventoryUse();
        }

        CloseInventory();

        Refresh();
    }

    private void ClearItemInfo()
    {
        if (dropOneButton != null)
        {
            dropOneButton.gameObject.SetActive(false);
        }

        if (dropStackButton != null)
        {
            dropStackButton.gameObject.SetActive(false);
        }
        if (selectedItemIcon != null)
        {
            selectedItemIcon.sprite = null;
            selectedItemIcon.enabled = false;
        }

        if (selectedItemName != null)
            selectedItemName.text = "";

        if (selectedItemDescription != null)
            selectedItemDescription.text = "";

        if (selectedItemQuantity != null)
            selectedItemQuantity.text = "";

        if (useButton != null)
        {
            useButton.gameObject.SetActive(
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
    }

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
                FindFirstObjectByType<Players>();
        }
    }

    private void OnDestroy()
    {
        InventoryManager.OnInventoryChanged -=
            Refresh;

        if (useButton != null)
        {
            useButton.onClick.RemoveListener(
                UseSelectedItem
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
            dropOneButton.onClick.RemoveListener(
                DropOneSelectedItem
            );
        }

        if (dropStackButton != null)
        {
            dropStackButton.onClick.RemoveListener(
                DropSelectedStack
            );
        }

        if (equipShortcutButton != null)
        {
            equipShortcutButton.onClick
                .RemoveListener(
                    EquipSelectedItemToShortcut
                );
        }

        if (isOpen)
        {
            Time.timeScale =
                previousTimeScale;
        }

        if (Instance == this)
            Instance = null;
    }

}