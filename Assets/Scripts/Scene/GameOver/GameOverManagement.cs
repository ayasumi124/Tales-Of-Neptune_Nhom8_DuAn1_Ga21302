using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagement : MonoBehaviour
{
    [Header("UI")]
    [Tooltip(
        "Object cha chứa toàn bộ HUD gameplay."
    )]
    [SerializeField]
    private GameObject gameplayUI;

    [SerializeField]
    private GameObject gameOverPanel;

    [Header("Music")]
    [Tooltip(
        "Nhạc Game Over. " +
        "Để trống nếu muốn giữ nhạc scene."
    )]
    [SerializeField]
    private AudioClip gameOverMusic;

    [Tooltip(
        "Tắt nhạc scene nếu không gán Game Over Music."
    )]
    [SerializeField]
    private bool stopMusicOnGameOver;

    private bool gameOver;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        gameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                false
            );
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(
                true
            );
        }
    }

    // =====================================================
    // GAME OVER
    // =====================================================

    public void ShowGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        // ---------------------------------------------
        // Đóng các UI gameplay đặc biệt
        // ---------------------------------------------

        if (SkillUnlockUI.Instance != null)
        {
            SkillUnlockUI.Instance
                .ForceHide();
        }

        // ---------------------------------------------
        // AUDIO
        // ---------------------------------------------

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .StopFootstep();

            if (gameOverMusic != null)
            {
                AudioManager.Instance
                    .ChangeMusic(
                        gameOverMusic
                    );
            }
            else if (stopMusicOnGameOver)
            {
                AudioManager.Instance
                    .StopMusic();
            }
        }

        // ---------------------------------------------
        // UI
        // ---------------------------------------------

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(
                false
            );
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                true
            );
        }

        /*
         * Đóng băng gameplay.
         */
        Time.timeScale = 0f;
    }

    // =====================================================
    // PLAY AGAIN
    // =====================================================

    public void PlayAgain()
    {
        if (!gameOver)
            return;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy SceneLoader."
            );

            return;
        }

        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            Debug.LogError(
                "Không tìm thấy Player."
            );

            return;
        }

        /*
         * Cho SceneLoader coroutine chạy được.
         */
        Time.timeScale = 1f;

        gameOver = false;

        // ---------------------------------------------
        // UI
        // ---------------------------------------------

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                false
            );
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(
                true
            );
        }

        /*
         * =============================================
         * QUAN TRỌNG NHẤT
         * =============================================
         *
         * BẮT ĐẦU RELOAD TRƯỚC KHI RESET HEALTH.
         *
         * LoadSceneRoutine() của SceneLoader sẽ lập tức:
         *
         * isLoading = true;
         * LockPlayer();
         *
         * trước yield đầu tiên.
         *
         * Vì vậy BossMovement.ShouldIgnoreDetection()
         * sẽ nhìn thấy SceneLoader.IsLoading = true.
         *
         * Boss không thể detect Player ở vị trí chết.
         */
        SceneLoader.Instance
            .ReloadSavedScene();

        /*
         * Bây giờ mới hồi sinh Player.
         *
         * Dù IsDead chuyển false thì
         * SceneLoader.IsLoading đã true.
         */
        ResetPlayerForRespawn();

        Debug.Log(
            "Play Again: Player đang được respawn."
        );
    }

    // =====================================================
    // RESET PLAYER
    // =====================================================

    private void ResetPlayerForRespawn()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            Debug.LogError(
                "Không tìm thấy Player để reset."
            );

            return;
        }

        GameObject playerObject =
            GameManager.Instance.Player;

        // ---------------------------------------------
        // HEALTH
        // ---------------------------------------------

        Health health =
            playerObject.GetComponent<Health>();

        if (health != null)
        {
            /*
             * ResetHealth() sẽ:
             * - IsDead = false
             * - hồi đầy HP
             * - bật Players
             * - bật Attack
             * - bật Dash
             */
            health.ResetHealth();

            /*
             * Trong lúc Fade/Loading,
             * Player không được nhận damage.
             */
            health.SetInvincible(
                true
            );
        }

        // ---------------------------------------------
        // MOVEMENT
        // ---------------------------------------------

        Players players =
            playerObject.GetComponent<Players>();

        if (players != null)
        {
            players.StopAutoWalk();

            /*
             * ResetHealth bật lại Players,
             * nên khóa ngay lập tức.
             */
            players.LockControl();
        }

        // ---------------------------------------------
        // ATTACK
        // ---------------------------------------------

        Attack attack =
            playerObject.GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();

            /*
             * SceneLoader.UnlockPlayer()
             * sẽ bật lại sau Fade.
             */
            attack.enabled = false;
        }

        // ---------------------------------------------
        // DASH
        // ---------------------------------------------

        PlayerDash dash =
            playerObject.GetComponent<PlayerDash>();

        if (dash != null)
        {
            if (dash.IsDashing)
            {
                dash.CancelDash();
            }

            /*
             * SceneLoader.UnlockPlayer()
             * sẽ bật lại sau Fade.
             */
            dash.enabled = false;
        }

        // ---------------------------------------------
        // PHYSICS
        // ---------------------------------------------

        Rigidbody2D rb =
            playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        /*
         * Không teleport Player ở đây.
         *
         * SceneLoader sẽ tự:
         *
         * MovePlayerToSpawn(spawnID)
         *
         * sau khi scene reload hoàn tất.
         */
    }

    // =====================================================
    // BACK TO MENU
    // =====================================================

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        gameOver = false;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(
                false
            );
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(
                true
            );
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .StopFootstep();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .EndSessionAndReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene(
                "MainMenu"
            );
        }
    }
}