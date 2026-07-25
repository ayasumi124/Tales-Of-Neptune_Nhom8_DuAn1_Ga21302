using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public Image cooldownMask;
    public Image durationMask;

    public TextMeshProUGUI cooldownText;

    public Image keyIcon;

    public AbilityType abilityType;

    [HideInInspector]
    public AbilityData skillData;



    void Start()
    {
        if (skillData != null)
        {
            icon.sprite = skillData.icon;
        }
    }
    void Update()
    {
        UpdateCooldown();
        UpdateDuration();
    }

    public void Setup(AbilityData data)
{
    skillData = data;

    if (icon != null)
        icon.sprite = data.icon;
}

    void UpdateCooldown()
    {
        if (skillData == null)
            return;

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(abilityType);

        if (state == null)
            return;

        if (state.cooldown > 0)
        {
            cooldownMask.fillAmount =
                state.cooldown / skillData.cooldown;

            cooldownText.text =
                Mathf.Ceil(state.cooldown).ToString();
        }
        else
        {
            cooldownMask.fillAmount = 0;
            cooldownText.text = "";
        }
    }

    void UpdateDuration()
    {
        if (skillData == null)
            return;

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(abilityType);

        if (state == null)
            return;

        if (state.duration > 0)
        {
            durationMask.fillAmount =
                state.duration / skillData.duration;
        }
        else
        {
            durationMask.fillAmount = 0;
        }
    }
}