using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUnlockUI : MonoBehaviour
{
    public static SkillUnlockUI Instance { get; private set; }

    public static Action OnSkillPanelClosed;

    [Header("UI")]
    public GameObject dimBackground;
    public GameObject panel;

    public Image icon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI description;

    private Animator animator;
    private CanvasGroup panelCanvasGroup;

    private bool waitingForClose;
    private Coroutine closeCoroutine;

    private void Awake()
    {
        // Không để UI mới từ scene khác ghi đè UI persistent cũ.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Phát hiện SkillUnlockUI trùng. Đã xóa bản mới: "
                + gameObject.scene.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panel != null)
        {
            animator = panel.GetComponent<Animator>();
            panelCanvasGroup = panel.GetComponent<CanvasGroup>();
        }
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

    public void ShowSkill(AbilityData data)
    {
        if (data == null)
        {
            Debug.LogError("AbilityData truyền vào ShowSkill đang null.");
            return;
        }

        Debug.Log(
            "ShowSkill chạy trên object: " + gameObject.name +
            " | Scene: " + gameObject.scene.name
        );

        if (closeCoroutine != null)
        {
            StopCoroutine(closeCoroutine);
            closeCoroutine = null;
        }

        waitingForClose = false;

        // GameObject chứa script phải luôn hoạt động.
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (dimBackground != null)
            dimBackground.SetActive(true);

        if (panel != null)
            panel.SetActive(true);

        // Lấy lại component sau khi scene/load thay đổi.
        if (panel != null)
        {
            if (animator == null)
                animator = panel.GetComponent<Animator>();

            if (panelCanvasGroup == null)
                panelCanvasGroup = panel.GetComponent<CanvasGroup>();

            // Ép Panel trở về trạng thái nhìn thấy.
            panel.transform.localScale = Vector3.one;

            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f;
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }

        if (icon != null)
        {
            icon.enabled = true;
            icon.sprite = data.icon;
        }

        if (skillName != null)
            skillName.text = data.skillName;

        if (description != null)
            description.text = data.description;

        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;

            animator.ResetTrigger("Hide");
            animator.ResetTrigger("Show");
            animator.SetTrigger("Show");
        }

        waitingForClose = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.skillUnlockSound
            );
        }
    }

    private void HideSkill()
    {
        if (!waitingForClose)
            return;

        waitingForClose = false;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.skillCloseSound
            );
        }

        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.ResetTrigger("Hide");
            animator.SetTrigger("Hide");
        }

        if (closeCoroutine != null)
            StopCoroutine(closeCoroutine);

        closeCoroutine = StartCoroutine(ClosePanel());
    }

    private IEnumerator ClosePanel()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        if (dimBackground != null)
            dimBackground.SetActive(false);

        if (panel != null)
            panel.SetActive(false);

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

        if (animator == null && panel != null)
            animator = panel.GetComponent<Animator>();

        if (animator != null)
        {
            animator.ResetTrigger("Show");
            animator.ResetTrigger("Hide");
        }

        if (dimBackground != null)
            dimBackground.SetActive(false);

        if (panel != null)
            panel.SetActive(false);

        // Không được dùng gameObject.SetActive(false).
    }

    private void OnDestroy()
    {
        // Chỉ xóa Instance khi đúng object hiện tại bị Destroy.
        if (Instance == this)
            Instance = null;
    }
}