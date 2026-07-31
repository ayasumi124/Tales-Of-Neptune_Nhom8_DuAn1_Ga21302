using UnityEngine;

public class SkillInventoryUI : MonoBehaviour
{
    public static SkillInventoryUI Instance { get; private set; }

    public GameObject panel;

    [Header("Inventory")]
    public Transform content;
    public GameObject inventorySkillPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Phát hiện SkillInventoryUI bị trùng. Xóa object: " +
                gameObject.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }
    }

    public void AddSkill(AbilityData data)
    {
        if (data == null)
        {
            Debug.LogError("AbilityData truyền vào AddSkill đang null.");
            return;
        }

        if (inventorySkillPrefab == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán inventorySkillPrefab."
            );
            return;
        }

        if (content == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán content."
            );
            return;
        }

        GameObject obj = Instantiate(
            inventorySkillPrefab,
            content
        );

        InventorySkillButton button =
            obj.GetComponent<InventorySkillButton>();

        if (button != null)
        {
            button.Setup(data);
        }
        else
        {
            Debug.LogError(
                "Prefab inventorySkillPrefab không có InventorySkillButton."
            );
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}