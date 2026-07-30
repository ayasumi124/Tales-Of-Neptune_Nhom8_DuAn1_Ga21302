using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject player;

    public GameObject Player => player;

    [Header("Respawn Data")]
    [SerializeField] private string currentScene;
    [SerializeField] private string currentSpawnID;

    public string CurrentScene => currentScene;
    public string CurrentSpawnID => currentSpawnID;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Xóa toàn bộ Managers bị tạo trùng
            Destroy(transform.root.gameObject);
            return;
        }

        Instance = this;

        // Giữ toàn bộ Managers, SceneLoader, FadeCanvas,
        // Player và các UI persistent qua scene
        DontDestroyOnLoad(transform.root.gameObject);

        if (string.IsNullOrWhiteSpace(currentScene))
        {
            currentScene = SceneManager.GetActiveScene().name;
        }
    }

    public void SetPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }

    public void SaveRespawnPoint(
        string sceneName,
        string spawnID)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            currentScene = sceneName;
        }

        if (!string.IsNullOrWhiteSpace(spawnID))
        {
            currentSpawnID = spawnID;
        }

        Debug.Log(
            $"Đã lưu Respawn: Scene = {currentScene}, " +
            $"Spawn = {currentSpawnID}"
        );
    }

    public void EndSessionAndReturnToMenu()
{
    Time.timeScale = 1f;

    if (GameSessionManager.Instance != null)
        GameSessionManager.Instance.ResetEntireSession();

    SceneManager.LoadScene("MainMenu");

    Destroy(transform.root.gameObject);
}
    public void SetStartingSpawn(
        string startingSpawnID)
    {
        currentScene =
            SceneManager.GetActiveScene().name;

        currentSpawnID = startingSpawnID;

        Debug.Log(
            $"Spawn bắt đầu: {currentScene} - " +
            $"{currentSpawnID}"
        );
    }

    public void MovePlayerToSpawn(string spawnID)
    {
        if (player == null)
        {
            Debug.LogError("GameManager chưa được gán Player.");
            return;
        }

        SpawnPoint spawnPoint = FindSpawnPoint(spawnID);

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"Không tìm thấy SpawnPoint có ID: {spawnID} " +
                $"trong scene {SceneManager.GetActiveScene().name}"
            );
            return;
        }

        Vector3 targetPosition = spawnPoint.transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        player.transform.position = targetPosition;

        Physics2D.SyncTransforms();

        if (rb != null)
        {
            rb.position = targetPosition;
            rb.simulated = true;
        }

        Debug.Log(
            $"Đã đưa Player tới SpawnPoint: {spawnID} " +
            $"tại vị trí {targetPosition}"
        );
    }
    private SpawnPoint FindSpawnPoint(
        string spawnID)
    {
        if (string.IsNullOrWhiteSpace(spawnID))
            return null;

        SpawnPoint[] spawnPoints =
            FindObjectsByType<SpawnPoint>(
                FindObjectsSortMode.None
            );

        foreach (SpawnPoint spawn in spawnPoints)
        {
            if (spawn.SpawnID == spawnID)
            {
                return spawn;
            }
        }

        return null;
    }
}