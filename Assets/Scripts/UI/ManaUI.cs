using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaUI : MonoBehaviour
{
    public static ManaUI Instance;

    public PlayerMana mana;

    public Image fill;

    public RectTransform manaBar;

    public TextMeshProUGUI warningText;
    private bool showing;
    float targetFill;

    public float speed = 6f;

    public Image manaIcon;

    Color normalColor = Color.white;
    Color errorColor = new Color(1f, 0.35f, 0.35f);

    Color fillNormal;
    Color fillFlash = new Color(0.6f, 1f, 1f);

    Vector2 startPos;

    Coroutine shakeCoroutine;
    Coroutine warningCoroutine;
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
        fillNormal = fill.color;
        startPos = manaBar.anchoredPosition;

        warningText.alpha = 0;

        UpdateTarget();

        fill.fillAmount = targetFill;
    }

    void Update()
    {
        fill.fillAmount =
            Mathf.Lerp(fill.fillAmount,
                       targetFill,
                       speed * Time.deltaTime);
    }

    void UpdateTarget()
    {
        targetFill =
            mana.currentMana / mana.maxMana;

        StartCoroutine(FlashFill());
    }

    public void ShowNoMana()
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.errorSound);

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        if (warningCoroutine != null)
            StopCoroutine(warningCoroutine);

        showing = false;

        shakeCoroutine = StartCoroutine(Shake());
        warningCoroutine = StartCoroutine(Warning());

        StartCoroutine(FlashIcon());
    }

    IEnumerator FlashIcon()
    {
        manaIcon.color = errorColor;

        yield return new WaitForSeconds(0.15f);

        manaIcon.color = normalColor;
    }
    IEnumerator Shake()
    {
        float timer = 0;

        while (timer < 0.2f)
        {
            manaBar.anchoredPosition =
                startPos + Random.insideUnitCircle * 4f;

            timer += Time.deltaTime;

            yield return null;
        }

        manaBar.anchoredPosition = startPos;
    }

    IEnumerator FlashFill()
    {
        fill.color = fillFlash;

        yield return new WaitForSeconds(0.1f);

        fill.color = fillNormal;
    }
    IEnumerator Warning()
    {
        
        warningText.gameObject.SetActive(true);

        warningText.text = "Not Enough Mana";

        warningText.alpha = 1;
        showing = true;

        warningText.rectTransform.localScale = Vector3.zero;

        while (warningText.rectTransform.localScale.x < 1f)
        {
            warningText.rectTransform.localScale =
                Vector3.Lerp(
                    warningText.rectTransform.localScale,
                    Vector3.one,
                    15 * Time.deltaTime);

            yield return null;
        }

        yield return new WaitForSeconds(0.8f);

        while (warningText.alpha > 0)
        {
            warningText.alpha -= Time.deltaTime * 2.5f;

            yield return null;
        }

        warningText.gameObject.SetActive(false);

        showing = false;

        warningCoroutine = null;
    }
}