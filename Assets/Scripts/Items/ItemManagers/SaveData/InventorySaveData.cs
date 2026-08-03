using System.IO;
using UnityEngine;

public static class InventorySaveSystem
{
    private const string FileName =
        "inventory_save.json";

    private static string SavePath =>
        Path.Combine(
            Application.persistentDataPath,
            FileName
        );

    public static bool Save(
        InventorySaveData data)
    {
        if (data == null)
            return false;

        try
        {
            string json =
                JsonUtility.ToJson(
                    data,
                    true
                );

            File.WriteAllText(
                SavePath,
                json
            );

            Debug.Log(
                $"Đã lưu Inventory:\n{SavePath}"
            );

            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                "Lỗi lưu Inventory: " +
                exception.Message
            );

            return false;
        }
    }

    public static InventorySaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log(
                "Chưa có file Inventory Save."
            );

            return null;
        }

        try
        {
            string json =
                File.ReadAllText(
                    SavePath
                );

            return JsonUtility
                .FromJson<InventorySaveData>(
                    json
                );
        }
        catch (System.Exception exception)
        {
            Debug.LogError(
                "Lỗi load Inventory: " +
                exception.Message
            );

            return null;
        }
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);

            Debug.Log(
                "Đã xóa Inventory Save."
            );
        }
    }
}