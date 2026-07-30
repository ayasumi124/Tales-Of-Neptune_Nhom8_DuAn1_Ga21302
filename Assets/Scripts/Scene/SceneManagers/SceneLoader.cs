using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private Canvas fadeCanvas;
    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvas =
                fadeCanvasGroup.GetComponentInParent<Canvas>();

            if (fadeCanvas != null)
            {
                fadeCanvas.overrideSorting = true;
                fadeCanvas.sortingOrder = 9999;
            }

            fadeCanvasGroup.gameObject.SetActive(true);
        }

        ForceTransparent();
    }

    public void LoadScene(
        string sceneName,
        string spawnID)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("Tên scene cần load đang rỗng.");
            return;
        }

        if (string.IsNullOrWhiteSpace(spawnID))
        {
            Debug.LogError("Spawn ID cần load đang rỗng.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy GameManager.");
            return;
        }

        GameManager.Instance.SaveRespawnPoint(
            sceneName,
            spawnID
        );

        StartCoroutine(
            LoadSceneRoutine(sceneName, spawnID)
        );
    }

    public void ReloadSavedScene()
    {
        if (isLoading)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy GameManager.");
            return;
        }

        string sceneName =
            GameManager.Instance.CurrentScene;

        string spawnID =
            GameManager.Instance.CurrentSpawnID;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            sceneName =
                SceneManager.GetActiveScene().name;
        }

        if (string.IsNullOrWhiteSpace(spawnID))
        {
            Debug.LogError(
                "Chưa có CurrentSpawnID để hồi sinh."
            );
            return;
        }

        StartCoroutine(
            LoadSceneRoutine(sceneName, spawnID)
        );
    }

    public void ReloadCurrentScene(
        string spawnID)
    {
        if (isLoading)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError("Không tìm thấy GameManager.");
            return;
        }

        string sceneName =
            SceneManager.GetActiveScene().name;

        GameManager.Instance.SaveRespawnPoint(
            sceneName,
            spawnID
        );

        StartCoroutine(
            LoadSceneRoutine(sceneName, spawnID)
        );
    }

    private IEnumerator LoadSceneRoutine(
    string sceneName,
    string spawnID)
{
    isLoading = true;

    Time.timeScale = 1f;

    Health playerHealth = null;

    if (GameManager.Instance != null &&
        GameManager.Instance.Player != null)
    {
        playerHealth =
            GameManager.Instance.Player.GetComponent<Health>();

        if (playerHealth != null)
            playerHealth.SetInvincible(true);
    }

    if (fadeCanvasGroup != null)
    {
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.transform.SetAsLastSibling();

        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = false;

        yield return FadeTo(1f);
    }
    else
    {
        Debug.LogError(
            "SceneLoader chưa được gán Fade Canvas Group."
        );
    }

    AsyncOperation operation =
        SceneManager.LoadSceneAsync(sceneName);

    while (!operation.isDone)
    {
        yield return null;
    }

    // Chờ các object trong scene mới chạy Awake/OnEnable.
    yield return null;

    if (GameManager.Instance != null)
    {
        GameManager.Instance.MovePlayerToSpawn(
            spawnID
        );
    }

    // Xóa vận tốc phát sinh trong lúc chuyển scene.
    if (GameManager.Instance != null &&
        GameManager.Instance.Player != null)
    {
        Rigidbody2D rb =
            GameManager.Instance.Player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // Chờ physics và camera cập nhật vị trí mới.
    yield return new WaitForFixedUpdate();
    yield return new WaitForEndOfFrame();

    if (fadeCanvasGroup != null)
    {
        yield return FadeTo(0f);

        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    // Chờ thêm một khoảng ngắn để tránh enemy đánh ngay
    // trong frame màn hình vừa hiện lại.
    yield return new WaitForSecondsRealtime(0.25f);

    if (playerHealth != null)
        playerHealth.SetInvincible(false);

    isLoading = false;
}

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha =
            fadeCanvasGroup.alpha;

        if (fadeDuration <= 0f)
        {
            fadeCanvasGroup.alpha = targetAlpha;
            yield break;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(timer / fadeDuration);

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    public void ForceTransparent()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        isLoading = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}