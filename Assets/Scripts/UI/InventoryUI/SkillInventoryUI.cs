using System.Collections.Generic;
using UnityEngine;

public class SkillInventoryUI : MonoBehaviour
{
    public static SkillInventoryUI Instance
    {
        get;
        private set;
    }

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Content")]
    [SerializeField] private Transform content;

    [Header("Unified Button Prefab")]
    [SerializeField]
    private InventorySkillButton skillButtonPrefab;

    [Header("Input")]
    [SerializeField]
    private KeyCode openKey =
        KeyCode.B;

    [SerializeField]
    private bool pauseGameWhenOpen = true;

    private readonly List<AbilityData>
        ownedAbilities =
            new List<AbilityData>();

    private readonly List<ElementData>
        ownedElements =
            new List<ElementData>();

    private bool isOpen;
    private float previousTimeScale = 1f;

    public bool IsOpen => isOpen;

    private InventorySkillButton currentSelected;

    public void SelectButton(
        InventorySkillButton button)
    {
        if (button == null)
            return;

        if (currentSelected != null &&
            currentSelected != button)
        {
            currentSelected.SetSelected(false);
        }

        currentSelected = button;
        currentSelected.SetSelected(true);
    }
    public void ClearSelection()
    {
        if (currentSelected == null)
            return;

        currentSelected.SetSelected(false);
        currentSelected = null;
    }


    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogWarning(
                $"Xóa SkillInventoryUI trùng: " +
                $"{gameObject.name}"
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (panel == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán Panel.",
                this
            );

            return;
        }

        if (panel == gameObject)
        {
            Debug.LogError(
                "Panel không được là object chứa " +
                "SkillInventoryUI.",
                this
            );

            return;
        }

        isOpen = false;
        panel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(openKey))
        {
            TogglePanel();
        }
    }

    public void TogglePanel()
    {
        if (isOpen)
        {
            ClosePanel();
        }
        else
        {
            OpenPanel();
        }
    }

    public void OpenPanel()
    {
        if (isOpen)
            return;

        if (panel == null)
            return;

        isOpen = true;
        panel.SetActive(true);

        RefreshButtons();

        if (pauseGameWhenOpen)
        {
            previousTimeScale =
                Time.timeScale;

            Time.timeScale = 0f;
        }

        Players player =
            FindPlayer();

        if (player != null)
        {
            player.LockControl();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryOpen();
        }
    }

    public void ClosePanel()
    {
        if (!isOpen)
            return;

        isOpen = false;
        ClearSelection();
        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        UnlockPlayer();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayInventoryClose();
        }

    }

    public bool AddAbility(
        AbilityData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "AbilityData truyền vào AddAbility đang null."
            );

            return false;
        }

        if (!ownedAbilities.Contains(data))
        {
            ownedAbilities.Add(data);

            Debug.Log(
                $"Đã thêm Ability: " +
                $"{data.skillName}"
            );
        }

        if (isOpen)
        {
            RefreshButtons();
        }

        return true;
    }

    public void AddSkill(
        AbilityData data)
    {
        AddAbility(data);
    }

    public bool AddElement(
        ElementData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "ElementData truyền vào AddElement đang null."
            );

            return false;
        }

        if (!ownedElements.Contains(data))
        {
            ownedElements.Add(data);

            Debug.Log(
                $"Đã thêm Element: " +
                $"{data.elementName}"
            );
        }

        if (isOpen)
        {
            RefreshButtons();
        }

        return true;
    }

    public bool HasAbility(
        AbilityData data)
    {
        return data != null &&
               ownedAbilities.Contains(data);
    }

    public bool HasElement(
        ElementData data)
    {
        return data != null &&
               ownedElements.Contains(data);
    }

    public void RefreshButtons()
    {
        if (content == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán Content."
            );

            return;
        }

        if (skillButtonPrefab == null)
        {
            Debug.LogError(
                "SkillInventoryUI chưa gán " +
                "Skill Button Prefab."
            );

            return;
        }

        ClearSelection();
        ClearButtons();

        foreach (AbilityData ability
                 in ownedAbilities)
        {
            InventorySkillButton button =
                Instantiate(
                    skillButtonPrefab,
                    content
                );

            button.SetupAbility(
                ability
            );
        }

        foreach (ElementData element
                 in ownedElements)
        {
            InventorySkillButton button =
                Instantiate(
                    skillButtonPrefab,
                    content
                );

            button.SetupElement(
                element
            );
        }
    }

    private void ClearButtons()
    {
        if (content == null)
            return;

        for (int i =
                 content.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                content.GetChild(i)
                    .gameObject
            );
        }
    }

    private Players FindPlayer()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            return GameManager.Instance.Player
                .GetComponent<Players>();
        }

        return FindFirstObjectByType<
            Players
        >();
    }

    private void UnlockPlayer()
    {
        bool sceneLoading =
            SceneLoader.Instance != null &&
            SceneLoader.Instance.IsLoading;

        if (sceneLoading)
            return;

        Players player =
            FindPlayer();

        if (player == null)
            return;

        Health health =
            player.GetComponent<Health>();

        if (health != null &&
            health.IsDead)
        {
            return;
        }

        player.UnlockControl();
    }

    private void OnDisable()
    {
        if (isOpen &&
            pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        isOpen = false;
    }

    private void OnDestroy()
    {
        if (isOpen &&
            pauseGameWhenOpen)
        {
            Time.timeScale =
                previousTimeScale > 0f
                    ? previousTimeScale
                    : 1f;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}