using UnityEngine;

public class ScenePortal : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetSpawnID;

    [Header("Interaction")]
    [SerializeField] private bool requireKey = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInside;
    private bool used;

    private void Update()
    {
        if (!requireKey)
            return;

        if (playerInside &&
            !used &&
            Input.GetKeyDown(interactKey))
        {
            EnterPortal();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (!requireKey)
        {
            EnterPortal();
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }

    private void EnterPortal()
    {
        if (used)
            return;

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
                "Portal chưa điền Target Scene."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(targetSpawnID))
        {
            Debug.LogError(
                "Portal chưa điền Target Spawn ID."
            );
            return;
        }

        used = true;

        SceneLoader.Instance.LoadScene(
            targetScene,
            targetSpawnID
        );
    }
}