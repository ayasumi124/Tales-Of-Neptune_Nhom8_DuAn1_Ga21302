using System.Collections;
using TMPro;
using UnityEngine;

public class ShopNotificationUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TextMeshProUGUI messageText;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [Header("Time")]
    [SerializeField]
    private float showDuration = 0.8f;

    [SerializeField]
    private float fadeDuration = 0.35f;

    [Header("Colors")]
    [SerializeField]
    private Color boughtColor =
        new Color(0.3f, 1f, 0.3f);

    [SerializeField]
    private Color warningColor =
        new Color(1f, 0.35f, 0.35f);

    private Coroutine messageRoutine;

    private void Awake()
    {
        if (messageText == null)
        {
            messageText =
                GetComponent<TextMeshProUGUI>();
        }

        if (canvasGroup == null)
        {
            canvasGroup =
                GetComponent<CanvasGroup>();
        }

        HideImmediate();
    }

    public void ShowBought(
        string itemName)
    {
        ShowMessage(
            $"Bought {itemName}!",
            boughtColor
        );
    }

    public void ShowNotEnoughCoin()
    {
        ShowMessage(
            "Not enough Coin!",
            warningColor
        );
    }

    public void ShowInventoryFull()
    {
        ShowMessage(
            "Inventory Full!",
            warningColor
        );
    }

    private void ShowMessage(
        string message,
        Color color)
    {
        if (messageText == null ||
            canvasGroup == null)
        {
            return;
        }

        if (messageRoutine != null)
        {
            StopCoroutine(
                messageRoutine
            );
        }

        messageRoutine =
            StartCoroutine(
                MessageRoutine(
                    message,
                    color
                )
            );
    }

    private IEnumerator MessageRoutine(
        string message,
        Color color)
    {
        messageText.text = message;
        messageText.color = color;

        canvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(
            showDuration
        );

        float time = 0f;

        while (time < fadeDuration)
        {
            time +=
                Time.unscaledDeltaTime;

            canvasGroup.alpha =
                Mathf.Lerp(
                    1f,
                    0f,
                    time / fadeDuration
                );

            yield return null;
        }

        canvasGroup.alpha = 0f;

        messageRoutine = null;
    }

    public void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
}