using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockUI : MonoBehaviour
{
    public static SkillUnlockUI Instance
    {
        get;
        private set;
    }

    public static Action OnSkillPanelClosed;
    [Header("Animation States")]
    [SerializeField] private string showStateName = "panelskillUI_show";
    [SerializeField] private string hideStateName = "panelskillUI_hide";

    [Header("UI")]
    [SerializeField] private GameObject dimBackground;
    [SerializeField] private GameObject panel;

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private TextMeshProUGUI description;

    [Header("Animation")]
    [SerializeField] private float closeDelay = 0.25f;

    private Animator animator;
    private CanvasGroup panelCanvasGroup;

    private bool waitingForClose;
    private Coroutine closeCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogWarning(
                "Phát hiện SkillUnlockUI trùng. " +
                "Đã xóa bản mới: " +
                gameObject.scene.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        CacheComponents();
    }

    private void Start()
    {
        ForceHide();
    }

    private void Update()
    {
        if (!waitingForClose)
            return;

        if (Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            HideSkill();
        }
    }

    private void CacheComponents()
    {
        if (panel == null)
            return;

        if (animator == null)
        {
            animator =
                panel.GetComponent<Animator>();
        }

        if (panelCanvasGroup == null)
        {
            panelCanvasGroup =
                panel.GetComponent<CanvasGroup>();
        }
    }

    public void ShowSkill(
        AbilityData data)
    {
        ShowAbility(data);
    }

    public void ShowAbility(
        AbilityData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "AbilityData truyền vào ShowAbility đang null."
            );

            return;
        }
        LogPanelInfo("ABILITY");

        ShowPanel(
            data.icon,
            data.skillName,
            data.description
        );
    }

    public void ShowElement(
        ElementData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "ElementData truyền vào ShowElement đang null."
            );

            return;
        }
        LogPanelInfo("ELEMENT");

        ShowPanel(
            data.elementIcon,
            data.elementName,
            data.description
        );
    }

    private void ShowPanel(
        Sprite rewardIcon,
        string rewardName,
        string rewardDescription)
    {
        if (panel == null)
        {
            Debug.LogError(
                "SkillUnlockUI chưa được gán Panel."
            );

            return;
        }

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        waitingForClose = false;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (dimBackground != null)
        {
            dimBackground.SetActive(true);
        }

        panel.SetActive(true);

        CacheComponents();

        panel.transform.localScale =
            Vector3.one;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        if (icon != null)
        {
            icon.sprite =
                rewardIcon;

            icon.enabled =
                rewardIcon != null;

            icon.preserveAspect =
                true;

            Color iconColor =
                icon.color;

            iconColor.a = 1f;
            icon.color = iconColor;
        }

        if (skillName != null)
        {
            skillName.text =
                string.IsNullOrWhiteSpace(
                    rewardName)
                    ? "Unknown Skill"
                    : rewardName;
        }

        if (description != null)
        {
            description.text =
                rewardDescription ?? "";
        }

        if (animator != null)
        {
            /*
             * Tắt Animator trước để nó không giữ lại
             * scale 0.15 từ animation Hide.
             */
            animator.enabled = false;

            panel.transform.localScale = Vector3.one;
            panel.transform.localRotation = Quaternion.identity;

            animator.enabled = true;
            animator.speed = 1f;

            animator.ResetTrigger("Hide");
            animator.ResetTrigger("Show");

            /*
             * Ép animation Show chạy lại từ frame đầu,
             * không phụ thuộc state trước đó.
             */
            animator.Play(
                showStateName,
                0,
                0f
            );

            animator.Update(0f);
        }
        else
        {
            panel.transform.localScale = Vector3.one;
        }

        waitingForClose = true;

        if (AudioManager.Instance != null &&
            AudioManager.Instance.skillUnlockSound != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance
                    .skillUnlockSound
            );
        }

        Debug.Log(
            $"Hiện SkillUnlockUI: {rewardName}"
        );
    }

    private void LogPanelInfo(string rewardType)
    {
        RectTransform panelRect =
            panel != null
                ? panel.GetComponent<RectTransform>()
                : null;

        Debug.Log(
            $"===== {rewardType} =====\n" +
            $"SkillUnlockUI object: {gameObject.name}\n" +
            $"Scene: {gameObject.scene.name}\n" +
            $"Panel: {(panel != null ? panel.name : "NULL")}\n" +
            $"Panel parent: " +
            $"{(panel != null && panel.transform.parent != null ? panel.transform.parent.name : "NULL")}\n" +
            $"Local scale: " +
            $"{(panelRect != null ? panelRect.localScale.ToString() : "NULL")}\n" +
            $"Lossy scale: " +
            $"{(panelRect != null ? panelRect.lossyScale.ToString() : "NULL")}",
            this
        );
    }
    private void HideSkill()
    {
        if (!waitingForClose)
            return;

        waitingForClose = false;

        if (AudioManager.Instance != null &&
            AudioManager.Instance.skillCloseSound != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance
                    .skillCloseSound
            );
        }

        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Hide");
        }

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
        }

        closeCoroutine =
            StartCoroutine(
                ClosePanelRoutine()
            );
    }

    private IEnumerator ClosePanelRoutine()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, closeDelay)
        );

        if (animator != null)
        {
            animator.enabled = false;
        }

        if (panel != null)
        {
            panel.transform.localScale =
                Vector3.one;

            panel.SetActive(false);
        }

        if (animator != null)
        {
            animator.enabled = true;
        }

        if (dimBackground != null)
            dimBackground.SetActive(false);

        closeCoroutine = null;

        OnSkillPanelClosed?.Invoke();
    }

    public void ForceHide()
    {
        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        waitingForClose = false;

        CacheComponents();

        if (animator != null)
        {
            animator.enabled = false;

            animator.ResetTrigger("Show");
            animator.ResetTrigger("Hide");
        }

        if (panel != null)
        {
            panel.transform.localScale =
                Vector3.one;

            panel.transform.localRotation =
                Quaternion.identity;
        }

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }

        if (dimBackground != null)
            dimBackground.SetActive(false);

        if (panel != null)
            panel.SetActive(false);

        /*
         * Bật lại để lần sau ShowPanel có thể Play state.
         */
        if (animator != null)
            animator.enabled = true;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}