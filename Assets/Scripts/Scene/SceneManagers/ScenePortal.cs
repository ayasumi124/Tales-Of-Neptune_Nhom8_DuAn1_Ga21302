using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    public string sceneToLoad;
    public string spawnPointID;

    bool loading;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (loading) return;

        if (!other.CompareTag("Player"))
            return;

        loading = true;

        SceneLoader.Instance.LoadScene(sceneToLoad, spawnPointID);
    }
}