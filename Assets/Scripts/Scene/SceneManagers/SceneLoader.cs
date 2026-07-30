using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;


public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    bool loading;

    void Awake()
    {
        Instance = this;
    }

    public void LoadScene(string sceneName, string spawnID)
    {
        if (loading)
            return;

        StartCoroutine(Load(sceneName, spawnID));
    }

    IEnumerator Load(string sceneName, string spawnID)
{
    GameManager.Instance.nextSpawnPoint = spawnID;

    yield return FadeUI.Instance.FadeOut();

    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

    while (!op.isDone)
        yield return null;

    // đợi 2 frame
    yield return null;
    yield return null;

    GameManager.Instance.MovePlayerToSpawn();

    yield return new WaitForEndOfFrame();

    Debug.Log("Player sau spawn = " + GameManager.Instance.player.transform.position);

    yield return FadeUI.Instance.FadeIn();
}
}