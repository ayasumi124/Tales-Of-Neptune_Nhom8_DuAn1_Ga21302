using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class PersistentID : MonoBehaviour
{
    [SerializeField]
    private string uniqueID;

    public string ID => uniqueID;

    public bool HasValidID =>
        !string.IsNullOrWhiteSpace(uniqueID);

#if UNITY_EDITOR

    private bool isValidating;

    private void OnValidate()
    {
        if (Application.isPlaying ||
            isValidating)
        {
            return;
        }

        /*
         * Không lưu ID trực tiếp trên Prefab Asset.
         * Mỗi instance trong scene sẽ có ID riêng.
         */
        if (PrefabUtility.IsPartOfPrefabAsset(
                gameObject))
        {
            return;
        }

        EnsureUniqueID();
    }

    [ContextMenu("Generate New Persistent ID")]
    public void GenerateNewID()
    {
        if (Application.isPlaying)
            return;

        AssignNewID();
    }

    private void EnsureUniqueID()
    {
        isValidating = true;

        bool needsNewID =
            string.IsNullOrWhiteSpace(
                uniqueID
            ) ||
            IsDuplicateInLoadedScenes();

        if (needsNewID)
        {
            AssignNewID();
        }

        isValidating = false;
    }

    private bool IsDuplicateInLoadedScenes()
    {
        if (string.IsNullOrWhiteSpace(uniqueID))
            return false;

        PersistentID[] allIDs =
            FindObjectsByType<PersistentID>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        foreach (PersistentID other
                 in allIDs)
        {
            if (other == null ||
                other == this)
            {
                continue;
            }

            if (other.uniqueID == uniqueID)
            {
                return true;
            }
        }

        return false;
    }

    private void AssignNewID()
    {
        uniqueID =
            System.Guid.NewGuid()
                .ToString("N");

        EditorUtility.SetDirty(this);

        if (gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(
                gameObject.scene
            );
        }

        Debug.Log(
            $"{name} nhận Persistent ID mới:\n" +
            uniqueID,
            this
        );
    }

#endif
}