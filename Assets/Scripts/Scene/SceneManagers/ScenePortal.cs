using System.Collections;
using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnID;

    [Header("Interaction")]
    [SerializeField] private bool requireKey = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Enter Portal")]
    [SerializeField] private Vector2 enterDirection =
        Vector2.up;

    [SerializeField] private float enterSpeed = 1.8f;
    [SerializeField] private float enterDuration = 0.45f;

    private bool playerInside;
    private bool used;

    private void OnEnable()
    {
        /*
         * Mỗi khi scene hoặc portal được bật lại,
         * cho phép portal hoạt động từ đầu.
         */
        used = false;
        playerInside = false;
    }

    private void Update()
    {
        if (!requireKey)
            return;

        if (!playerInside || used)
            return;

        if (Input.GetKeyDown(interactKey))
            EnterPortal();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (!requireKey && !used)
            EnterPortal();
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        /*
         * Phòng trường hợp Player được teleport
         * trực tiếp vào Collider nên Enter không chạy.
         */
        playerInside = true;
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        /*
         * Sau khi Player rời portal,
         * portal được phép sử dụng lại.
         */
        used = false;
    }

    private void EnterPortal()
    {
        if (used)
        {
            Debug.Log(
                $"{name}: Portal đang bị used khóa."
            );

            return;
        }

        if (SceneLoader.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy SceneLoader."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            Debug.LogError(
                $"{name}: Chưa nhập Target Scene."
            );

            return;
        }

        if (string.IsNullOrWhiteSpace(targetSpawnID))
        {
            Debug.LogError(
                $"{name}: Chưa nhập Target Spawn ID."
            );

            return;
        }

        Debug.Log(
            $"{name}: Bắt đầu chuyển tới " +
            $"{targetScene} - {targetSpawnID}"
        );

        used = true;

        StartCoroutine(
            EnterPortalRoutine()
        );
    }

    private IEnumerator EnterPortalRoutine()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            /*
             * Nếu không có Player persistent,
             * vẫn thử load scene thông thường.
             */
            SceneLoader.Instance.LoadScene(
                targetScene,
                targetSpawnID
            );

            yield break;
        }

        GameObject playerObject =
            GameManager.Instance.Player;

        Players player =
            playerObject.GetComponent<Players>();

        Attack attack =
            playerObject.GetComponent<Attack>();

        PlayerDash dash =
            playerObject.GetComponent<PlayerDash>();

        Health health =
            playerObject.GetComponent<Health>();

        if (player == null)
        {
            Debug.LogError(
                "Player không có script Players."
            );

            used = false;
            yield break;
        }

        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        if (dash != null)
            dash.enabled = false;

        if (health != null)
            health.SetInvincible(true);

        Vector2 direction =
            enterDirection.sqrMagnitude > 0.001f
                ? enterDirection.normalized
                : player.LastDirection;

        player.AutoWalk(
            direction,
            Mathf.Max(0f, enterSpeed)
        );

        SceneLoader.Instance.BeginPortalFade();

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, enterDuration)
        );

        player.StopAutoWalk();
        player.LockControl();

        SceneLoader.Instance.LoadSceneAfterPortalFade(
            targetScene,
            targetSpawnID
        );
    }
}