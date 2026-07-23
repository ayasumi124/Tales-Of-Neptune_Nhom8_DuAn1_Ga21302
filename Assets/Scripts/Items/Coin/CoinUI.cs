using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CoinUI : MonoBehaviour
{
    public static CoinUI Instance;

    public Image coinIcon;

    [Header("UI")]
    public GameObject coinPanel;
    public TextMeshProUGUI coinText;

    [Header("Hide")]
    public float hideDelay = 5f;

    private int coin;
    private float timer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        coin = 0;

        UpdateUI();

        coinPanel.SetActive(false);
    }

    void Update()
    {
        if (!coinPanel.activeSelf)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            coinPanel.SetActive(false);
        }
    }

    public void AddCoin(int amount)
    {
        coin += amount;

        UpdateUI();

        coinPanel.SetActive(true);

        timer = hideDelay;
    }

    void UpdateUI()
    {
        coinText.text = coin.ToString();
    }

    public int GetCoin()
    {
        return coin;
    }
}