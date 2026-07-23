using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManagement : MonoBehaviour
{
    public GameObject gameOverPanel;

    private bool gameOver = false;
    void Start()
{
    gameOverPanel.SetActive(false);
}
    public void ShowGameOver()
{
    if (gameOver)
        return;

    gameOver = true;

    gameOverPanel.SetActive(true);

    //Time.timeScale = 0f;
}

    public void PlayAgain()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}