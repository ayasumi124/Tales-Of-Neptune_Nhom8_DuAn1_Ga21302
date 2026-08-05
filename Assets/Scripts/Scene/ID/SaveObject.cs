using UnityEngine;

[RequireComponent(typeof(PersistentID))]
public abstract class SaveObject : MonoBehaviour
{
    protected PersistentID persistentID;

    public string SaveID
    {
        get
        {
            if (persistentID == null)
            {
                persistentID =
                    GetComponent<PersistentID>();
            }

            return persistentID != null
                ? persistentID.ID
                : string.Empty;
        }
    }

    public bool HasValidSaveID =>
        !string.IsNullOrWhiteSpace(
            SaveID
        );

    protected virtual void Awake()
    {
        persistentID =
            GetComponent<PersistentID>();

        if (persistentID == null)
        {
            Debug.LogError(
                $"{name} thiếu PersistentID.",
                this
            );
        }
    }

    protected string BuildSceneSaveID()
    {
        return gameObject.scene.name +
               "_" +
               SaveID;
    }
}