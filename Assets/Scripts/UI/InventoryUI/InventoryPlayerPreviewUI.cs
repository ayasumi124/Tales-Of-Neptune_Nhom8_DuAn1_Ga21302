using UnityEngine;
using UnityEngine.UI;

public class InventoryPlayerPreviewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image previewImage;

    [Header("Player")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Header("Display")]
    [SerializeField] private bool copyFlipX = true;

    [Tooltip("Phóng to hoặc thu nhỏ sprite trong UI.")]
    [SerializeField] private float previewScale = 1f;

    private void Awake()
    {
        if (previewImage == null)
        {
            previewImage =
                GetComponent<Image>();
        }

        FindPlayerSprite();
        RefreshPreview();
    }

    private void OnEnable()
    {
        FindPlayerSprite();
        RefreshPreview();
    }

    private void LateUpdate()
    {
        /*
         * SpriteRenderer của Player thay sprite theo Animator,
         * nên UI cũng cập nhật theo frame hiện tại.
         */
        RefreshPreview();
    }

    private void FindPlayerSprite()
    {
        if (playerSpriteRenderer != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            playerSpriteRenderer =
                GameManager.Instance.Player
                    .GetComponent<SpriteRenderer>();

            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer =
                    GameManager.Instance.Player
                        .GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (playerSpriteRenderer == null)
        {
            Players player =
                FindFirstObjectByType<Players>();

            if (player != null)
            {
                playerSpriteRenderer =
                    player.GetComponent<SpriteRenderer>();

                if (playerSpriteRenderer == null)
                {
                    playerSpriteRenderer =
                        player.GetComponentInChildren<SpriteRenderer>();
                }
            }
        }

        if (playerSpriteRenderer == null)
        {
            Debug.LogError(
                "InventoryPlayerPreviewUI không tìm thấy " +
                "SpriteRenderer của Player."
            );
        }
    }

    private void RefreshPreview()
    {
        if (previewImage == null)
            return;

        if (playerSpriteRenderer == null)
            FindPlayerSprite();

        if (playerSpriteRenderer == null ||
            playerSpriteRenderer.sprite == null)
        {
            previewImage.sprite = null;
            previewImage.enabled = false;
            return;
        }

        previewImage.enabled = true;
        previewImage.sprite =
            playerSpriteRenderer.sprite;

        previewImage.preserveAspect = true;

        /*
         * Image không có flipX, nên dùng scale X âm
         * để sao chép hướng quay của SpriteRenderer.
         */
        float xScale = previewScale;

        if (copyFlipX &&
            playerSpriteRenderer.flipX)
        {
            xScale *= -1f;
        }

        previewImage.rectTransform.localScale =
            new Vector3(
                xScale,
                previewScale,
                1f
            );
    }

    public void SetPlayerSpriteRenderer(
        SpriteRenderer newRenderer)
    {
        playerSpriteRenderer =
            newRenderer;

        RefreshPreview();
    }
}