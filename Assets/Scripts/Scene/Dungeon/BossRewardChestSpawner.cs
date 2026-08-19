using UnityEngine;

public class BossRewardChestSpawner : MonoBehaviour
{
    [Header("Boss")]
    [SerializeField]
    private Transform bossContainer;

    [Header("Reward Chests")]
    [SerializeField]
    private GameObject[] chestPrefabs;

    [Header("Spawn Points")]
    [SerializeField]
    private Transform[] spawnPoints;

    [Header("Settings")]
    [SerializeField]
    private float checkInterval = 0.25f;

    [SerializeField]
    private float spawnDelay = 1f;

    private bool bossWasAlive;
    private bool rewardSpawned;

    private float checkTimer;
    private float spawnTimer;

    private bool waitingToSpawn;

    private void Start()
    {
        bossWasAlive =
            HasLivingBoss();

        checkTimer = 0f;
    }

    private void Update()
    {
        if (rewardSpawned)
            return;

        if (waitingToSpawn)
        {
            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                SpawnRewards();
            }

            return;
        }

        checkTimer -= Time.deltaTime;

        if (checkTimer > 0f)
            return;

        checkTimer = checkInterval;

        bool bossAlive =
            HasLivingBoss();

        /*
         * Chỉ spawn reward nếu trước đó
         * thực sự có Boss sống.
         *
         * Tránh trường hợp scene vừa load
         * mà container rỗng -> tự spawn chest.
         */
        if (bossWasAlive &&
            !bossAlive)
        {
            waitingToSpawn = true;

            spawnTimer =
                Mathf.Max(
                    0f,
                    spawnDelay
                );
        }

        if (bossAlive)
        {
            bossWasAlive = true;
        }
    }

    private bool HasLivingBoss()
    {
        if (bossContainer == null)
            return false;

        for (int i = 0;
             i < bossContainer.childCount;
             i++)
        {
            Transform child =
                bossContainer.GetChild(i);

            if (child == null ||
                !child.gameObject.activeInHierarchy)
            {
                continue;
            }

            EnermyHealth health =
                child.GetComponentInChildren<
                    EnermyHealth
                >();

            if (health != null)
            {
                return true;
            }
        }

        return false;
    }

    private void SpawnRewards()
    {
        if (rewardSpawned)
            return;

        rewardSpawned = true;
        waitingToSpawn = false;

        if (chestPrefabs == null ||
            spawnPoints == null)
        {
            return;
        }

        int count =
            Mathf.Min(
                chestPrefabs.Length,
                spawnPoints.Length
            );

        for (int i = 0;
             i < count;
             i++)
        {
            GameObject prefab =
                chestPrefabs[i];

            Transform point =
                spawnPoints[i];

            if (prefab == null ||
                point == null)
            {
                continue;
            }

            Instantiate(
                prefab,
                point.position,
                point.rotation
            );
        }
    }

    private void OnValidate()
    {
        checkInterval =
            Mathf.Max(
                0.05f,
                checkInterval
            );

        spawnDelay =
            Mathf.Max(
                0f,
                spawnDelay
            );
    }
}