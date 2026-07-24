using UnityEngine;
using System;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 100;
    public float currentStamina;

    public float drainSpeed = 20f;
    public float recoverSpeed = 20f;

    public bool IsExhausted { get; private set; }

    public float exhaustRecover = 80f;
    public static event Action OnStaminaChanged;

    void Start()
    {
        currentStamina = maxStamina;
    }

    public bool CanRun()
    {
        return currentStamina > 0;
    }

    public void Drain()
    {
        currentStamina -= drainSpeed * Time.deltaTime;

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            IsExhausted = true;
        }

        OnStaminaChanged?.Invoke();
    }

    public bool UseStamina(float amount)
{
    if (currentStamina < amount)
        return false;

    currentStamina -= amount;
    return true;
}

    public void Recover()
    {
        currentStamina += recoverSpeed * Time.deltaTime;

        if (currentStamina > maxStamina)
            currentStamina = maxStamina;

        if (currentStamina >= exhaustRecover)
            IsExhausted = false;

        OnStaminaChanged?.Invoke();
    }
}
