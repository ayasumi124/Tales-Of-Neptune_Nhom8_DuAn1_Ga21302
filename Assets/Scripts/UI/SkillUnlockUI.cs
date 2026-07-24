using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillUnlockUI : MonoBehaviour
{
    public static SkillUnlockUI Instance;

    public static Action OnSkillPanelClosed;

    public Image icon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI description;

    bool waitingForClose;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!waitingForClose)
            return;

        if (Input.GetKeyDown(KeyCode.Return))
        {
            waitingForClose = false;

            gameObject.SetActive(false);

            OnSkillPanelClosed?.Invoke();
        }
    }

    public void ShowSkill(AbilityData data)
    {
        gameObject.SetActive(true);

        icon.sprite = data.icon;
        skillName.text = data.skillName;
        description.text = data.description;

        waitingForClose = true;
    }
}