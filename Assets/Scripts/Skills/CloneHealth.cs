using UnityEngine;

public class CloneHealth : MonoBehaviour
{
    public int maxHealth = 7;
    public int currentHealth;

    public Canvas hpCanvas;

    float hpTimer;

    void Start()
    {
        currentHealth = maxHealth;

        if (hpCanvas != null)
            hpCanvas.enabled = false;
    }

    void Update()
    {
        if (hpCanvas == null)
            return;

        hpTimer -= Time.deltaTime;

        if (hpTimer <= 0)
            hpCanvas.enabled = false;
    }

    void ShowHP()
    {
        if (hpCanvas == null)
            return;

        hpCanvas.enabled = true;
        hpTimer = 2f;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        ShowHP();

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        GetComponent<CloneFollow>().Die();
    }
}