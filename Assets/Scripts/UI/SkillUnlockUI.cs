using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillUnlockUI : MonoBehaviour
{

      public static SkillUnlockUI Instance;

void Awake()
{
    Instance = this;
}

void Start()
{
    gameObject.SetActive(false);
}
    public Image icon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI description;


    public void ShowSkill(AbilityData data)
{
    gameObject.SetActive(true);

    icon.sprite = data.icon;
    skillName.text = data.skillName;
    description.text = data.description;

    StopAllCoroutines();
    StartCoroutine(HideUI());
}

IEnumerator HideUI()
{
    yield return new WaitForSeconds(3f);

    gameObject.SetActive(false);
}

}