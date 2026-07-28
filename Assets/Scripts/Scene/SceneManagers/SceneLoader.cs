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
        yield return null;

        GameManager.Instance.MovePlayerToSpawn();
        CinemachineCamera cam = FindFirstObjectByType<CinemachineCamera>();

        if (cam != null)
        {
            cam.Target.TrackingTarget = GameManager.Instance.player.transform;
        }

        yield return null;

        yield return FadeUI.Instance.FadeIn();
    }
}