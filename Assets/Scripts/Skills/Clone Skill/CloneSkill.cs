using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class CloneSkill : MonoBehaviour
{


    public GameObject clonePrefab;
    public AbilityData skillData;

    public SkillSlotUI slotUI;
    PlayerMana mana;

    void Start()
    {
        mana = GetComponent<PlayerMana>();

        slotUI.Setup(skillData);

        slotUI.abilityType = skillData.type;
    }

    void Update()
    {
        Debug.Log("Unlocked: " + AbilityManager.Instance.HasAbility(AbilityType.Clone));
        if (!AbilityManager.Instance.HasAbility(AbilityType.Clone))
            return;
        Debug.Log(AbilityManager.Instance.clone.unlocked);



        // Dùng skill
        if (Input.GetKeyDown(KeyCode.K) &&
    AbilityManager.Instance.clone.cooldown <= 0)
        {
            if (!mana.UseMana(skillData.manaCost))
            {
                ManaUI.Instance.ShowNoMana();
                return;
            }

            SpawnClone();

            AbilityManager.Instance.clone.cooldown = skillData.cooldown;
            AbilityManager.Instance.clone.duration = skillData.duration;
        }
    }

    void SpawnClone()
    {
        Debug.Log("Spawn Clone");

        GameObject clone = Instantiate(
            clonePrefab,
            transform.position,
            Quaternion.identity);

        clone.GetComponent<CloneFollow>().player = transform;

        Destroy(clone, skillData.duration);
    }
}