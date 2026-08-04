using System.Collections.Generic;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance
    {
        get;
        private set;
    }

    /*
     * Lưu FullChestID:
     * SceneName_ChestID
     */
    private readonly HashSet<string> openedChests =
        new HashSet<string>();

    /*
     * Enemy đã chết, lưu riêng theo từng scene.
     */
    private readonly Dictionary<string, HashSet<string>>
        deadEnemiesByScene =
            new Dictionary<string, HashSet<string>>();

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        /*
         * Giữ dữ liệu rương và enemy khi load scene.
         */
        DontDestroyOnLoad(gameObject);
    }

    // =====================================================
    // CHEST
    // =====================================================

    public bool MarkChestOpened(
        string fullChestID)
    {
        if (string.IsNullOrWhiteSpace(
                fullChestID))
        {
            Debug.LogError(
                "Không thể lưu rương vì FullChestID đang rỗng."
            );

            return false;
        }

        bool added =
            openedChests.Add(
                fullChestID
            );

        if (added)
        {
            Debug.Log(
                $"Đã lưu trạng thái rương: {fullChestID}"
            );
        }

        return added;
    }

    public bool IsChestOpened(
        string fullChestID)
    {
        if (string.IsNullOrWhiteSpace(
                fullChestID))
        {
            return false;
        }

        return openedChests.Contains(
            fullChestID
        );
    }

    public bool ResetChest(
        string fullChestID)
    {
        if (string.IsNullOrWhiteSpace(
                fullChestID))
        {
            return false;
        }

        bool removed =
            openedChests.Remove(
                fullChestID
            );

        if (removed)
        {
            Debug.Log(
                $"Đã reset rương: {fullChestID}"
            );
        }

        return removed;
    }

    public void ResetAllChests()
    {
        openedChests.Clear();

        Debug.Log(
            "Đã reset toàn bộ trạng thái rương."
        );
    }

    // =====================================================
    // ENEMY
    // =====================================================

    public void MarkEnemyDead(
        string sceneName,
        string enemyID)
    {
        if (string.IsNullOrWhiteSpace(
                sceneName) ||
            string.IsNullOrWhiteSpace(
                enemyID))
        {
            return;
        }

        if (!deadEnemiesByScene.ContainsKey(
                sceneName))
        {
            deadEnemiesByScene[sceneName] =
                new HashSet<string>();
        }

        deadEnemiesByScene[sceneName].Add(
            enemyID
        );
    }

    public bool IsEnemyDead(
        string sceneName,
        string enemyID)
    {
        if (string.IsNullOrWhiteSpace(
                sceneName) ||
            string.IsNullOrWhiteSpace(
                enemyID))
        {
            return false;
        }

        if (!deadEnemiesByScene.TryGetValue(
                sceneName,
                out HashSet<string> deadEnemies))
        {
            return false;
        }

        return deadEnemies.Contains(
            enemyID
        );
    }

    public void ResetEnemiesInScene(
        string sceneName)
    {
        if (string.IsNullOrWhiteSpace(
                sceneName))
        {
            return;
        }

        deadEnemiesByScene.Remove(
            sceneName
        );
    }

    // =====================================================
    // RESET SESSION
    // =====================================================

    public void ResetEntireSession()
    {
        openedChests.Clear();
        deadEnemiesByScene.Clear();

        Debug.Log(
            "Đã reset toàn bộ dữ liệu phiên chơi."
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}