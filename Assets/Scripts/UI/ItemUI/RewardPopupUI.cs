using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardPopupUI : MonoBehaviour
{
    public static RewardPopupUI Instance
    {
        get;
        private set;
    }

    private enum RewardPopupType
    {
        Coin,
        Item,
        DungeonKey
    }

    [Header("Panel")]
    [SerializeField]
    private GameObject rewardPanel;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Content")]
    [SerializeField]
    private Image rewardIcon;

    [SerializeField]
    private TextMeshProUGUI rewardNameText;

    [SerializeField]
    private TextMeshProUGUI rewardQuantityText;

    [Header("Animation")]
    [SerializeField]
    private float displayDuration = 1.5f;

    [SerializeField]
    private float fadeDuration = 0.25f;

    [Header("Audio")]
    [Tooltip("Âm thanh phát khi popup vừa hiện.")]
    [SerializeField]
    private AudioClip rewardAppearSound;

    [Range(0f, 3f)]
    [SerializeField]
    private float rewardAppearVolume = 1f;

    [Tooltip("Âm thanh phát khi popup Coin đóng.")]
    [SerializeField]
    private AudioClip coinRewardCloseSound;

    [Range(0f, 3f)]
    [SerializeField]
    private float coinRewardCloseVolume = 1f;

    [Tooltip(
        "Âm thanh phát khi popup Dungeon Key đóng."
    )]
    [SerializeField]
    private AudioClip dungeonKeyCloseSound;

    [Range(0f, 3f)]
    [SerializeField]
    private float dungeonKeyCloseVolume = 1f;

    private Coroutine popupCoroutine;

    private RewardPopupType currentRewardType;

    private ItemData currentItemData;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }
    }

    public void ShowItem(
        ItemData itemData,
        int quantity)
    {
        if (itemData == null)
            return;

        currentRewardType =
            RewardPopupType.Item;

        currentItemData =
            itemData;

        ShowReward(
            itemData.Icon,
            itemData.ItemName,
            quantity
        );
    }

    public void ShowCoin(
        Sprite coinIcon,
        int amount)
    {
        currentRewardType =
            RewardPopupType.Coin;

        currentItemData =
            null;

        ShowReward(
            coinIcon,
            "Coin",
            amount
        );
    }

    public void ShowDungeonKey(
        Sprite keyIcon,
        int amount = 1)
    {
        currentRewardType =
            RewardPopupType.DungeonKey;

        currentItemData =
            null;

        ShowReward(
            keyIcon,
            "Dungeon Key",
            amount
        );
    }

    private void ShowReward(
        Sprite icon,
        string rewardName,
        int quantity)
    {
        if (popupCoroutine != null)
        {
            StopCoroutine(
                popupCoroutine
            );

            popupCoroutine = null;
        }

        if (rewardIcon != null)
        {
            rewardIcon.sprite =
                icon;

            rewardIcon.enabled =
                icon != null;

            rewardIcon.preserveAspect =
                true;
        }

        if (rewardNameText != null)
        {
            rewardNameText.text =
                rewardName;
        }

        if (rewardQuantityText != null)
        {
            rewardQuantityText.text =
                $"x{Mathf.Max(1, quantity)}";
        }

        popupCoroutine =
            StartCoroutine(
                PopupRoutine()
            );
    }

    private IEnumerator PopupRoutine()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(
                true
            );
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                1f;
        }

        PlayAppearSound();

        yield return
            new WaitForSecondsRealtime(
                Mathf.Max(
                    0f,
                    displayDuration
                )
            );

        if (canvasGroup != null)
        {
            float timer = 0f;

            float duration =
                Mathf.Max(
                    0.01f,
                    fadeDuration
                );

            while (timer < duration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer / duration
                    );

                canvasGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0f,
                        progress
                    );

                yield return null;
            }

            canvasGroup.alpha =
                0f;
        }

        PlayCloseRewardSound();

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(
                false
            );
        }

        currentItemData =
            null;

        popupCoroutine =
            null;
    }

    private void PlayAppearSound()
    {
        if (AudioManager.Instance == null ||
            rewardAppearSound == null)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(
            rewardAppearSound,
            rewardAppearVolume
        );
    }

    private void PlayCloseRewardSound()
    {
        if (AudioManager.Instance == null)
            return;

        switch (currentRewardType)
        {
            case RewardPopupType.Coin:

                if (coinRewardCloseSound != null)
                {
                    AudioManager.Instance.PlaySFX(
                        coinRewardCloseSound,
                        coinRewardCloseVolume
                    );
                }

                break;

            case RewardPopupType.Item:

                if (currentItemData != null &&
                    currentItemData.pickupSound != null)
                {
                    AudioManager.Instance.PlayItemSFX(
                        currentItemData.pickupSound,
                        currentItemData.pickupVolume
                    );
                }

                break;

            case RewardPopupType.DungeonKey:

                if (dungeonKeyCloseSound != null)
                {
                    AudioManager.Instance.PlaySFX(
                        dungeonKeyCloseSound,
                        dungeonKeyCloseVolume
                    );
                }

                break;
        }
    }

    private void OnDisable()
    {
        if (popupCoroutine != null)
        {
            StopCoroutine(
                popupCoroutine
            );

            popupCoroutine =
                null;
        }

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(
                false
            );
        }

        currentItemData =
            null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}