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
    SpawnPoint[] points = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

    foreach (SpawnPoint p in points)
    {
        if (p.spawnID == nextSpawnPoint)
        {
            player.transform.position = p.transform.position;
            Debug.Log("Spawn tại: " + p.spawnID);
            return;
        }
    }

    Debug.LogWarning("Không tìm thấy SpawnPoint: " + nextSpawnPoint);
}
}