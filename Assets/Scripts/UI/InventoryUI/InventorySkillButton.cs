using UnityEngine;
using UnityEngine.UI;

public class InventorySkillButton : MonoBehaviour
{
    public Image icon;

    AbilityData data;

    public void Setup(AbilityData skill)
    {
        data = skill;

        icon.sprite = skill.icon;
    }

    public void OnClick()
    {
        Debug.Log(data.skillName);
    }
}