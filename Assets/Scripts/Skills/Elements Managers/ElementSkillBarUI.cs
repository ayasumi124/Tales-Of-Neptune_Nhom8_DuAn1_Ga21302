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
            SelectSkill(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectSkill(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectSkill(2);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            SelectSkill(3);
    }

    public void ShowElement(
        ElementData element)
    {
        if (element == null)
            return;

        currentElement = element;

        PositionAboveTaskbar();

        if (skillBarPanel != null)
            skillBarPanel.SetActive(true);

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
        currentElement = null;

        if (skillBarPanel != null)
            skillBarPanel.SetActive(false);
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
            return;

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

        // Giai đoạn sau sẽ gọi script thi triển skill ở đây.
        FireSkillController fireController = null;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            fireController =
                GameManager.Instance.Player
                    .GetComponent<FireSkillController>();
        }

        if (fireController == null)
        {
            fireController =
                FindFirstObjectByType<
                    FireSkillController
                >();
        }

        if (fireController == null)
        {
            Debug.LogError(
                "Không tìm thấy FireSkillController trên Player."
            );

            return;
        }

        if (currentElement.elementType ==
            ElementType.Fire)
        {
            fireController.TryCast(skill);
        }
    }

    private void PositionAboveTaskbar()
    {
        if (skillBarRect == null ||
            taskbar == null)
        {
            return;
        }

        Vector2 position =
            taskbar.anchoredPosition;

        position.y += verticalOffset;

        skillBarRect.anchoredPosition =
            position;
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
                button.Refresh();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}