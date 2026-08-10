using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DungeonKeyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private Image keyIcon;

    [SerializeField]
    private TMP_Text keyText;

    [Header("Display")]
    [SerializeField]
    private bool hideWhenZero = false;

    private void OnEnable()
    {
        DungeonKeyManager.OnKeyCountChanged +=
            UpdateUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        DungeonKeyManager.OnKeyCountChanged -=
            UpdateUI;
    }

    private void Start()
    {
        RefreshUI();
    }

    private void RefreshUI()
    {
        int amount = 0;

        if (DungeonKeyManager.Instance != null)
        {
            amount =
                DungeonKeyManager.Instance
                    .KeyCount;
        }

        UpdateUI(amount);
    }

    private void UpdateUI(
        int amount)
    {
        if (keyText != null)
        {
            keyText.text =
                $"x{amount}";
        }

        if (hideWhenZero)
        {
            if (keyIcon != null)
            {
                keyIcon.enabled =
                    amount > 0;
            }

            if (keyText != null)
            {
                keyText.enabled =
                    amount > 0;
            }
        }
        else
        {
            if (keyIcon != null)
                keyIcon.enabled = true;

            if (keyText != null)
                keyText.enabled = true;
        }
    }
}