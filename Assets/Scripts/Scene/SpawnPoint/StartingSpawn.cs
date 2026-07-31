using UnityEngine;
using UnityEngine.SceneManagement;

public class StartingSpawn : MonoBehaviour
{
    [SerializeField] private string startingSpawnID =
        "Spawn_Start";

    private void Start()
    {
        if (GameManager.Instance == null)
            return;

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