using System.Collections.Generic;
using UnityEngine;

public class GameSessionManager : MonoBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    // Những rương đã mở trong toàn bộ phiên chơi
    private readonly HashSet<string> openedChests =
        new HashSet<string>();

    // Enemy đã chết, lưu riêng theo từng scene
    private readonly Dictionary<string, HashSet<string>>
        deadEnemiesByScene =
            new Dictionary<string, HashSet<string>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // =========================
    // CHEST
    // =========================

    public void MarkChestOpened(string chestID)
    {
        if (string.IsNullOrWhiteSpace(chestID))
            return;

        openedChests.Add(chestID);
    }

    public bool IsChestOpened(string chestID)
    {
        if (string.IsNullOrWhiteSpace(chestID))
            return false;

        return openedChests.Contains(chestID);
    }

    // =========================
    // ENEMY
    // =========================

    public void MarkEnemyDead(
        string sceneName,
        string enemyID)
    {
        if (string.IsNullOrWhiteSpace(sceneName) ||
            string.IsNullOrWhiteSpace(enemyID))
        {
            return;
        }

        if (!deadEnemiesByScene.ContainsKey(sceneName))
        {
            deadEnemiesByScene[sceneName] =
                new HashSet<string>();
        }

        deadEnemiesByScene[sceneName].Add(enemyID);
    }

    public bool IsEnemyDead(
        string sceneName,
        string enemyID)
    {
        if (string.IsNullOrWhiteSpace(sceneName) ||
            string.IsNullOrWhiteSpace(enemyID))
        {
            return false;
        }

        if (!deadEnemiesByScene.TryGetValue(
            sceneName,
            out HashSet<string> deadEnemies))
        {
            return false;
        }

        return deadEnemies.Contains(enemyID);
    }

    /// <summary>
    /// Dùng khi vào scene bằng Portal.
    /// Enemy trong scene đó sẽ được respawn.
    /// </summary>
    public void ResetEnemiesInScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
            return;

        deadEnemiesByScene.Remove(sceneName);
    }

    // =========================
    // RESET SESSION
    // =========================

    public void ResetEntireSession()
    {
        openedChests.Clear();
        deadEnemiesByScene.Clear();

        Debug.Log("Đã reset toàn bộ dữ liệu phiên chơi.");
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}