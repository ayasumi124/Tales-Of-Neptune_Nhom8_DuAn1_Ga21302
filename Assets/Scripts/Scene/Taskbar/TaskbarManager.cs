using UnityEngine;

public class TaskbarManager : MonoBehaviour
{
    public SkillSlotUI[] slots;

    void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (SkillSlotUI slot in slots)
        {
            if (slot.skillData == null)
                continue;

            slot.gameObject.SetActive(
                AbilityManager.Instance.HasAbility(slot.skillData.type));
        }
    }
    void Update()
    {
        foreach (SkillSlotUI slot in slots)
        {
            if (slot.skillData == null)
                continue;

            bool unlocked =
                AbilityManager.Instance.HasAbility(slot.skillData.type);

            slot.gameObject.SetActive(unlocked);
        }
    }
}