using System;
using UnityEngine;

public class Chest : SaveObject
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

    // =====================================================
    // SAVE
    // =====================================================

    [Header("Chest Save")]
    [Tooltip(
        "Bật = rương nhớ trạng thái đã mở.\n" +
        "Tắt = rương reset mỗi lần load lại scene.\n" +
        "Key Chest nên TẮT."
    )]
    [SerializeField]
    private bool saveOpenedState = true;

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
        BuildSceneSaveID();

    // =====================================================
    // UNITY
    // =====================================================

    protected override void Awake()
    {
        base.Awake();

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

        /*
         * Rương bình thường:
         * kiểm tra ID và load trạng thái.
         *
         * Key Chest:
         * luôn reset khi scene được load.
         */
        if (saveOpenedState)
        {
            ValidateRuntimeChestID();
            LoadChestState();
        }
        else
        {
            ResetRuntimeChest();
        }

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
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

    // =====================================================
    // PLAYER INTERACTION
    // =====================================================

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
        {
            keyIcon.SetActive(true);
        }
    }

    /*
     * Quan trọng với Key Chest được spawn bằng SetActive.
     *
     * Nếu Player đang đứng sẵn trong collider lúc chest
     * xuất hiện thì OnTriggerEnter2D có thể không chạy
     * như mong muốn.
     */
    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (opened ||
            !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null &&
            !keyIcon.activeSelf)
        {
            keyIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }

    // =====================================================
    // OPEN CHEST
    // =====================================================

    public void OpenChest()
    {
        if (opened)
            return;

        /*
         * Chỉ rương persistent mới cần ID.
         *
         * Key Chest không lưu trạng thái nên
         * không cần Persistent ID.
         */
        if (saveOpenedState &&
            !HasValidSaveID)
        {
            Debug.LogError(
                $"{name} chưa có Persistent ID hợp lệ.",
                this
            );

            return;
        }

        opened = true;
        playerInside = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }

        Debug.Log(
            $"Bắt đầu mở rương: {name}"
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
             * Không có Animator:
             * trao thưởng ngay.
             */
            OnRewardAnimationEvent();
        }
    }

    // =====================================================
    // ANIMATION EVENT
    // =====================================================

    /*
     * Đặt Animation Event này ở animation mở rương.
     */
    public void OnRewardAnimationEvent()
    {
        if (rewardGiven)
            return;

        bool success;

        // ---------------------------------------------
        // Coin / Item / Dungeon Key / Potion...
        // ---------------------------------------------

        if (chestReward != null)
        {
            success =
                chestReward.ClaimReward();

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

        // ---------------------------------------------
        // Ability / Element
        // ---------------------------------------------

        success =
            ClaimSkillReward();

        if (success)
        {
            CompleteReward();

            /*
             * Không ResumeGame ở đây.
             *
             * SkillUnlockUI sẽ ResumeGame
             * sau khi panel đóng.
             */
        }
        else
        {
            CancelFailedReward();
        }
    }

    // =====================================================
    // SKILL REWARD
    // =====================================================

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
            AbilityManager.Instance
                .UnlockAbility(
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

    // =====================================================
    // COMPLETE REWARD
    // =====================================================

    private void CompleteReward()
    {
        rewardGiven = true;

        /*
         * CHỈ rương persistent mới được lưu.
         *
         * Key Chest không chạy đoạn này.
         */
        if (saveOpenedState &&
            GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance
                .MarkChestOpened(
                    FullChestID
                );

            Debug.Log(
                $"Đã lưu rương: {FullChestID}"
            );
        }
        else
        {
            Debug.Log(
                $"{name}: nhận reward, " +
                "không lưu trạng thái rương."
            );
        }
    }

    // =====================================================
    // FAILED REWARD
    // =====================================================

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
            $"{name}: nhận thưởng thất bại, " +
            "cho phép mở lại."
        );
    }

    // =====================================================
    // LOAD SAVED CHEST
    // =====================================================

    private void LoadChestState()
    {
        /*
         * Key Chest tuyệt đối không load save.
         */
        if (!saveOpenedState)
            return;

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
            chestReward
                .RestoreClaimedState();
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

    // =====================================================
    // RESET RUNTIME CHEST
    // =====================================================

    /*
     * Dùng cho Key Chest.
     *
     * Mỗi lần scene được load lại:
     *
     * opened = false
     * rewardGiven = false
     * ChestReward reset
     * Animator trở về trạng thái ban đầu
     */
    private void ResetRuntimeChest()
    {
        opened = false;
        rewardGiven = false;
        playerInside = false;

        if (chestReward != null)
        {
            chestReward.ResetReward();
        }

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);

            animator.SetBool(
                "IsOpened",
                false
            );
        }

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }

        Debug.Log(
            $"{name}: reset runtime chest."
        );
    }

    // =====================================================
    // FIND OBJECTS
    // =====================================================

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

    // =====================================================
    // FREEZE GAME
    // =====================================================

    private void FreezeGame()
    {
        FindObjects();

        if (player != null)
        {
            player.LockControl();
        }

        foreach (
            EnermyMovement enemy
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

        foreach (
            CloneFollow clone
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

    // =====================================================
    // RESUME GAME
    // =====================================================

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

        foreach (
            EnermyMovement enemy
            in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = true;
            }
        }

        foreach (
            CloneFollow clone
            in clones)
        {
            if (clone != null)
            {
                clone.enabled = true;
            }
        }
    }

    // =====================================================
    // VALIDATE SAVE ID
    // =====================================================

    private void ValidateRuntimeChestID()
    {
        /*
         * Key Chest không cần validate ID.
         */
        if (!saveOpenedState)
            return;

        if (!HasValidSaveID)
        {
            Debug.LogError(
                $"{name} chưa có Persistent ID.",
                this
            );

            return;
        }

        Chest[] chests =
            FindObjectsByType<Chest>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (
            Chest other
            in chests)
        {
            if (other == null ||
                other == this)
            {
                continue;
            }

            /*
             * Chỉ so ID với những chest
             * cũng sử dụng save.
             */
            if (!other.saveOpenedState)
                continue;

            if (other.FullChestID ==
                FullChestID)
            {
                Debug.LogError(
                    $"TRÙNG CHEST ID: {FullChestID}\n" +
                    $"Rương 1: {name}\n" +
                    $"Rương 2: {other.name}",
                    this
                );

                return;
            }
        }
    }

    // =====================================================
    // MANUAL RESET
    // =====================================================

    [ContextMenu(
        "Reset This Chest In Session"
    )]
    private void ResetThisChestInSession()
    {
        if (saveOpenedState &&
            GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance
                .ResetChest(
                    FullChestID
                );
        }

        ResetRuntimeChest();
    }

    // =====================================================
    // DESTROY
    // =====================================================

    private void OnDestroy()
    {
        SkillUnlockUI.OnSkillPanelClosed -=
            ResumeGame;
    }
}