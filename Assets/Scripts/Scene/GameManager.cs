using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using System;

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
        StartCoroutine(MovePlayerRoutine());
    }

    IEnumerator MovePlayerRoutine()
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        SpawnPoint[] points =
            FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

        foreach (SpawnPoint p in points)
        {
            if (p.spawnID == nextSpawnPoint)
            {
                // Tắt physics tạm thời
                if (rb != null)
                {
                    rb.simulated = false;
                }

                player.transform.position = p.transform.position;

                Debug.Log("Spawn Player tại: " + player.transform.position);

                // Đợi 2 frame cho scene ổn định
                yield return null;
                yield return null;

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0;
                    rb.simulated = true;
                }

                yield break;
            }
        }

        Debug.LogError("Không tìm thấy SpawnPoint: " + nextSpawnPoint);
    }

}