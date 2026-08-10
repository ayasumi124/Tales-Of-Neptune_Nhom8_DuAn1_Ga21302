using System;
using UnityEngine;

public class DungeonKeyManager : MonoBehaviour
{
    public static DungeonKeyManager Instance
    {
        get;
        private set;
    }

    public int KeyCount
    {
        get;
        private set;
    }

    public bool HasKey =>
        KeyCount > 0;

    public static event Action<int>
        OnKeyCountChanged;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GiveKey(
        int amount = 1)
    {
        if (amount <= 0)
            return;

        KeyCount += amount;

        OnKeyCountChanged?.Invoke(
            KeyCount
        );

        Debug.Log(
            $"Nhận Dungeon Key x{amount}. " +
            $"Hiện có: {KeyCount}"
        );
    }

    public bool UseKey()
    {
        if (KeyCount <= 0)
        {
            Debug.Log(
                "Không có Dungeon Key."
            );

            return false;
        }

        KeyCount--;

        OnKeyCountChanged?.Invoke(
            KeyCount
        );

        Debug.Log(
            $"Đã dùng Dungeon Key. " +
            $"Còn lại: {KeyCount}"
        );

        return true;
    }

    public void ResetKeys()
    {
        KeyCount = 0;

        OnKeyCountChanged?.Invoke(
            KeyCount
        );
    }
}