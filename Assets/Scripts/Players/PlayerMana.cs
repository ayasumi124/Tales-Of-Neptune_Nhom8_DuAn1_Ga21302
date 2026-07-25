using UnityEngine;
using System;

public class PlayerMana : MonoBehaviour
{
    public float maxMana = 100;
    public float currentMana;

    public static event Action OnManaChanged;
    public ParticleSystem manaRecoverEffect;

    void Start()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke();
    }

    public bool UseMana(float amount)
{
    Debug.Log("Current Mana: " + currentMana);
    Debug.Log("Need Mana: " + amount);

    if (currentMana < amount)
    {
        Debug.Log("Không đủ mana");
        return false;
    }

    currentMana -= amount;

    Debug.Log("Mana còn lại: " + currentMana);

    OnManaChanged?.Invoke();

    return true;
}

    public void RestoreMana(float amount)
    {
        currentMana += amount;

        if (currentMana > maxMana)
            currentMana = maxMana;

        OnManaChanged?.Invoke();
    }
}