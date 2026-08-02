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

    [Header("Player")]
    [SerializeField] private Players player;

    private readonly List<InventorySlotUI>
        slotUIs = new List<InventorySlotUI>();

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

        if (AudioManager.Instance != null &&
            AudioManager.Instance.openInventorySound != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.openInventorySound
            );
        }

        Refresh();
    }

    public void CloseInventory()
    {
        isOpen = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (player != null)
            player.UnlockControl();
    }

    private void HideImmediate()
    {
        isOpen = false;

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
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
        if (InventoryManager.Instance == null)
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

        selectedSlotIndex =
            slotIndex;

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

        ItemData data =
            selectedItem.itemData;

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

        /*
         * Đóng Inventory để thấy animation
         * Player uống bình.
         */
        CloseInventory();

        Refresh();
    }

    private void ClearItemInfo()
    {
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
            useButton.gameObject.SetActive(false);
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

        if (Instance == this)
            Instance = null;
    }
}