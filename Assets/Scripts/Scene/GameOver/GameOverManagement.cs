using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagement : MonoBehaviour
{
    [Header("UI")]
    [Tooltip(
        "Object cha chứa toàn bộ HUD gameplay."
    )]
    [SerializeField] private GameObject gameplayUI;

    [SerializeField] private GameObject gameOverPanel;

    [Header("Music")]
    [Tooltip(
        "Nhạc Game Over. Để trống nếu muốn giữ nhạc scene."
    )]
    [SerializeField] private AudioClip gameOverMusic;

    [Tooltip(
        "Tắt nhạc scene nếu không gán Game Over Music."
    )]
    [SerializeField] private bool stopMusicOnGameOver;

    private bool gameOver;

    private void Awake()
    {
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);
    }

    public void ShowGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (SkillUnlockUI.Instance != null)
        {
            SkillUnlockUI.Instance.ForceHide();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopFootstep();

            if (gameOverMusic != null)
            {
                AudioManager.Instance.ChangeMusic(
                    gameOverMusic
                );
            }
            else if (stopMusicOnGameOver)
            {
                AudioManager.Instance.StopMusic();
            }
        }

        /*
         * Ẩn toàn bộ HUD gameplay.
         */
        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        /*
         * Chỉ giữ Game Over Panel.
         */
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void PlayAgain()
    {
        if (!gameOver)
            return;

        Time.timeScale = 1f;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        /*
         * Bật lại HUD khi bấm Play Again.
         */
        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        ResetPlayer();

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            Health health =
                GameManager.Instance.Player
                    .GetComponent<Health>();

            if (health != null)
                health.SetInvincible(true);
        }

        if (SceneLoader.Instance != null)
        {
            /*
             * SceneMusic của scene được reload
             * sẽ tự đổi lại nhạc scene.
             */
            SceneLoader.Instance.ReloadSavedScene();
        }
        else
        {
            Debug.LogError(
                "Không tìm thấy SceneLoader."
            );
        }
    }

    private void ResetPlayer()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            Debug.LogError(
                "Không tìm thấy Player để reset."
            );

            return;
        }

        GameObject player =
            GameManager.Instance.Player;

        Health health =
            player.GetComponent<Health>();

        if (health != null)
            health.ResetHealth();

        Players players =
            player.GetComponent<Players>();

        if (players != null)
        {
            players.StopAutoWalk();
            players.LockControl();
        }

        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopFootstep();

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