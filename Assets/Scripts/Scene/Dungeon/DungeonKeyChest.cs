using UnityEngine;

public class DungeonKeyChest : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip openSound;

    [Header("Interaction")]
    [SerializeField]
    private KeyCode interactKey = KeyCode.E;

    [Header("Animator")]
    [SerializeField]
    private string openTrigger = "Open";

    private bool playerInside;
    private bool opened;

    private void Awake()
    {
        if (animator == null)
        {
            animator =
                GetComponent<Animator>();
        }

        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        /*
         * Khi chest vừa được hiện sau khi clear room,
         * reset trạng thái tương tác.
         */
        playerInside = false;
    }

    private void Update()
    {
        if (opened)
            return;

        if (!playerInside)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (opened)
            return;

        if (DungeonKeyManager.Instance == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy DungeonKeyManager."
            );

            return;
        }

        opened = true;

        Debug.Log(
            $"{name}: mở Key Chest."
        );

        if (animator != null &&
            !string.IsNullOrWhiteSpace(
                openTrigger))
        {
            animator.ResetTrigger(
                openTrigger
            );

            animator.SetTrigger(
                openTrigger
            );
        }

        if (audioSource != null &&
            openSound != null)
        {
            audioSource.PlayOneShot(
                openSound
            );
        }

        DungeonKeyManager.Instance
            .GiveKey();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        CheckPlayerEnter(other);
    }

    /*
     * Quan trọng:
     * chest có thể vừa SetActive(true)
     * khi Player đã đứng trong vùng trigger.
     */
    private void OnTriggerStay2D(
        Collider2D other)
    {
        CheckPlayerEnter(other);
    }

    private void CheckPlayerEnter(
        Collider2D other)
    {
        if (other == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }

    private void OnTriggerExit2D(
        Collider2D other)
    {
        if (other == null)
            return;

        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }
}