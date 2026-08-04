using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private Image cooldownMask;
    [SerializeField] private Image durationMask;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private Image keyIcon;

    [Header("Skill")]
    [SerializeField] private AbilityType abilityType;

    [HideInInspector]
    public AbilityData skillData;

    private void Start()
    {
        if (skillData != null)
        {
            ShowSkill(skillData);
        }
        else
        {
            Clear();
        }
    }

    private void Update()
    {
        UpdateCooldown();
        UpdateDuration();
    }

    public void ShowSkill(AbilityData data)
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

        if (keyIcon != null)
        {
            keyIcon.gameObject.SetActive(true);
            keyIcon.enabled = true;
        }

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject.SetActive(false);
        }

        if (durationMask != null)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject.SetActive(false);
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }

    private void UpdateCooldown()
    {
        if (cooldownMask == null ||
            cooldownText == null)
        {
            return;
        }

        if (skillData == null ||
            AbilityManager.Instance == null)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject.SetActive(false);
            cooldownText.text = "";
            return;
        }

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(
                abilityType
            );

        if (state == null ||
            state.cooldown <= 0f)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject.SetActive(false);
            cooldownText.text = "";
            return;
        }

        float maxCooldown =
            state.maxCooldown > 0f
                ? state.maxCooldown
                : skillData.cooldown;

        if (maxCooldown <= 0f)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject.SetActive(false);
            cooldownText.text = "";
            return;
        }

        cooldownMask.gameObject.SetActive(true);

        cooldownMask.fillAmount =
            Mathf.Clamp01(
                state.cooldown /
                maxCooldown
            );

        cooldownText.text =
            Mathf.CeilToInt(
                state.cooldown
            ).ToString();
    }

    private void UpdateDuration()
    {
        if (durationMask == null)
            return;

        if (skillData == null ||
            AbilityManager.Instance == null)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject.SetActive(false);
            return;
        }

        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(
                abilityType
            );

        if (state == null ||
            state.duration <= 0f)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject.SetActive(false);
            return;
        }

        float totalDuration =
            state.maxDuration > 0f
                ? state.maxDuration
                : skillData.duration;

        if (totalDuration <= 0f)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject.SetActive(false);
            return;
        }

        durationMask.gameObject.SetActive(true);

        durationMask.fillAmount =
            Mathf.Clamp01(
                state.duration /
                totalDuration
            );
    }

    public void Setup(AbilityData data)
{
    ShowSkill(data);
}
    public void Clear()
    {
        skillData = null;

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
            icon.gameObject.SetActive(false);
        }

        if (keyIcon != null)
        {
            keyIcon.enabled = false;
            keyIcon.gameObject.SetActive(false);
        }

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject.SetActive(false);
        }

        if (durationMask != null)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject.SetActive(false);
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }
}