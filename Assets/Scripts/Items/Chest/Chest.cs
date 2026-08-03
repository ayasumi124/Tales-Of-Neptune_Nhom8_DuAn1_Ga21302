using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chest : MonoBehaviour
{
    public enum ChestSkillRewardType
    {
        Ability,
        Element
    }

    [Header("Skill Reward Type")]
    [SerializeField]
    private ChestSkillRewardType skillRewardType;

    [Header("Ability Reward")]
    [SerializeField]
    private AbilityData abilityReward;

    [Header("Element Reward")]
    [SerializeField]
    private ElementData elementReward;

    [Header("Chest Save")]
    [Tooltip(
        "Mỗi rương phải có một ID khác nhau."
    )]
    [SerializeField]
    private string chestID;

    [SerializeField]
    private bool opened;

    [Header("Animator")]
    [SerializeField]
    private string openedStateName =
        "Chest1_opened";

    [Header("UI")]
    [SerializeField]
    private GameObject keyIcon;

    private bool rewardGiven;
    private bool playerInside;

    private Players player;
    private EnermyMovement[] enemies;
    private CloneFollow[] clones;
    private Animator animator;
    private ChestReward chestReward;

    public string FullChestID =>
        SceneManager.GetActiveScene().name +
        "_" +
        chestID;

    private void Awake()
    {
        animator =
            GetComponent<Animator>();

        chestReward =
            GetComponent<ChestReward>();
    }

    private void Start()
    {
        FindObjects();

        SkillUnlockUI.OnSkillPanelClosed +=
            ResumeGame;

        ValidateRuntimeChestID();
        LoadChestState();

        if (keyIcon != null)
            keyIcon.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside ||
            opened)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (opened ||
            !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null)
            keyIcon.SetActive(true);
    }

    private void OnTriggerExit2D(
        Collider2D other)
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

        if (string.IsNullOrWhiteSpace(
                chestID))
        {
            Debug.LogError(
                $"{name} chưa có Chest ID."
            );

            return;
        }

        /*
         * Khóa tại scene hiện tại để người chơi
         * không thể bấm mở nhiều lần trong animation.
         *
         * Chưa MarkChestOpened cho đến khi
         * phần thưởng thực sự được nhận thành công.
         */
        opened = true;
        playerInside = false;

        if (keyIcon != null)
            keyIcon.SetActive(false);

        Debug.Log(
            $"Bắt đầu mở rương: {FullChestID}"
        );

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance
                    .chestOpenSound
            );
        }

        FreezeGame();

        if (animator != null)
        {
            animator.SetBool(
                "IsOpened",
                true
            );
        }
        else
        {
            /*
             * Không có Animator thì trao thưởng ngay.
             */
            OnRewardAnimationEvent();
        }
    }

    /*
     * Animation Event chỉ gọi duy nhất hàm này.
     *
     * Không dùng tên GiveReward nữa để tránh
     * trùng với component khác.
     */
    // Animation Event gọi duy nhất hàm này.
public void OnRewardAnimationEvent()
{
    if (rewardGiven)
        return;

    bool success;

    // Rương Coin, Potion, Heart Container.
    if (chestReward != null)
    {
        success = chestReward.ClaimReward();

        if (success)
        {
            CompleteReward();
            ResumeGame();
        }
        else
        {
            CancelFailedReward();
        }

        return;
    }

    // Rương Ability hoặc Element.
    success = ClaimSkillReward();

    if (success)
    {
        CompleteReward();

        /*
         * Không ResumeGame ở đây.
         * SkillUnlockUI sẽ gọi ResumeGame
         * sau khi người chơi đóng panel.
         */
    }
    else
    {
        CancelFailedReward();
    }
}

    private bool ClaimSkillReward()
{
    switch (skillRewardType)
    {
        case ChestSkillRewardType.Ability:
            return ClaimAbilityReward();

        case ChestSkillRewardType.Element:
            return ClaimElementReward();

        default:
            Debug.LogError(
                $"{name}: Skill Reward Type không hợp lệ."
            );

            return false;
    }
}

private bool ClaimAbilityReward()
{
    if (abilityReward == null)
    {
        Debug.LogError(
            $"{name} chưa gán AbilityData."
        );

        return false;
    }

    if (AbilityManager.Instance != null)
    {
        AbilityManager.Instance.UnlockAbility(
            abilityReward.type
        );
    }

    if (SkillInventoryUI.Instance == null)
    {
        Debug.LogError(
            "Không tìm thấy SkillInventoryUI."
        );

        return false;
    }

    SkillInventoryUI.Instance.AddAbility(
        abilityReward
    );

    if (SkillUnlockUI.Instance == null)
    {
        Debug.LogError(
            "Không tìm thấy SkillUnlockUI."
        );

        return false;
    }

    SkillUnlockUI.Instance.ShowAbility(
        abilityReward
    );

    return true;
}

private bool ClaimElementReward()
{
    if (elementReward == null)
    {
        Debug.LogError(
            $"{name} chưa gán ElementData."
        );

        return false;
    }

    if (SkillInventoryUI.Instance == null)
    {
        Debug.LogError(
            "Không tìm thấy SkillInventoryUI."
        );

        return false;
    }

    SkillInventoryUI.Instance.AddElement(
        elementReward
    );

    if (SkillUnlockUI.Instance == null)
    {
        Debug.LogError(
            "Không tìm thấy SkillUnlockUI."
        );

        return false;
    }

    SkillUnlockUI.Instance.ShowElement(
        elementReward
    );

    return true;
}

    private void CompleteReward()
    {
        rewardGiven = true;

        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance
                .MarkChestOpened(
                    FullChestID
                );
        }

        Debug.Log(
            $"Đã nhận thưởng rương: {FullChestID}"
        );
    }

    private void CancelFailedReward()
    {
        opened = false;
        rewardGiven = false;

        if (animator != null)
        {
            animator.SetBool(
                "IsOpened",
                false
            );
        }

        ResumeGame();

        Debug.LogWarning(
            $"Nhận thưởng thất bại, " +
            $"cho phép mở lại rương: {FullChestID}"
        );
    }

    private void LoadChestState()
    {
        if (GameSessionManager.Instance == null)
            return;

        if (!GameSessionManager.Instance
                .IsChestOpened(
                    FullChestID))
        {
            return;
        }

        opened = true;
        rewardGiven = true;

        if (chestReward != null)
        {
            chestReward.RestoreClaimedState();
        }

        if (animator != null)
        {
            animator.SetBool(
                "IsOpened",
                true
            );

            if (!string.IsNullOrWhiteSpace(
                    openedStateName))
            {
                animator.Play(
                    openedStateName,
                    0,
                    0f
                );

                animator.Update(0f);
            }
        }

        Debug.Log(
            $"Rương đã mở trước đó: {FullChestID}"
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
        {
            player =
                FindFirstObjectByType<Players>();
        }

        enemies =
            FindObjectsByType<EnermyMovement>(
                FindObjectsSortMode.None
            );

        clones =
            FindObjectsByType<CloneFollow>(
                FindObjectsSortMode.None
            );
    }

    private void FreezeGame()
    {
        FindObjects();

        if (player != null)
        {
            player.LockControl();
        }

        foreach (EnermyMovement enemy
                 in enemies)
        {
            if (enemy == null)
                continue;

            enemy.enabled = false;

            Rigidbody2D enemyRb =
                enemy.GetComponent<Rigidbody2D>();

            if (enemyRb != null)
            {
                enemyRb.linearVelocity =
                    Vector2.zero;
            }
        }

        foreach (CloneFollow clone
                 in clones)
        {
            if (clone == null)
                continue;

            clone.enabled = false;

            Rigidbody2D cloneRb =
                clone.GetComponent<Rigidbody2D>();

            if (cloneRb != null)
            {
                cloneRb.linearVelocity =
                    Vector2.zero;
            }
        }
    }

    private void ResumeGame()
    {
        FindObjects();

        if (player != null)
        {
            Health health =
                player.GetComponent<Health>();

            bool sceneLoading =
                SceneLoader.Instance != null &&
                SceneLoader.Instance.IsLoading;

            if (!sceneLoading &&
                (health == null ||
                 !health.IsDead))
            {
                player.UnlockControl();
            }
        }

        foreach (EnermyMovement enemy
                 in enemies)
        {
            if (enemy != null)
                enemy.enabled = true;
        }

        foreach (CloneFollow clone
                 in clones)
        {
            if (clone != null)
                clone.enabled = true;
        }
    }

    private void ValidateRuntimeChestID()
    {
        if (string.IsNullOrWhiteSpace(
                chestID))
        {
            Debug.LogError(
                $"{name} chưa được gán Chest ID."
            );

            return;
        }

        Chest[] chests =
            FindObjectsByType<Chest>(
                FindObjectsSortMode.None
            );

        foreach (Chest other
                 in chests)
        {
            if (other == null ||
                other == this)
            {
                continue;
            }

            if (other.FullChestID ==
                FullChestID)
            {
                Debug.LogError(
                    $"TRÙNG CHEST ID: {FullChestID}\n" +
                    $"Rương 1: {name}\n" +
                    $"Rương 2: {other.name}\n" +
                    "Hãy chọn một rương và dùng " +
                    "'Generate New Chest ID'."
                );
            }
        }
    }

    [ContextMenu("Generate New Chest ID")]
    private void GenerateNewChestID()
    {
        chestID =
            Guid.NewGuid()
                .ToString("N");

        Debug.Log(
            $"{name} có Chest ID mới: {chestID}",
            this
        );
    }

    [ContextMenu("Reset This Chest In Session")]
    private void ResetThisChestInSession()
    {
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance
                .ResetChest(
                    FullChestID
                );
        }

        opened = false;
        rewardGiven = false;

        if (chestReward != null)
            chestReward.ResetReward();

        if (animator != null)
        {
            animator.SetBool(
                "IsOpened",
                false
            );

            animator.Rebind();
            animator.Update(0f);
        }
    }

    private void OnValidate()
    {
        /*
         * Chỉ tự tạo ID khi đang rỗng.
         * Duplicate prefab sẽ giữ ID cũ,
         * nên sau khi duplicate cần chọn:
         * Generate New Chest ID.
         */
        if (string.IsNullOrWhiteSpace(
                chestID))
        {
            chestID =
                Guid.NewGuid()
                    .ToString("N");
        }
    }

    private void OnDestroy()
    {
        SkillUnlockUI.OnSkillPanelClosed -=
            ResumeGame;
    }
}