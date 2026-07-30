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
        Time.timeScale = 1f;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        ResetPlayer();

        if (FadeUI.Instance != null)
            FadeUI.Instance.ForceTransparent();

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.ReloadCurrentScene("Start");
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
            Debug.LogError("Không tìm thấy Player trong GameManager.");
            return;
        }

        GameObject player = GameManager.Instance.Player;

        player.SetActive(true);

        Health health = player.GetComponent<Health>();
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Animator animator = player.GetComponent<Animator>();
        Players movement = player.GetComponent<Players>();
        PlayerDash dash = player.GetComponent<PlayerDash>();
        Attack attack = player.GetComponent<Attack>();

        if (health != null)
            health.ResetHealth();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }

        if (movement != null)
            movement.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (attack != null)
        {
            attack.enabled = true;
            attack.CancelAttack();
        }

        if (animator != null)
        {
            animator.speed = 1f;
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        gameOver = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (FadeUI.Instance != null)
            FadeUI.Instance.ForceTransparent();

        SceneManager.LoadScene("MainMenu");
    }
}