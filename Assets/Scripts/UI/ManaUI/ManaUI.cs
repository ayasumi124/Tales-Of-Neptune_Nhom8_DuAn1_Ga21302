using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ManaUI : MonoBehaviour
{
    public static ManaUI Instance { get; private set; }

    [Header("Reference")]
    [SerializeField] private PlayerMana mana;

    [SerializeField] private Image fill;
    [SerializeField] private Image manaIcon;
    [SerializeField] private RectTransform manaBar;
    [SerializeField] private TextMeshProUGUI warningText;

    [Header("Animation")]
    [SerializeField] private float speed = 6f;

    private float targetFill;

    private Vector2 startPos;

    private Color normalColor;
    private readonly Color errorColor =
        new Color(1f, 0.35f, 0.35f);

    private Color fillNormal;
    private readonly Color fillFlash =
        new Color(0.6f, 1f, 1f);

    private Coroutine shakeCoroutine;
    private Coroutine warningCoroutine;
    private Coroutine flashFillCoroutine;
    private Coroutine flashIconCoroutine;

    private bool initialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Phát hiện ManaUI bị trùng, xóa object: " +
                gameObject.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeUI();
    }

    private void OnEnable()
    {
        PlayerMana.OnManaChanged -= UpdateTarget;
        PlayerMana.OnManaChanged += UpdateTarget;
    }

    private void OnDisable()
    {
        PlayerMana.OnManaChanged -= UpdateTarget;
    }

    private void InitializeUI()
    {
        if (initialized)
            return;

        initialized = true;

        if (manaIcon != null)
            normalColor = manaIcon.color;

        if (fill != null)
            fillNormal = fill.color;

        if (manaBar != null)
            startPos = manaBar.anchoredPosition;

        if (warningText != null)
        {
            warningText.alpha = 0f;
            warningText.gameObject.SetActive(false);
        }

        FindMana();

        UpdateTarget();

        if (fill != null)
            fill.fillAmount = targetFill;
    }

    private void FindMana()
    {
        if (mana != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            mana = GameManager.Instance.Player
                .GetComponent<PlayerMana>();
        }

        if (mana == null)
        {
            mana = FindFirstObjectByType<PlayerMana>();
        }

        if (mana == null)
        {
            Debug.LogError(
                "ManaUI không tìm thấy PlayerMana."
            );
        }
    }

    private void Update()
    {
        if (fill == null)
            return;

        fill.fillAmount = Mathf.Lerp(
            fill.fillAmount,
            targetFill,
            speed * Time.unscaledDeltaTime
        );
    }

    private void UpdateTarget()
    {
        if (mana == null)
            FindMana();

        if (mana == null || fill == null)
            return;

        float oldFill = targetFill;

        targetFill = Mathf.Clamp01(
            (float)mana.currentMana / mana.maxMana
        );

        if (targetFill > oldFill)
        {
            if (flashFillCoroutine != null)
                StopCoroutine(flashFillCoroutine);

            flashFillCoroutine =
                StartCoroutine(FlashFill());
        }
    }

    public void ShowNoMana()
    {
        Debug.Log(
            "ManaUI ShowNoMana được gọi. Instance: " +
            Instance
        );

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError(
                "ManaUI đang bị inactive trong Hierarchy."
            );
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.errorSound
            );
        }

        StopEffectCoroutines();

        if (manaBar != null)
            manaBar.anchoredPosition = startPos;

        shakeCoroutine = StartCoroutine(Shake());
        warningCoroutine = StartCoroutine(Warning());
        flashIconCoroutine = StartCoroutine(FlashIcon());
    }

    private void StopEffectCoroutines()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        if (flashIconCoroutine != null)
        {
            StopCoroutine(flashIconCoroutine);
            flashIconCoroutine = null;
        }

        if (manaBar != null)
            manaBar.anchoredPosition = startPos;

        if (manaIcon != null)
            manaIcon.color = normalColor;

        if (warningText != null)
        {
            warningText.alpha = 0f;
            warningText.gameObject.SetActive(false);
        }
    }

    private IEnumerator Shake()
    {
        if (manaBar == null)
            yield break;

        float timer = 0f;

        while (timer < 0.2f)
        {
            manaBar.anchoredPosition =
                startPos +
                Random.insideUnitCircle * 4f;

            timer += Time.unscaledDeltaTime;

            yield return null;
        }

        manaBar.anchoredPosition = startPos;

        shakeCoroutine = null;
    }

    private IEnumerator FlashIcon()
    {
        if (manaIcon == null)
            yield break;

        manaIcon.color = errorColor;

        yield return new WaitForSecondsRealtime(0.15f);

        manaIcon.color = normalColor;

        flashIconCoroutine = null;
    }

    private IEnumerator FlashFill()
    {
        if (fill == null)
            yield break;

        fill.color = fillFlash;

        yield return new WaitForSecondsRealtime(0.15f);

        fill.color = fillNormal;

        flashFillCoroutine = null;
    }

    private IEnumerator Warning()
    {
        if (warningText == null)
            yield break;

        warningText.gameObject.SetActive(true);

        warningText.text = "Not Enough Mana";
        warningText.alpha = 1f;
        warningText.rectTransform.localScale =
            Vector3.zero;

        while (
            warningText.rectTransform.localScale.x <
            0.98f)
        {
            warningText.rectTransform.localScale =
                Vector3.Lerp(
                    warningText.rectTransform.localScale,
                    Vector3.one,
                    18f * Time.unscaledDeltaTime
                );

            yield return null;
        }

        warningText.rectTransform.localScale =
            Vector3.one;

        yield return new WaitForSecondsRealtime(0.8f);

        while (warningText.alpha > 0f)
        {
            warningText.alpha -=
                Time.unscaledDeltaTime * 2.5f;

            yield return null;
        }

        warningText.alpha = 0f;
        warningText.gameObject.SetActive(false);

        warningCoroutine = null;
    }

    private void OnDestroy()
    {
        PlayerMana.OnManaChanged -= UpdateTarget;

        if (Instance == this)
            Instance = null;
    }
}