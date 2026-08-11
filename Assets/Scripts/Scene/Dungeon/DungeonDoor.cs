using UnityEngine;

public class DungeonDoor : MonoBehaviour
{
    public enum DoorOrientation
    {
        Horizontal,
        Vertical
    }

    [Header("Door Type")]
    [SerializeField]
    private DoorOrientation orientation =
        DoorOrientation.Horizontal;

    // =====================================================
    // STATES
    // =====================================================

    [Header("Closed State")]
    [Tooltip(
        "Object chứa Sprite + Collider của cửa khi đóng."
    )]
    [SerializeField]
    private GameObject closedState;

    [Header("Horizontal Open States")]
    [Tooltip(
        "Cửa ngang thu về bên trái của màn hình."
    )]
    [SerializeField]
    private GameObject openWorldLeft;

    [Tooltip(
        "Cửa ngang thu về bên phải của màn hình."
    )]
    [SerializeField]
    private GameObject openWorldRight;

    [Header("Vertical Open States")]
    [Tooltip(
        "Cửa dọc thu lên phía trên màn hình."
    )]
    [SerializeField]
    private GameObject openWorldUp;

    [Tooltip(
        "Cửa dọc thu xuống phía dưới màn hình."
    )]
    [SerializeField]
    private GameObject openWorldDown;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI")]
    [SerializeField]
    private GameObject keyIcon;

    // =====================================================
    // INTERACTION
    // =====================================================

    [Header("Interaction")]
    [SerializeField]
    private KeyCode interactKey =
        KeyCode.E;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip openSound;

    [SerializeField]
    private AudioClip lockedSound;

    [Range(0f, 2f)]
    [SerializeField]
    private float openVolume = 1f;

    [Range(0f, 2f)]
    [SerializeField]
    private float lockedVolume = 1f;

    // =====================================================
    // RUNTIME
    // =====================================================

    private bool playerInside;
    private bool opened;

    private Transform currentPlayer;

    public bool IsOpened =>
        opened;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        SetClosedState();
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
        if (opened)
            return;

        if (!playerInside)
            return;

        if (currentPlayer == null)
            return;

        if (Input.GetKeyDown(
                interactKey))
        {
            TryOpen();
        }
    }

    // =====================================================
    // TRY OPEN
    // =====================================================

    private void TryOpen()
    {
        if (opened)
            return;

        if (DungeonKeyManager.Instance == null)
        {
            Debug.LogError(
                $"{name}: không tìm thấy " +
                "DungeonKeyManager."
            );

            return;
        }

        /*
         * Chưa có key.
         */
        if (!DungeonKeyManager.Instance.HasKey)
        {
            PlayLockedSound();

            Debug.Log(
                $"{name}: cần Dungeon Key."
            );

            return;
        }

        /*
         * Chỉ trừ key khi chắc chắn mở cửa.
         */
        if (!DungeonKeyManager.Instance.UseKey())
        {
            PlayLockedSound();
            return;
        }

        OpenDoor();
    }

    // =====================================================
    // OPEN
    // =====================================================

    private void OpenDoor()
    {
        if (opened)
            return;

        opened = true;
        playerInside = false;

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }

        /*
         * Tắt cửa đóng.
         */
        if (closedState != null)
        {
            closedState.SetActive(false);
        }

        /*
         * Chọn state mở dựa theo vị trí Player.
         */
        if (orientation ==
            DoorOrientation.Horizontal)
        {
            OpenHorizontalDoor();
        }
        else
        {
            OpenVerticalDoor();
        }

        PlayOpenSound();

        Debug.Log(
            $"{name}: Door Opened."
        );
    }

    // =====================================================
    // HORIZONTAL
    // =====================================================

    private void OpenHorizontalDoor()
    {
        DisableAllOpenStates();

        if (currentPlayer == null)
        {
            /*
             * Fallback.
             */
            if (openWorldRight != null)
            {
                openWorldRight.SetActive(true);
            }

            return;
        }

        /*
         * Player ở dưới cửa:
         *
         * Player nhìn UP.
         * Bên phải của Player = world RIGHT.
         */
        bool playerBelowDoor =
            currentPlayer.position.y <
            transform.position.y;

        if (playerBelowDoor)
        {
            if (openWorldRight != null)
            {
                openWorldRight.SetActive(true);
            }
        }
        /*
         * Player ở trên cửa:
         *
         * Player nhìn DOWN.
         * Bên phải của Player = world LEFT.
         */
        else
        {
            if (openWorldLeft != null)
            {
                openWorldLeft.SetActive(true);
            }
        }
    }

    // =====================================================
    // VERTICAL
    // =====================================================

   private void OpenVerticalDoor()
{
    DisableAllOpenStates();

    if (currentPlayer == null)
    {
        if (openWorldDown != null)
        {
            openWorldDown.SetActive(true);
        }

        return;
    }

    bool playerLeftOfDoor =
        currentPlayer.position.x <
        transform.position.x;

    /*
     * Player đứng bên trái cửa,
     * nhìn sang phải.
     *
     * Theo prefab hiện tại của bạn:
     * dùng OpenWorldUp.
     */
    if (playerLeftOfDoor)
    {
        if (openWorldUp != null)
        {
            openWorldUp.SetActive(true);
        }
    }
    /*
     * Player đứng bên phải cửa,
     * nhìn sang trái.
     *
     * Theo prefab hiện tại:
     * dùng OpenWorldDown.
     */
    else
    {
        if (openWorldDown != null)
        {
            openWorldDown.SetActive(true);
        }
    }
}

    // =====================================================
    // STATE
    // =====================================================

    private void SetClosedState()
    {
        opened = false;

        if (closedState != null)
        {
            closedState.SetActive(true);
        }

        DisableAllOpenStates();
    }

    private void DisableAllOpenStates()
    {
        if (openWorldLeft != null)
        {
            openWorldLeft.SetActive(false);
        }

        if (openWorldRight != null)
        {
            openWorldRight.SetActive(false);
        }

        if (openWorldUp != null)
        {
            openWorldUp.SetActive(false);
        }

        if (openWorldDown != null)
        {
            openWorldDown.SetActive(false);
        }
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayOpenSound()
    {
        if (audioSource == null ||
            openSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            openSound,
            openVolume
        );
    }

    private void PlayLockedSound()
    {
        if (audioSource == null ||
            lockedSound == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            lockedSound,
            lockedVolume
        );
    }

    // =====================================================
    // PLAYER TRIGGER
    // =====================================================

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        currentPlayer =
            other.transform;

        if (keyIcon != null)
        {
            keyIcon.SetActive(true);
        }
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (opened)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        currentPlayer =
            other.transform;

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

        currentPlayer = null;

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }

    // =====================================================
    // EDITOR RESET
    // =====================================================

    [ContextMenu("Reset Door")]
    private void ResetDoor()
    {
        playerInside = false;
        currentPlayer = null;

        SetClosedState();

        if (keyIcon != null)
        {
            keyIcon.SetActive(false);
        }
    }

    private void OnValidate()
    {
        openVolume =
            Mathf.Max(
                0f,
                openVolume
            );

        lockedVolume =
            Mathf.Max(
                0f,
                lockedVolume
            );
    }
}