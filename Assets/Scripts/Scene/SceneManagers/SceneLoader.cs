using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void LoadScene(string sceneName, string spawnID)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadRoutine(sceneName, spawnID));
    }

    private IEnumerator LoadRoutine(
        string sceneName,
        string spawnID)
    {
        isLoading = true;
        Time.timeScale = 1f;

        Debug.Log("1. Fade Out");

        if (FadeUI.Instance != null)
            yield return FadeUI.Instance.FadeOut();

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
            yield return null;

        yield return null;
        yield return null;

        if (GameManager.Instance != null)
        {
            yield return GameManager.Instance
                .MovePlayerToSpawn(spawnID);
        }

        yield return new WaitForEndOfFrame();

        if (FadeUI.Instance != null)
            yield return FadeUI.Instance.FadeIn();

        isLoading = false;
    }

    public void ReloadCurrentScene(string spawnID)
    {
        LoadScene(
            SceneManager.GetActiveScene().name,
            spawnID
        );
    }
}