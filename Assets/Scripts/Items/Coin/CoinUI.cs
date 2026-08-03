using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    public static CoinUI Instance
    {
        get;
        private set;
    }

    [Header("UI")]
    [SerializeField] private GameObject coinPanel;
    [SerializeField] private Image coinIcon;
    [SerializeField] private TextMeshProUGUI coinText;

    [Header("Hide")]
    [SerializeField] private float hideDelay = 5f;

    [Header("Data")]
    [SerializeField] private int coin;

    private float hideTimer;
    private bool panelVisible;

    public int Coin => coin;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Debug.LogWarning(
                $"CoinUI trùng, xóa bản mới ở scene: " +
                $"{gameObject.scene.name}"
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;

        /*
         * Chỉ dùng dòng này nếu CoinUI là root object.
         * Nếu CoinUI nằm dưới Canvas persistent thì
         * Canvas cha đã DontDestroyOnLoad là đủ.
         */
        if (transform.parent == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        /*
         * Không đặt coin = 0 ở đây.
         * Nếu đặt lại, mỗi lần scene tạo UI mới
         * coin sẽ bị reset.
         */
        coin = Mathf.Max(0, coin);

        UpdateUI();

        if (coinPanel != null)
        {
            coinPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!panelVisible)
            return;

        hideTimer -= Time.unscaledDeltaTime;

        if (hideTimer <= 0f)
        {
            HidePanel();
        }
    }

    public void AddCoin(int amount)
    {
        if (amount <= 0)
            return;

        coin += amount;

        UpdateUI();
        ShowPanel();

        Debug.Log(
            $"Đã cộng {amount} coin. Tổng coin: {coin}"
        );
    }

    public bool SpendCoin(int amount)
    {
        if (amount <= 0)
            return false;

        if (coin < amount)
        {
            Debug.Log(
                $"Không đủ coin. Cần {amount}, hiện có {coin}."
            );

            return false;
        }

        coin -= amount;

        UpdateUI();
        ShowPanel();

        return true;
    }

    public bool HasEnoughCoin(int amount)
    {
        return amount >= 0 &&
               coin >= amount;
    }

    public void SetCoin(int amount)
    {
        coin = Mathf.Max(0, amount);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }

    private void ShowPanel()
    {
        panelVisible = true;
        hideTimer = Mathf.Max(0f, hideDelay);

        if (coinPanel != null)
        {
            coinPanel.SetActive(true);
        }
    }

    private void HidePanel()
    {
        panelVisible = false;

        if (coinPanel != null)
        {
            coinPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        /*
         * Bản duplicate bị Destroy không được phép
         * xóa Instance của bản persistent.
         */
        if (Instance == this)
        {
            Instance = null;
        }
    }
}