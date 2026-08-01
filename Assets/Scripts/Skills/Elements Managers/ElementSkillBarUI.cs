using UnityEngine;

public class ElementSkillBarUI : MonoBehaviour
{
    public static ElementSkillBarUI Instance
    {
        get;
        private set;
    }

    [Header("Panel")]
    [SerializeField]
    private GameObject skillBarPanel;

    [SerializeField]
    private RectTransform skillBarRect;

    [Header("Skill Buttons")]
    [SerializeField]
    private ElementSkillButtonUI[] skillButtons =
        new ElementSkillButtonUI[4];

    [Header("Taskbar Position")]
    [SerializeField]
    private RectTransform taskbar;

    [SerializeField]
    private float verticalOffset = 100f;

    private ElementData currentElement;

    public ElementData CurrentElement =>
        currentElement;

    public bool IsOpen =>
        skillBarPanel != null &&
        skillBarPanel.activeSelf;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Hide();
    }

    private void OnEnable()
    {
        ElementMasteryManager.OnMasteryChanged +=
            OnMasteryChanged;

        ElementMasteryManager.OnElementSkillUnlocked +=
            OnSkillUnlocked;
    }

    private void OnDisable()
    {
        ElementMasteryManager.OnMasteryChanged -=
            OnMasteryChanged;

        ElementMasteryManager.OnElementSkillUnlocked -=
            OnSkillUnlocked;
    }

    private void Start()
    {
        PositionAboveTaskbar();
    }

    private void Update()
    {
        if (currentElement == null ||
            !IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectSkill(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectSkill(1);
        }

        /*
         * Skill 3 chỉ cần gọi một lần khi vừa nhấn.
         * FireSkillController sẽ tự duy trì
         * khi phím 3 vẫn đang được giữ.
         */
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectSkill(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectSkill(3);
        }
    }

    public void ShowElement(
        ElementData element)
    {
        if (element == null)
            return;

        currentElement = element;

        PositionAboveTaskbar();

        if (skillBarPanel != null)
        {
            skillBarPanel.SetActive(true);
        }

        for (int i = 0;
             i < skillButtons.Length;
             i++)
        {
            if (skillButtons[i] == null)
                continue;

            ElementSkillData skill = null;

            if (element.skills != null &&
                i < element.skills.Length)
            {
                skill = element.skills[i];
            }

            skillButtons[i].Setup(
                skill,
                i
            );
        }

        Debug.Log(
            $"Đã mở thanh skill của " +
            $"{element.elementName}"
        );
    }

    public void Hide()
    {
        StopActiveChannelSkill();

        currentElement = null;

        if (skillBarPanel != null)
        {
            skillBarPanel.SetActive(false);
        }
    }

    private void SelectSkill(int index)
    {
        if (currentElement == null)
            return;

        if (currentElement.skills == null ||
            index < 0 ||
            index >= currentElement.skills.Length)
        {
            return;
        }

        ElementSkillData skill =
            currentElement.skills[index];

        if (skill == null)
        {
            Debug.LogWarning(
                $"Skill ở vị trí {index + 1} đang null."
            );

            return;
        }

        if (!skill.unlocked)
        {
            if (ElementMasteryManager.Instance != null)
            {
                ElementMasteryManager.Instance
                    .UnlockSkill(skill);
            }

            if (!skill.unlocked)
            {
                Debug.Log(
                    $"{skill.skillName} chưa được mở khóa."
                );

                return;
            }
        }

        Debug.Log(
            $"Chọn {currentElement.elementName} " +
            $"Skill {index + 1}: " +
            $"{skill.skillName}"
        );

        switch (currentElement.elementType)
        {
            case ElementType.Fire:
                CastFireSkill(skill);
                break;

            case ElementType.Ice:
                Debug.Log(
                    "Ice Skill Controller chưa được tạo."
                );
                break;

            case ElementType.Thunder:
                Debug.Log(
                    "Thunder Skill Controller chưa được tạo."
                );
                break;
        }
    }

    private void CastFireSkill(
        ElementSkillData skill)
    {
        FireSkillController controller =
            FindFireController();

        if (controller == null)
        {
            Debug.LogError(
                "Không tìm thấy FireSkillController trên Player."
            );

            return;
        }

        /*
         * Controller tự kiểm tra Skill Index:
         *
         * 1, 2, 4 → cast bình thường.
         * 3       → bắt đầu Fire Breath.
         */
        controller.TryCast(skill);
    }

    private FireSkillController FindFireController()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            FireSkillController controller =
                GameManager.Instance.Player
                    .GetComponent<FireSkillController>();

            if (controller != null)
                return controller;
        }

        return FindFirstObjectByType<
            FireSkillController
        >();
    }

    private void StopActiveChannelSkill()
    {
        FireSkillController controller =
            FindFireController();

        if (controller != null &&
            controller.IsBreathing)
        {
            controller.StopFireBreath();
        }
    }

    private void PositionAboveTaskbar()
    {
        if (skillBarRect == null ||
            taskbar == null)
        {
            return;
        }

        /*
         * Đồng bộ Anchor và Pivot để tránh
         * thanh Element bị lệch so với Taskbar.
         */
        skillBarRect.anchorMin =
            taskbar.anchorMin;

        skillBarRect.anchorMax =
            taskbar.anchorMax;

        skillBarRect.pivot =
            taskbar.pivot;

        skillBarRect.anchoredPosition =
            taskbar.anchoredPosition +
            new Vector2(
                0f,
                verticalOffset
            );
    }

    private void OnMasteryChanged(
        ElementType elementType,
        int amount)
    {
        if (currentElement == null ||
            currentElement.elementType != elementType)
        {
            return;
        }

        RefreshButtons();
    }

    private void OnSkillUnlocked(
        ElementSkillData skill)
    {
        if (currentElement == null ||
            skill == null ||
            skill.elementType !=
            currentElement.elementType)
        {
            return;
        }

        RefreshButtons();
    }

    private void RefreshButtons()
    {
        foreach (
            ElementSkillButtonUI button
            in skillButtons)
        {
            if (button != null)
            {
                button.Refresh();
            }
        }
    }

    private void OnDestroy()
    {
        StopActiveChannelSkill();

        if (Instance == this)
        {
            Instance = null;
        }
    }
}