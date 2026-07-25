using UnityEngine;

public class SkillInventoryUI : MonoBehaviour
{
    public static SkillInventoryUI Instance;

    public GameObject panel;

    [Header("Inventory")]
    public Transform content;
    public GameObject inventorySkillPrefab;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            panel.SetActive(!panel.activeSelf);
        }
    }

    public void AddSkill(AbilityData data)
    {
        GameObject obj =
            Instantiate(inventorySkillPrefab, content);

        obj.GetComponent<InventorySkillButton>()
            .Setup(data);
    }
}