using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagement : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

    private bool gameOver;

    private void Awake()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (gameOver)
            return;

        gameOver = true;

        if (SkillUnlockUI.Instance != null)
            SkillUnlockUI.Instance.ForceHide();

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

    ResetPlayer();

    if (GameManager.Instance != null &&
        GameManager.Instance.Player != null)
    {
        Health health =
            GameManager.Instance.Player.GetComponent<Health>();

        if (health != null)
            health.SetInvincible(true);
    }

    if (SceneLoader.Instance != null)
    {
        SceneLoader.Instance.ReloadSavedScene();
    }
    else
    {
        Debug.LogError("Không tìm thấy SceneLoader.");
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
        {
            health.ResetHealth();
        }

        Rigidbody2D rb =
            player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndSessionAndReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}