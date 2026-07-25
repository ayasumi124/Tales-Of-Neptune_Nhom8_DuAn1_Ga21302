using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class SkillUnlockUI : MonoBehaviour
{
    public static SkillUnlockUI Instance;

    public static Action OnSkillPanelClosed;

    [Header("UI")]
    public GameObject dimBackground;
    public GameObject panel;

    public Image icon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI description;

    Animator animator;

    bool waitingForClose;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        animator = panel.GetComponent<Animator>();

        dimBackground.SetActive(false);
        panel.SetActive(false);
    }

    void Update()
    {
        if (!waitingForClose)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            waitingForClose = false;

            // Âm thanh đóng skill panel
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.skillCloseSound
            );

            animator.SetTrigger("Hide");

            StartCoroutine(ClosePanel());
        }
    }

    public void ShowSkill(AbilityData data)
    {
        dimBackground.SetActive(true);
        panel.SetActive(true);

        icon.sprite = data.icon;
        skillName.text = data.skillName;
        description.text = data.description;

        animator.ResetTrigger("Hide");
        animator.SetTrigger("Show");

        waitingForClose = true;

        AudioManager.Instance.PlaySFX(AudioManager.Instance.skillUnlockSound);
    }

    IEnumerator ClosePanel()
    {
        yield return new WaitForSeconds(0.25f);

        dimBackground.SetActive(false);
        panel.SetActive(false);

        OnSkillPanelClosed?.Invoke();
    }
}