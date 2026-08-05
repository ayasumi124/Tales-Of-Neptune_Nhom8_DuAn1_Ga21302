#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

public static class GeneratePersistentID
{
    [MenuItem("Tools/Generate Persistent IDs")]
    public static void Generate()
    {
        PersistentID[] ids =
            Object.FindObjectsByType<PersistentID>(
                FindObjectsSortMode.None);

        int count = 0;

        foreach (PersistentID id in ids)
        {
            SerializedObject so =
                new SerializedObject(id);

            SerializedProperty property =
                so.FindProperty("uniqueID");

            property.stringValue =
                System.Guid.NewGuid().ToString();

            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(id);

            count++;
        }

        AssetDatabase.SaveAssets();

        Debug.Log($"Generated {count} Persistent IDs.");
    }
}

#endif