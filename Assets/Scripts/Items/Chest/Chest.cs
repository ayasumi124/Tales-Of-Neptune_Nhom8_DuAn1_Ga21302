using UnityEngine;

public class Chest : MonoBehaviour
{
    [Header("Reward")]
    public AbilityData reward;

    [SerializeField] private bool opened;

    private bool rewardGiven;

    private Players player;
    private EnermyMovement[] enemies;
    private CloneFollow[] clones;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        FindObjects();

        SkillUnlockUI.OnSkillPanelClosed += ResumeGame;
    }

    private void OnDestroy()
    {
        SkillUnlockUI.OnSkillPanelClosed -= ResumeGame;
    }

    void FindObjects()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player.GetComponent<Players>();
        }

        if (player == null)
            player = FindFirstObjectByType<Players>();

        enemies = FindObjectsByType<EnermyMovement>(
            FindObjectsSortMode.None);

        clones = FindObjectsByType<CloneFollow>(
            FindObjectsSortMode.None);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        if (Input.GetKeyDown(KeyCode.E))
            OpenChest();
    }

    public void OpenChest()
    {
        if (opened)
            return;

        opened = true;

        Debug.Log("Open Chest");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.chestOpenSound);
        }

        FreezeGame();

        if (animator != null)
            animator.SetBool("IsOpened", true);

        // Không gọi GiveReward ở đây.
        // Animation Event sẽ gọi GiveReward().
    }

    void FreezeGame()
    {
        FindObjects();

        if (player != null)
            player.enabled = false;

        foreach (EnermyMovement e in enemies)
        {
            if (e == null)
                continue;

            e.enabled = false;

            Rigidbody2D rb = e.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        foreach (CloneFollow c in clones)
        {
            if (c == null)
                continue;

            c.enabled = false;

            Rigidbody2D rb = c.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    void ResumeGame()
    {
        FindObjects();

        if (player != null)
            player.enabled = true;

        foreach (EnermyMovement e in enemies)
        {
            if (e != null)
                e.enabled = true;
        }

        foreach (CloneFollow c in clones)
        {
            if (c != null)
                c.enabled = true;
        }
    }

    // ===== Animation Event =====
    public void GiveReward()
    {
        if (rewardGiven)
            return;

        rewardGiven = true;

        if (reward == null)
        {
            Debug.LogError("Reward chưa được gán.");

            ResumeGame();
            return;
        }

        if (AbilityManager.Instance != null)
            AbilityManager.Instance.UnlockAbility(reward.type);

        if (SkillInventoryUI.Instance != null)
            SkillInventoryUI.Instance.AddSkill(reward);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.EquipSkill(reward, 3);

        if (SkillUnlockUI.Instance != null)
        {
            SkillUnlockUI.Instance.ShowSkill(reward);
        }
        else
        {
            Debug.LogError("Không tìm thấy SkillUnlockUI.");

            ResumeGame();
        }
    }
}