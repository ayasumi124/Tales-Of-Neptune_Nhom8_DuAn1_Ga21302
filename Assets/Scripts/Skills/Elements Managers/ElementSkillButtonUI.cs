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
    private IceSkillController iceController;

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
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            GameObject player =
                GameManager.Instance.Player;

            if (fireController == null)
            {
                fireController =
                    player.GetComponent<
                        FireSkillController
                    >();
            }

            if (iceController == null)
            {
                iceController =
                    player.GetComponent<
                        IceSkillController
                    >();
            }
        }

        if (fireController == null)
        {
            fireController =
                FindFirstObjectByType<
                    FireSkillController
                >();
        }

        if (iceController == null)
        {
            iceController =
                FindFirstObjectByType<
                    IceSkillController
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

    FindController();

    float duration = 0f;
    float cooldown = 0f;
    float remainingCooldown = 0f;

    switch (skillData.elementType)
    {
        case ElementType.Fire:

            if (fireController == null)
            {
                HideMasks();
                return;
            }

            duration =
                fireController
                    .GetDurationNormalized(
                        skillData
                    );

            cooldown =
                fireController
                    .GetCooldownNormalized(
                        skillData
                    );

            remainingCooldown =
                fireController
                    .GetRemainingCooldown(
                        skillData
                    );

            break;

        case ElementType.Ice:

            if (iceController == null)
            {
                HideMasks();
                return;
            }

            duration =
                iceController
                    .GetDurationNormalized(
                        skillData
                    );

            cooldown =
                iceController
                    .GetCooldownNormalized(
                        skillData
                    );

            remainingCooldown =
                iceController
                    .GetRemainingCooldown(
                        skillData
                    );

            break;

        default:

            HideMasks();
            return;
    }

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
        cooldownText.text =
            cooldown > 0f
                ? Mathf.CeilToInt(
                    remainingCooldown
                ).ToString()
                : "";
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