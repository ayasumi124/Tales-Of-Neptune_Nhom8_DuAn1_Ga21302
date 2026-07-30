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

        if (data == null)
        {
            Clear();
            return;
        }

        gameObject.SetActive(true);

        abilityType = data.type;

        if (icon != null)
        {
            icon.gameObject.SetActive(true);
            icon.enabled = true;
            icon.sprite = data.icon;

            Color iconColor = icon.color;
            iconColor.a = 1f;
            icon.color = iconColor;
        }

        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

        if (durationMask != null)
            durationMask.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";
    }

    void UpdateCooldown()
    {
        if (skillData == null)
        {
            cooldownMask.fillAmount = 0;
            cooldownText.text = "";
            return;
        }

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(abilityType);

        if (state == null)
            return;

        if (state.cooldown > 0)
        {
            float maxCd =
                state.maxCooldown > 0
                ? state.maxCooldown
                : skillData.cooldown;

            cooldownMask.fillAmount =
                state.cooldown / maxCd;

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
        if (durationMask == null ||
            skillData == null ||
            AbilityManager.Instance == null)
            return;

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(abilityType);

        if (state == null || state.duration <= 0f)
        {
            durationMask.fillAmount = 0f;
            return;
        }

        float totalDuration = state.maxDuration > 0f
            ? state.maxDuration
            : skillData.duration;

        if (totalDuration <= 0f)
        {
            durationMask.fillAmount = 0f;
            return;
        }

        durationMask.fillAmount = Mathf.Clamp01(
            state.duration / totalDuration
        );
    }

    public void Clear()
    {
        skillData = null;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

        if (durationMask != null)
            durationMask.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";
    }
}