using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance
    {
        get;
        private set;
    }

    [Header("Fade")]
    [SerializeField]
    private CanvasGroup fadeCanvasGroup;

    [SerializeField]
    private float fadeDuration = 0.5f;

    [Header("Loading UI")]
    [SerializeField]
    private TextMeshProUGUI loadingText;

    [Tooltip(
        "Thời gian tối thiểu giữ màn hình Loading."
    )]
    [SerializeField]
    private float loadingDelay = 2f;

    [Tooltip(
        "Thời gian chờ trước khi trả lại điều khiển."
    )]
    [SerializeField]
    private float spawnWaitTime = 0.3f;

    private Canvas fadeCanvas;

    private bool portalFadeStarted;
    private bool isLoading;

    private Coroutine portalFadeCoroutine;
    private Coroutine loadingCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (fadeCanvasGroup != null)
        {
            fadeCanvas =
                fadeCanvasGroup
                    .GetComponentInParent<Canvas>();

            if (fadeCanvas != null)
            {
                fadeCanvas.overrideSorting = true;
                fadeCanvas.sortingOrder = 9999;
            }

            fadeCanvasGroup
                .gameObject
                .SetActive(true);
        }

        if (loadingText != null)
        {
            loadingText
                .gameObject
                .SetActive(false);
        }

        ForceTransparent();
    }

    public void LoadScene(
        string sceneName,
        string spawnID)
    {
        if (isLoading)
            return;

        if (!ValidateLoadData(
                sceneName,
                spawnID))
        {
            return;
        }

        GameManager.Instance.SaveRespawnPoint(
            sceneName,
            spawnID
        );

        StartCoroutine(
            LoadSceneRoutine(
                sceneName,
                spawnID
            )
        );
    }

    public void ReloadSavedScene()
    {
        if (isLoading)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy GameManager."
            );

            return;
        }

        string sceneName =
            GameManager.Instance.CurrentScene;

        string spawnID =
            GameManager.Instance.CurrentSpawnID;

        if (string.IsNullOrWhiteSpace(
                sceneName))
        {
            sceneName =
                SceneManager
                    .GetActiveScene()
                    .name;
        }

        if (string.IsNullOrWhiteSpace(
                spawnID))
        {
            Debug.LogError(
                "Chưa có CurrentSpawnID " +
                "để hồi sinh."
            );

            return;
        }

        StartCoroutine(
            LoadSceneRoutine(
                sceneName,
                spawnID
            )
        );
    }

    public void ReloadCurrentScene(
        string spawnID)
    {
        if (isLoading)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy GameManager."
            );

            return;
        }

        string sceneName =
            SceneManager
                .GetActiveScene()
                .name;

        GameManager.Instance.SaveRespawnPoint(
            sceneName,
            spawnID
        );

        StartCoroutine(
            LoadSceneRoutine(
                sceneName,
                spawnID
            )
        );
    }

    public void BeginPortalFade()
    {
        if (portalFadeStarted)
            return;

        portalFadeStarted = true;

        if (portalFadeCoroutine != null)
        {
            StopCoroutine(
                portalFadeCoroutine
            );
        }

        portalFadeCoroutine =
            StartCoroutine(
                BeginPortalFadeRoutine()
            );
    }

    public void LoadSceneAfterPortalFade(
        string sceneName,
        string spawnID)
    {
        if (isLoading)
            return;

        if (!ValidateLoadData(
                sceneName,
                spawnID))
        {
            portalFadeStarted = false;
            return;
        }

        GameManager.Instance.SaveRespawnPoint(
            sceneName,
            spawnID
        );

        StartCoroutine(
            LoadSceneAfterPortalRoutine(
                sceneName,
                spawnID
            )
        );
    }

    private bool ValidateLoadData(
        string sceneName,
        string spawnID)
    {
        if (string.IsNullOrWhiteSpace(
                sceneName))
        {
            Debug.LogError(
                "Tên scene cần load đang rỗng."
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                spawnID))
        {
            Debug.LogError(
                "Spawn ID cần load đang rỗng."
            );

            return false;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy GameManager."
            );

            return false;
        }

        return true;
    }

    private IEnumerator BeginPortalFadeRoutine()
    {
        if (fadeCanvasGroup == null)
            yield break;

        PrepareFadeCanvas();

        yield return FadeTo(1f);

        portalFadeCoroutine = null;
    }

    private IEnumerator LoadSceneRoutine(
        string sceneName,
        string spawnID)
    {
        isLoading = true;
        Time.timeScale = 1f;

        LockPlayer();

        if (fadeCanvasGroup != null)
        {
            PrepareFadeCanvas();
            yield return FadeTo(1f);
        }
        else
        {
            Debug.LogError(
                "SceneLoader chưa được gán " +
                "Fade Canvas Group."
            );
        }

        ShowLoading(true);

        yield return LoadSceneAsync(
            sceneName
        );

        if (GameManager.Instance != null)
        {
            GameManager.Instance
                .MovePlayerToSpawn(
                    spawnID
                );
        }

        yield return WaitForSceneReady();

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, loadingDelay)
        );

        ShowLoading(false);

        if (fadeCanvasGroup != null)
        {
            yield return FadeTo(0f);
            DisableFadeRaycast();
        }

        yield return new WaitForSecondsRealtime(
            0.5f
        );

        UnlockPlayer();

        portalFadeStarted = false;
        isLoading = false;
    }

    private IEnumerator LoadSceneAfterPortalRoutine(
        string sceneName,
        string spawnID)
    {
        isLoading = true;
        Time.timeScale = 1f;

        LockPlayer();

        /*
         * Chờ fade-out từ ScenePortal hoàn thành.
         */
        if (portalFadeCoroutine != null)
        {
            yield return portalFadeCoroutine;
            portalFadeCoroutine = null;
        }
        else if (fadeCanvasGroup != null)
        {
            while (
                fadeCanvasGroup.alpha < 0.99f)
            {
                yield return null;
            }
        }

        ShowLoading(true);

        yield return LoadSceneAsync(
            sceneName
        );

        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            Debug.LogError(
                "Không tìm thấy GameManager " +
                "hoặc Player."
            );

            FinishFailedLoad();
            yield break;
        }

        SpawnPoint spawnPoint =
            GameManager.Instance.FindSpawnPoint(
                spawnID
            );

        if (spawnPoint == null)
        {
            Debug.LogError(
                $"Không tìm thấy SpawnPoint: " +
                $"{spawnID}"
            );

            FinishFailedLoad();
            yield break;
        }

        GameObject playerObject =
            GameManager.Instance.Player;

        Players player =
            playerObject.GetComponent<Players>();

        Rigidbody2D rb =
            playerObject.GetComponent<Rigidbody2D>();

        /*
         * Nếu có Exit Start Point:
         * Player xuất hiện tại tâm portal.
         *
         * Nếu không gán:
         * Player xuất hiện thẳng tại SpawnPoint.
         */
        Vector3 startPosition =
            spawnPoint.HasPortalExit
                ? spawnPoint.ExitStartPosition
                : spawnPoint.FinalPosition;

        Vector3 finalPosition =
            spawnPoint.FinalPosition;

        GameManager.Instance
            .MovePlayerToPosition(
                startPosition
            );

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;
        }

        yield return WaitForSceneReady();

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, loadingDelay)
        );

        ShowLoading(false);

        /*
         * Không có Exit Start Point hoặc
         * hai điểm trùng nhau:
         * chỉ fade-in bình thường.
         */
        if (!spawnPoint.HasPortalExit ||
            spawnPoint.ExitDistance <= 0.01f ||
            player == null)
        {
            GameManager.Instance
                .MovePlayerToPosition(
                    finalPosition
                );

            if (fadeCanvasGroup != null)
            {
                yield return FadeTo(0f);
                DisableFadeRaycast();
            }

            yield return new WaitForSecondsRealtime(
                Mathf.Max(
                    0f,
                    spawnWaitTime
                )
            );

            UnlockPlayer();

            portalFadeStarted = false;
            isLoading = false;

            yield break;
        }

        Vector2 exitDirection =
            spawnPoint.ExitDirection;

        float exitDuration =
            spawnPoint.ExitDuration;

        /*
         * Player bước ra khỏi portal,
         * đồng thời màn hình sáng dần.
         *
         * IsControlLocked vẫn true,
         * nhưng AutoMove được FixedUpdate
         * ưu tiên trước LockControl.
         */
        player.AutoWalk(
            exitDirection,
            spawnPoint.ExitSpeed
        );

        Coroutine fadeInCoroutine = null;

        if (fadeCanvasGroup != null)
        {
            fadeInCoroutine =
                StartCoroutine(
                    FadeTo(0f)
                );
        }

        yield return new WaitForSecondsRealtime(
            exitDuration
        );

        player.StopAutoWalk();

        /*
         * Đặt chính xác tại SpawnPoint
         * để tránh sai số Rigidbody.
         */
        GameManager.Instance
            .MovePlayerToPosition(
                finalPosition
            );

        if (fadeInCoroutine != null)
        {
            yield return fadeInCoroutine;
        }

        DisableFadeRaycast();

        yield return new WaitForSecondsRealtime(
            Mathf.Max(
                0f,
                spawnWaitTime
            )
        );

        UnlockPlayer();

        portalFadeStarted = false;
        isLoading = false;
    }

    private IEnumerator LoadSceneAsync(
        string sceneName)
    {
        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName
            );

        if (operation == null)
        {
            Debug.LogError(
                $"Không thể load scene: " +
                $"{sceneName}"
            );

            yield break;
        }

        while (!operation.isDone)
            yield return null;

        /*
         * Chờ Awake, OnEnable và Start
         * của object scene mới.
         */
        yield return null;
        yield return new WaitForEndOfFrame();
    }

    private IEnumerator WaitForSceneReady()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
    }

    private void LockPlayer()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            return;
        }

        GameObject playerObject =
            GameManager.Instance.Player;

        Players player =
            playerObject.GetComponent<Players>();

        if (player != null)
        {
            /*
             * Đảm bảo không còn AutoWalk cũ.
             */
            player.StopAutoWalk();
            player.LockControl();
        }

        Attack attack =
            playerObject.GetComponent<Attack>();

        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        PlayerDash dash =
            playerObject.GetComponent<PlayerDash>();

        if (dash != null)
            dash.enabled = false;

        Rigidbody2D rb =
            playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;
        }

        Health health =
            playerObject.GetComponent<Health>();

        if (health != null)
            health.SetInvincible(true);
    }

    private void UnlockPlayer()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            return;
        }

        GameObject playerObject =
            GameManager.Instance.Player;

        Health health =
            playerObject.GetComponent<Health>();

        if (health != null &&
            health.IsDead)
        {
            return;
        }

        Rigidbody2D rb =
            playerObject.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity = 0f;
        }

        Players player =
            playerObject.GetComponent<Players>();

        if (player != null)
        {
            player.StopAutoWalk();
            player.UnlockControl();
        }

        Attack attack =
            playerObject.GetComponent<Attack>();

        if (attack != null)
            attack.enabled = true;

        PlayerDash dash =
            playerObject.GetComponent<PlayerDash>();

        if (dash != null)
            dash.enabled = true;

        if (health != null)
            health.SetInvincible(false);
    }

    private void PrepareFadeCanvas()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup
            .gameObject
            .SetActive(true);

        fadeCanvasGroup
            .transform
            .SetAsLastSibling();

        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = false;
    }

    private void DisableFadeRaycast()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;
    }

    private IEnumerator FadeTo(
        float targetAlpha)
    {
        if (fadeCanvasGroup == null)
            yield break;

        float startAlpha =
            fadeCanvasGroup.alpha;

        if (fadeDuration <= 0f)
        {
            fadeCanvasGroup.alpha =
                targetAlpha;

            yield break;
        }

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            fadeCanvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

            yield return null;
        }

        fadeCanvasGroup.alpha =
            targetAlpha;
    }

    private void ShowLoading(bool show)
    {
        if (loadingText == null)
            return;

        loadingText
            .gameObject
            .SetActive(show);

        if (show)
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(
                    loadingCoroutine
                );
            }

            loadingCoroutine =
                StartCoroutine(
                    LoadingAnimation()
                );
        }
        else
        {
            if (loadingCoroutine != null)
            {
                StopCoroutine(
                    loadingCoroutine
                );

                loadingCoroutine = null;
            }

            loadingText.text =
                "Loading...";
        }
    }

    private IEnumerator LoadingAnimation()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount++;

            if (dotCount > 3)
                dotCount = 0;

            loadingText.text =
                "Loading" +
                new string('.', dotCount);

            yield return
                new WaitForSecondsRealtime(
                    0.35f
                );
        }
    }

    private void FinishFailedLoad()
    {
        ShowLoading(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            DisableFadeRaycast();
        }

        UnlockPlayer();

        portalFadeStarted = false;
        isLoading = false;
    }

    public void ForceTransparent()
    {
        if (fadeCanvasGroup == null)
            return;

        fadeCanvasGroup
            .gameObject
            .SetActive(true);

        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
        fadeCanvasGroup.interactable = false;

        portalFadeStarted = false;
        isLoading = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}