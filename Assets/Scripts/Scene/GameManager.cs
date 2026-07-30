using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private GameObject player;

    public GameObject Player => player;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(transform.root.gameObject);
            return;
        }

        Instance = this;

        // Giữ toàn bộ prefab Managers.
        DontDestroyOnLoad(transform.root.gameObject);

        if (player == null)
        {
            Players playerScript =
                transform.root.GetComponentInChildren<Players>(true);

            if (playerScript != null)
                player = playerScript.gameObject;
        }
    }

    public IEnumerator MovePlayerToSpawn(string spawnID)
    {
        if (player == null)
        {
            Debug.LogError("GameManager chưa có Player.");
            yield break;
        }

        SpawnPoint[] points =
            FindObjectsByType<SpawnPoint>(
                FindObjectsSortMode.None);

        SpawnPoint targetPoint = null;

        foreach (SpawnPoint point in points)
        {
            if (point.spawnID == spawnID)
            {
                targetPoint = point;
                break;
            }
        }

        if (targetPoint == null)
        {
            Debug.LogError(
                "Không tìm thấy SpawnPoint: " + spawnID);

            yield break;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        Players movement = player.GetComponent<Players>();
        PlayerDash dash = player.GetComponent<PlayerDash>();
        Attack attack = player.GetComponent<Attack>();

        // Tạm khóa Player khi dịch chuyển.
        if (movement != null)
            movement.enabled = false;

        if (dash != null)
            dash.enabled = false;

        if (attack != null)
            attack.CancelAttack();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        // Đặt đúng world position của SpawnPoint.
        player.transform.position =
            targetPoint.transform.position;

        Physics2D.SyncTransforms();

        // Chờ scene và physics ổn định.
        yield return null;
        yield return new WaitForFixedUpdate();

        // Đặt lại lần nữa để tránh physics đẩy sai vị trí.
        player.transform.position =
            targetPoint.transform.position;

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

        Debug.Log(
            "Player đã spawn tại " +
            spawnID +
            " - Position: " +
            player.transform.position);
    }
}