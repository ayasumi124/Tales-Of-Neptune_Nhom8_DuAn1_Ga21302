using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementSkillButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private TextMeshProUGUI keyText;
    [SerializeField] private TextMeshProUGUI masteryText;

    private ElementSkillData skillData;
    private int skillIndex;

    public ElementSkillData SkillData =>
        skillData;

    public bool IsUnlocked =>
        skillData != null &&
        skillData.unlocked;

    public void Setup(
        ElementSkillData data,
        int index)
    {
        skillData = data;
        skillIndex = index;

        gameObject.SetActive(true);

        if (keyText != null)
        {
            keyText.text =
                (skillIndex + 1).ToString();
        }

        if (skillIcon != null)
        {
            skillIcon.sprite =
                data != null
                    ? data.icon
                    : null;

            skillIcon.enabled =
                data != null;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (skillData == null)
        {
            SetLocked(true);

            if (masteryText != null)
                masteryText.text = "";

            return;
        }

        bool unlocked =
            skillData.unlocked;

        if (!unlocked &&
            ElementMasteryManager.Instance != null &&
            ElementMasteryManager.Instance
                .CanUnlock(skillData))
        {
            ElementMasteryManager.Instance
                .UnlockSkill(skillData);

            unlocked =
                skillData.unlocked;
        }

        SetLocked(!unlocked);

        if (masteryText != null)
        {
            masteryText.text =
                unlocked
                    ? ""
                    : skillData.requiredMastery
                        .ToString();
        }
    }

    private void SetLocked(bool locked)
    {
        if (lockOverlay != null)
        {
            lockOverlay.SetActive(locked);
        }

        if (skillIcon != null)
        {
            Color color =
                skillIcon.color;

            color.a =
                locked ? 0.4f : 1f;

            skillIcon.color = color;
        }
    }

    public void Clear()
    {
        skillData = null;

        if (skillIcon != null)
        {
            skillIcon.sprite = null;
            skillIcon.enabled = false;
        }

        if (keyText != null)
            keyText.text = "";

        if (masteryText != null)
            masteryText.text = "";

        if (lockOverlay != null)
            lockOverlay.SetActive(false);
    }
}