using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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

    yield return null; // đợi Awake/Start của scene

    GameManager.Instance.MovePlayerToSpawn();

    yield return FadeUI.Instance.FadeIn();
}
}