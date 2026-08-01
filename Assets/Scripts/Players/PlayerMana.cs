using System;
using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana")]
    public float maxMana = 100f;
    public float currentMana;

    public static event Action OnManaChanged;

    [Header("Effects")]
    public ParticleSystem manaRecoverEffect;

    private void Start()
    {
        currentMana =
            Mathf.Clamp(
                currentMana,
                0f,
                maxMana
            );

        /*
         * Nếu muốn mỗi lần chạy game luôn đầy mana,
         * giữ dòng này.
         */
        currentMana = maxMana;

        NotifyManaChanged();
    }

    public bool UseMana(float amount)
    {
        if (amount <= 0f)
            return true;

        if (currentMana < amount)
        {
            Debug.Log("Không đủ mana.");
            return false;
        }

        currentMana =
            Mathf.Clamp(
                currentMana - amount,
                0f,
                maxMana
            );

        Debug.Log(
            $"Mana còn lại: " +
            $"{currentMana}/{maxMana}"
        );

        NotifyManaChanged();

        return true;
    }

    public bool RestoreMana(float amount)
    {
        if (amount <= 0f)
            return false;

        if (currentMana >= maxMana)
        {
            Debug.Log("Mana đã đầy.");
            return false;
        }

        float oldMana =
            currentMana;

        currentMana =
            Mathf.Clamp(
                currentMana + amount,
                0f,
                maxMana
            );

        float restoredAmount =
            currentMana - oldMana;

        if (restoredAmount <= 0f)
            return false;

        Debug.Log(
            $"Hồi {restoredAmount} Mana. " +
            $"Mana: {currentMana}/{maxMana}"
        );

        if (manaRecoverEffect != null)
        {
            manaRecoverEffect.Play();
        }

        NotifyManaChanged();

        return true;
    }

    public bool IncreaseMaxMana(
        float amount,
        bool restoreToFull = true)
    {
        if (amount <= 0f)
            return false;

        float oldMaxMana =
            maxMana;

        maxMana += amount;

        if (maxMana < 1f)
            maxMana = 1f;

        if (restoreToFull)
        {
            currentMana =
                maxMana;
        }
        else
        {
            currentMana +=
                maxMana - oldMaxMana;

            currentMana =
                Mathf.Clamp(
                    currentMana,
                    0f,
                    maxMana
                );
        }

        Debug.Log(
            $"Max Mana tăng từ " +
            $"{oldMaxMana} lên {maxMana}."
        );

        NotifyManaChanged();

        return true;
    }

    public bool IsManaFull()
    {
        return currentMana >= maxMana;
    }

    public void ResetMana()
    {
        currentMana =
            maxMana;

        NotifyManaChanged();
    }

    private void NotifyManaChanged()
    {
        OnManaChanged?.Invoke();
    }
}