using UnityEngine;

public class CloneHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public Canvas hpCanvas;
    private Animator animator;

    float hpTimer;

    void Start()
    {
        animator = GetComponent<Animator>();
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
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }

    }

    public void Die()
    {
        animator.SetTrigger("Death");
        DestroyClone();
    }

    public void DestroyClone()
    {
        Destroy(gameObject); // thời gian bằng animation chết
    }
}