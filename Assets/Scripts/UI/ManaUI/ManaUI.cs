using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaUI : MonoBehaviour
{
    public static ManaUI Instance;

    [Header("Reference")]
    public PlayerMana mana;

    public Image fill;
    public Image manaIcon;
    public RectTransform manaBar;
    public TextMeshProUGUI warningText;

    [Header("Animation")]
    public float speed = 6f;

    float targetFill;

    Vector2 startPos;

    Color normalColor;
    Color errorColor = new Color(1f, 0.35f, 0.35f);

    Color fillNormal;
    Color fillFlash = new Color(0.6f, 1f, 1f);

    Coroutine shakeCoroutine;
    Coroutine warningCoroutine;
    Coroutine flashFillCoroutine;
    Coroutine flashIconCoroutine;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        PlayerMana.OnManaChanged += UpdateTarget;
    }

    void OnDisable()
    {
        PlayerMana.OnManaChanged -= UpdateTarget;
    }

    void Start()
    {
        normalColor = manaIcon.color;
        fillNormal = new Color(
    fill.color.r,
    fill.color.g,
    fill.color.b,
    fill.color.a
);

        startPos = manaBar.anchoredPosition;

        warningText.gameObject.SetActive(false);

        UpdateTarget();

        fill.fillAmount = targetFill;
    }

    void Update()
    {
        fill.fillAmount =
            Mathf.Lerp(
                fill.fillAmount,
                targetFill,
                speed * Time.deltaTime);
    }

    void UpdateTarget()
    {
        targetFill = Mathf.Clamp01(
            (float)mana.currentMana / mana.maxMana
        );

        // Chỉ flash khi mana tăng
        if (targetFill > fill.fillAmount)
        {
            if (flashFillCoroutine != null)
                StopCoroutine(flashFillCoroutine);

            flashFillCoroutine = StartCoroutine(FlashFill());
        }
    }




    public void ShowNoMana()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.errorSound);
        }

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        if (flashIconCoroutine != null)
            StopCoroutine(flashIconCoroutine);

        shakeCoroutine =
            StartCoroutine(Shake());

        warningCoroutine =
            StartCoroutine(Warning());

        flashIconCoroutine =
            StartCoroutine(FlashIcon());
    }

    IEnumerator Shake()
    {
        float timer = 0;

        while (timer < 0.2f)
        {
            manaBar.anchoredPosition =
                startPos +
                Random.insideUnitCircle * 4f;

            timer += Time.deltaTime;

            yield return null;
        }

        manaBar.anchoredPosition = startPos;

        shakeCoroutine = null;
    }

    IEnumerator FlashIcon()
    {
        manaIcon.color = errorColor;

        yield return new WaitForSeconds(0.15f);

        manaIcon.color = normalColor;

        flashIconCoroutine = null;
    }

    IEnumerator FlashFill()
    {
        fill.color = fillFlash;

        yield return new WaitForSeconds(0.15f);

        fill.color = fillNormal;

        flashFillCoroutine = null;
    }

    IEnumerator Warning()
    {
        warningText.gameObject.SetActive(true);

        warningText.text = "Not Enough Mana";

        warningText.alpha = 1;

        warningText.rectTransform.localScale = Vector3.zero;

        while (warningText.rectTransform.localScale.x < 0.98f)
        {
            warningText.rectTransform.localScale =
                Vector3.Lerp(
                    warningText.rectTransform.localScale,
                    Vector3.one,
                    18f * Time.deltaTime);

            yield return null;
        }

        warningText.rectTransform.localScale = Vector3.one;

        yield return new WaitForSeconds(0.8f);

        while (warningText.alpha > 0)
        {
            warningText.alpha -= Time.deltaTime * 2.5f;

            yield return null;
        }

        warningText.gameObject.SetActive(false);

        warningCoroutine = null;
    }
}