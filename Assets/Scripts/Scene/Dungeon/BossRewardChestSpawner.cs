using System.Collections;
using UnityEngine;

public class BossRewardChestSpawner : MonoBehaviour
{
    // =====================================================
    // BOSS
    // =====================================================

    [Header("Boss")]
    [Tooltip(
        "Kéo đúng EnermyHealth của Boss vào đây."
    )]
    [SerializeField]
    private EnermyHealth targetBoss;


    // =====================================================
    // REWARD
    // =====================================================

    [Header("Reward Chests")]
    [SerializeField]
    private GameObject[] chestPrefabs;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform[] spawnPoints;


    // =====================================================
    // SETTINGS
    // =====================================================

    [Header("Settings")]
    [Min(0f)]
    [SerializeField]
    private float spawnDelay = 1f;


    // =====================================================
    // STATE
    // =====================================================

    private bool rewardSpawned;
    private Coroutine spawnCoroutine;


    // =====================================================
    // EVENT
    // =====================================================

    private void OnEnable()
    {
        EnermyHealth.OnBossDied +=
            HandleBossDied;
    }

    private void OnDisable()
    {
        EnermyHealth.OnBossDied -=
            HandleBossDied;
    }


    // =====================================================
    // BOSS DIED
    // =====================================================

    private void HandleBossDied(
        EnermyHealth deadBoss)
    {
        if (rewardSpawned)
            return;

        if (deadBoss == null)
            return;

        /*
         * Nếu đã gán Target Boss,
         * chỉ Boss đó mới được kích hoạt reward.
         */
        if (targetBoss != null &&
            deadBoss != targetBoss)
        {
            return;
        }

        Debug.Log(
            $"BossRewardChestSpawner nhận Boss chết: " +
            $"{deadBoss.name}"
        );

        if (spawnCoroutine != null)
        {
            StopCoroutine(
                spawnCoroutine
            );
        }

        spawnCoroutine =
            StartCoroutine(
                SpawnRewardRoutine()
            );
    }


    // =====================================================
    // DELAY
    // =====================================================

    private IEnumerator SpawnRewardRoutine()
    {
        if (spawnDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    spawnDelay
                );
        }

        SpawnRewards();

        spawnCoroutine = null;
    }


    // =====================================================
    // SPAWN
    // =====================================================

    private void SpawnRewards()
    {
        if (rewardSpawned)
            return;

        if (chestPrefabs == null ||
            chestPrefabs.Length == 0)
        {
            Debug.LogError(
                "BossRewardChestSpawner: " +
                "chưa gán Chest Prefabs.",
                this
            );

            return;
        }

        if (spawnPoints == null ||
            spawnPoints.Length == 0)
        {
            Debug.LogError(
                "BossRewardChestSpawner: " +
                "chưa gán Spawn Points.",
                this
            );

            return;
        }

        int count =
            Mathf.Min(
                chestPrefabs.Length,
                spawnPoints.Length
            );

        if (count <= 0)
            return;

        rewardSpawned = true;

        Debug.Log(
            $"Spawn {count} Boss Reward Chest."
        );

        for (int i = 0;
             i < count;
             i++)
        {
            GameObject chestPrefab =
                chestPrefabs[i];

            Transform spawnPoint =
                spawnPoints[i];

            if (chestPrefab == null)
            {
                Debug.LogWarning(
                    $"Chest Prefab [{i}] đang null.",
                    this
                );

                continue;
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning(
                    $"Spawn Point [{i}] đang null.",
                    this
                );

                continue;
            }

            GameObject chest =
                Instantiate(
                    chestPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

            Debug.Log(
                $"Spawn {chest.name} tại " +
                $"{spawnPoint.name}."
            );
        }
    }


    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        spawnDelay =
            Mathf.Max(
                0f,
                spawnDelay
            );
    }
}