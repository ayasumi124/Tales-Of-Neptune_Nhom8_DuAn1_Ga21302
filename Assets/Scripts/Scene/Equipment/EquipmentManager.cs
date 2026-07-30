using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    public SkillSlotUI slot3;
    public SkillSlotUI slot4;
    public SkillSlotUI slot5;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        slot3.Clear();
        slot4.Clear();
        slot5.Clear();
        slot3.gameObject.SetActive(false);
        slot4.gameObject.SetActive(false);
        slot5.gameObject.SetActive(false);
    }
    public void EquipSkill(AbilityData data, int slot)
    {
        switch (slot)
        {
            case 3:
                slot3.gameObject.SetActive(true);
                slot3.Setup(data);
                break;

            case 4:
                slot4.gameObject.SetActive(true);
                slot4.Setup(data);
                break;

            case 5:
                slot5.gameObject.SetActive(true);
                slot5.Setup(data);
                break;
        }
    }

    public void UnequipSkill(int slot)
    {
        switch (slot)
        {
            case 3:
                slot3.Clear();
                slot3.gameObject.SetActive(false);
                break;

            case 4:
                slot4.Clear();
                slot4.gameObject.SetActive(false);
                break;

            case 5:
                slot5.Clear();
                slot5.gameObject.SetActive(false);
                break;
        }
    }
}