using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingSpawn : MonoBehaviour
{
    [SerializeField] private string startingSpawnID =
        "Spawn_Start";

    [SerializeField] private string gameplayScene =
        "Valley";

    private void Start()
    {
        if (GameManager.Instance == null)
            return;

        // Không spawn Player trong MainMenu
        if (SceneManager.GetActiveScene().name == "MainMenu")
            return;

        // Chỉ áp dụng StartingSpawn cho scene gameplay đầu tiên
        if (SceneManager.GetActiveScene().name != gameplayScene)
            return;

        // Nếu đã có spawn trước đó thì không ghi đè
        if (!string.IsNullOrWhiteSpace(
            GameManager.Instance.CurrentSpawnID))
        {
            return;
        }

        GameManager.Instance.SetStartingSpawn(
            startingSpawnID
        );

        GameManager.Instance.MovePlayerToSpawn(
            startingSpawnID
        );

        Debug.Log(
            $"Khởi tạo spawn đầu game tại " +
            $"{SceneManager.GetActiveScene().name} - " +
            $"{startingSpawnID}"
        );
    }
}