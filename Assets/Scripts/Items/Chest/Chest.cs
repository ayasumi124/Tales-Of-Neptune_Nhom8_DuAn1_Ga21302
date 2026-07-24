using UnityEngine;

public class Chest : MonoBehaviour
{
    public AbilityData reward;

    [SerializeField]
    private bool opened;
    bool rewardGiven = false;
    Players player;

    EnermyMovement[] enemies;

    CloneFollow[] clones;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = FindFirstObjectByType<Players>();

        enemies = FindObjectsByType<EnermyMovement>(
            FindObjectsSortMode.None);

        clones = FindObjectsByType<CloneFollow>(
            FindObjectsSortMode.None);
        SkillUnlockUI.OnSkillPanelClosed += ResumeGame;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    public void OpenChest()
    {
        if (opened)
            return;

        opened = true;

        FreezeGame();

        animator.SetBool("IsOpened", true);
    }
    void FreezeGame()
    {
        player.enabled = false;

        foreach (EnermyMovement e in enemies)
        {
            if (e != null)
            {
                e.enabled = false;

                Rigidbody2D rb = e.GetComponent<Rigidbody2D>();

                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }

        foreach (CloneFollow c in clones)
        {
            if (c != null)
            {
                c.enabled = false;

                Rigidbody2D rb = c.GetComponent<Rigidbody2D>();

                if (rb != null)
                    rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void ResumeGame()
    {
        player.enabled = true;

        enemies = FindObjectsByType<EnermyMovement>(
            FindObjectsSortMode.None);

        foreach (EnermyMovement e in enemies)
        {
            if (e != null)
                e.enabled = true;
        }

        clones = FindObjectsByType<CloneFollow>(
            FindObjectsSortMode.None);

        foreach (CloneFollow c in clones)
        {
            if (c != null)
                c.enabled = true;
        }
    }

    void OnDestroy()
    {
        SkillUnlockUI.OnSkillPanelClosed -= ResumeGame;
    }

    // Animation Event
    public void GiveReward()
    {
        if (rewardGiven)
            return;

        rewardGiven = true;

        AbilityManager.Instance.UnlockAbility(reward);

        SkillUnlockUI.Instance.ShowSkill(reward);
    }
}