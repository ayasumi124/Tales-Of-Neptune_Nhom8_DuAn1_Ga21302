using UnityEngine;

public class Chest : MonoBehaviour
{
    public AbilityData reward;

    bool opened;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            opened = true;

            animator.SetTrigger("Open");
        }
    }

    // Animation Event
    public void GiveReward()
    {
        AbilityManager.Instance.UnlockAbility(reward);

        SkillUnlockUI.Instance.ShowSkill(reward);
    }
}