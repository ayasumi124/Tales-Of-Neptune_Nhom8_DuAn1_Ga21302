using UnityEngine;

public class CloneHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    public Canvas hpCanvas;
    private Animator animator;
    private Rigidbody2D rb;
    private CloneAudio cloneAudio;
    float hpTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cloneAudio = GetComponent<CloneAudio>();
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
        if (cloneAudio != null)
            cloneAudio.PlayHurt();
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
        rb.linearVelocity = Vector2.zero;

        if (cloneAudio != null)
            cloneAudio.PlayDeath();

        Destroy(gameObject, 0.5f);
    }

    public void DestroyClone()
    {
        Destroy(gameObject); // thời gian bằng animation chết
    }
}