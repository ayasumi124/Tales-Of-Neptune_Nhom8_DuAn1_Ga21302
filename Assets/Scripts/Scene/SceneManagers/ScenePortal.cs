using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Destination")]
    public string sceneToLoad;
    public string spawnPointID;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("Không tìm thấy SceneLoader.");
            return;
        }

        SceneLoader.Instance.LoadScene(
            sceneToLoad,
            spawnPointID);
    }
}