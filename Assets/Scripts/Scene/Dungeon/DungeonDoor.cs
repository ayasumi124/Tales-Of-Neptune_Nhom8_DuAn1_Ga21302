using System.Collections;
using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject keyIcon;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode interactKey =
        KeyCode.E;

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip openSound;

    [SerializeField]
    private AudioClip lockedSound;

    [Header("Door")]
    [Tooltip(
        "Thời gian chờ sau khi phát SFX " +
        "rồi mới xóa Door."
    )]
    [Min(0f)]
    [SerializeField]
    private float destroyDelay = 0.3f;

    private bool playerInside;
    private bool opened;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }

    private void Start()
    {
        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside ||
            opened)
        {
            return;
        }

        if (Input.GetKeyDown(
                interactKey))
        {
            TryOpen();
        }
    }

    private void TryOpen()
    {
        if (DungeonKeyManager.Instance == null)
        {
            Debug.LogError(
                $"{name}: không tìm thấy " +
                "DungeonKeyManager."
            );

            return;
        }

        /*
         * Không có key:
         * cửa không mở.
         */
        if (!DungeonKeyManager.Instance
                .UseKey())
        {
            PlayLockedSound();

            Debug.Log(
                $"{name}: cần Dungeon Key."
            );

            return;
        }

        StartCoroutine(
            OpenDoorRoutine()
        );
    }

    private IEnumerator OpenDoorRoutine()
    {
        if (opened)
            yield break;

        opened = true;
        playerInside = false;

        /*
         * Tắt icon ngay khi bắt đầu mở.
         */
        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }

        /*
         * Tắt collider để Player có thể đi qua.
         */
        Collider2D[] colliders =
            GetComponentsInChildren<
                Collider2D
            >();

        foreach (Collider2D col
                 in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        if (audioSource != null &&
            openSound != null)
        {
            audioSource.PlayOneShot(
                openSound
            );
        }

        yield return
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0f,
                    destroyDelay
                )
            );

        Destroy(gameObject);
    }

    private void PlayLockedSound()
    {
        if (audioSource == null ||
            lockedSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            lockedSound
        );
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (opened ||
            !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null)
        {
            keyIcon.SetActive(true);
        }
    }

    /*
     * Phòng trường hợp Player đang đứng
     * trong Trigger khi Door vừa được bật.
     */
    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (opened ||
            !other.CompareTag("Player"))
        {
            return;
        }

        playerInside = true;

        if (keyIcon != null &&
            !keyIcon.activeSelf)
        {
            keyIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }
}