using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementSkillButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image lockOverlay;

    [SerializeField] private Image cooldownMask;
    [SerializeField] private Image durationMask;

    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private TextMeshProUGUI masteryText;
    [SerializeField] private TextMeshProUGUI keyText;

    private ElementSkillData skillData;
    private int skillIndex;

    private FireSkillController fireController;

    private void Update()
    {
        UpdateMasks();
    }

    public void Setup(
        ElementSkillData data,
        int index)
    {
        skillData = data;
        skillIndex = index;

        if (skillData == null)
        {
            Clear();
            return;
        }

        if (skillIcon != null)
        {
            skillIcon.enabled = true;
            skillIcon.sprite = skillData.icon;
        }

        if (keyText != null)
        {
            keyText.text =
                (index + 1).ToString();
        }

        FindController();
        Refresh();
    }

    private void FindController()
    {
        if (fireController != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            fireController =
                GameManager.Instance.Player
                    .GetComponent<
                        FireSkillController
                    >();
        }

        if (fireController == null)
        {
            fireController =
                FindFirstObjectByType<
                    FireSkillController
                >();
        }
    }

    private void UpdateMasks()
    {
        if (skillData == null)
        {
            HideMasks();
            return;
        }

        if (skillData.elementType !=
            ElementType.Fire)
        {
            HideMasks();
            return;
        }

        if (fireController == null)
            FindController();

        if (fireController == null)
        {
            HideMasks();
            return;
        }

        float duration =
            fireController.GetDurationNormalized(
                skillData
            );

        float cooldown =
            fireController.GetCooldownNormalized(
                skillData
            );

        // DurationMask hoạt động độc lập.
        if (durationMask != null)
        {
            bool showDuration =
                duration > 0f;

            durationMask.gameObject.SetActive(
                showDuration
            );

            durationMask.fillAmount =
                showDuration
                    ? duration
                    : 0f;
        }

        // CooldownMask cũng hoạt động độc lập.
        if (cooldownMask != null)
        {
            bool showCooldown =
                cooldown > 0f;

            cooldownMask.gameObject.SetActive(
                showCooldown
            );

            cooldownMask.fillAmount =
                showCooldown
                    ? cooldown
                    : 0f;
        }

        if (cooldownText != null)
        {
            if (cooldown > 0f)
            {
                float remaining =
                    fireController
                        .GetRemainingCooldown(
                            skillData
                        );

                cooldownText.text =
                    Mathf.CeilToInt(
                        remaining
                    ).ToString();
            }
            else
            {
                cooldownText.text = "";
            }
        }
    }
    private void HideMasks()
    {
        if (durationMask != null)
        {
            durationMask.fillAmount = 0f;
            durationMask.gameObject
                .SetActive(false);
        }

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0f;
            cooldownMask.gameObject
                .SetActive(false);
        }

        if (cooldownText != null)
            cooldownText.text = "";
    }

    public void Refresh()
    {
        if (skillData == null)
            return;

        if (lockOverlay != null)
        {
            lockOverlay.gameObject.SetActive(
                !skillData.unlocked
            );
        }

        if (masteryText != null)
        {
            masteryText.text =
                skillData.unlocked
                    ? ""
                    : skillData.requiredMastery
                        .ToString();
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

        HideMasks();

        if (lockOverlay != null)
            lockOverlay.gameObject.SetActive(false);

        if (masteryText != null)
            masteryText.text = "";

        if (keyText != null)
            keyText.text = "";
    }
}