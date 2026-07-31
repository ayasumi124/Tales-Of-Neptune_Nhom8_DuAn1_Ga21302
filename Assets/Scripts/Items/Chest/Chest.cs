using UnityEngine;
using UnityEngine.SceneManagement;

public class Chest : MonoBehaviour
{
    [Header("Reward")]
    public AbilityData reward;

    [Header("Chest Save")]
    [SerializeField] private string chestID;

    [SerializeField] private bool opened;

    [Header("Animator States")]
    [SerializeField] private string openedStateName = "Chest1_opened";

    private bool rewardGiven;

    private Players player;
    private EnermyMovement[] enemies;
    private CloneFollow[] clones;
    private Animator animator;

    [Header("UI")]
    [SerializeField] private GameObject keyIcon;

    private bool playerInside;

    private string FullChestID =>
        SceneManager.GetActiveScene().name + "_" + chestID;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        FindObjects();

        SkillUnlockUI.OnSkillPanelClosed += ResumeGame;

        LoadChestState();
        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (opened)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OnDestroy()
    {
        SkillUnlockUI.OnSkillPanelClosed -= ResumeGame;
    }

    private void LoadChestState()
    {
        if (GameSessionManager.Instance == null)
            return;

        if (!GameSessionManager.Instance.IsChestOpened(FullChestID))
            return;

        opened = true;
        rewardGiven = true;

        if (animator != null)
        {
            animator.SetBool("IsOpened", true);

            animator.Play(
                openedStateName,
                0,
                0f
            );

            animator.Update(0f);
        }

        Debug.Log(
            $"Chest đã mở trước đó: {FullChestID}"
        );
    }

    private void FindObjects()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player =
                GameManager.Instance.Player
                    .GetComponent<Players>();
        }

        if (player == null)
            player = FindFirstObjectByType<Players>();

        enemies =
            FindObjectsByType<EnermyMovement>(
                FindObjectsSortMode.None
            );

        clones =
            FindObjectsByType<CloneFollow>(
                FindObjectsSortMode.None
            );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (keyIcon != null)
            keyIcon.SetActive(true);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    public void OpenChest()
    {
        if (opened)
            return;

        if (string.IsNullOrWhiteSpace(chestID))
        {
            Debug.LogError(
                $"Chest {gameObject.name} chưa được gán Chest ID."
            );
            return;
        }

        opened = true;
        playerInside = false;

        if (keyIcon != null)
            keyIcon.SetActive(false);

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.MarkChestOpened(
                FullChestID
            );
        }

        Debug.Log(
            $"Open Chest: {FullChestID}"
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.chestOpenSound
            );
        }

        FreezeGame();

        if (animator != null)
            animator.SetBool("IsOpened", true);

        // Không gọi GiveReward tại đây.
        // Animation Event sẽ gọi GiveReward().
    }

    private void FreezeGame()
    {
        FindObjects();

        if (player != null)
            player.enabled = false;

        foreach (EnermyMovement enemy in enemies)
        {
            if (enemy == null)
                continue;

            enemy.enabled = false;

            Rigidbody2D rb =
                enemy.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }

        foreach (CloneFollow clone in clones)
        {
            if (clone == null)
                continue;

            clone.enabled = false;

            Rigidbody2D rb =
                clone.GetComponent<Rigidbody2D>();

            if (rb != null)
                rb.linearVelocity = Vector2.zero;
        }
    }

    private void ResumeGame()
    {
        FindObjects();

        if (player != null)
            player.enabled = true;

        foreach (EnermyMovement enemy in enemies)
        {
            if (enemy != null)
                enemy.enabled = true;
        }

        foreach (CloneFollow clone in clones)
        {
            if (clone != null)
                clone.enabled = true;
        }
    }

    // Animation Event gọi hàm này
    public void GiveReward()
    {
        if (rewardGiven)
            return;

        rewardGiven = true;

        if (reward == null)
        {
            Debug.LogError(
                $"Reward của chest {FullChestID} chưa được gán."
            );

            ResumeGame();
            return;
        }

        if (AbilityManager.Instance != null)
        {
            AbilityManager.Instance.UnlockAbility(
                reward.type
            );
        }

        if (SkillInventoryUI.Instance != null)
        {
            SkillInventoryUI.Instance.AddSkill(
                reward
            );
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.EquipSkill(
                reward,
                3
            );
        }

        if (SkillUnlockUI.Instance != null)
        {
            SkillUnlockUI.Instance.ShowSkill(
                reward
            );
        }
        else
        {
            Debug.LogError(
                "Không tìm thấy SkillUnlockUI."
            );

            ResumeGame();
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(chestID))
        {
            chestID = gameObject.name;
        }
    }
}