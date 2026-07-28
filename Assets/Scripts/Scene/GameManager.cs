using UnityEngine;
using Unity.Cinemachine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject player;

    public string nextSpawnPoint;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(transform.root.gameObject);
        }
    }

    public void SetPlayer(GameObject p)
    {
        player = p;
    }

    public void MovePlayerToSpawn()
    {
        Debug.Log("Spawn cần tìm: " + nextSpawnPoint);

        SpawnPoint[] points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint p in points)
        {
            Debug.Log("Spawn hiện có: " + p.spawnID);

            if (p.spawnID == nextSpawnPoint)
            {
                player.transform.position = p.transform.position;
                Debug.Log(player.transform.position);
                Debug.Log("Đã spawn tại " + p.spawnID);
                return;
            }
        }

        Debug.LogError("KHÔNG TÌM THẤY SPAWN!");
    }
}