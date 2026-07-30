using UnityEngine;
using System.Collections;

public class FadeUI : MonoBehaviour
{
    public static FadeUI Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.4f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("FadeCanvas chưa có CanvasGroup.");
            return;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public IEnumerator FadeOut()
    {
        Debug.Log("FadeOut bắt đầu");

        canvasGroup.blocksRaycasts = true;

        yield return FadeTo(1f);

        Debug.Log("FadeOut xong, alpha = " + canvasGroup.alpha);
    }

    public IEnumerator FadeIn()
    {
        Debug.Log("FadeIn bắt đầu");

        yield return FadeTo(0f);

        canvasGroup.blocksRaycasts = false;

        Debug.Log("FadeIn xong, alpha = " + canvasGroup.alpha);
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void ForceTransparent()
    {
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }
}