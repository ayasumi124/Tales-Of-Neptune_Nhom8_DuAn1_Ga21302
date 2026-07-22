using UnityEngine;

public class EnermyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("Death")]
    public float deathDelay = 1f;

    [Header("HP UI")]
    public Canvas hpCanvas;

    private float hpTimer;




    private Animator animator;

    private bool isDead;
    private EnermyAudio enermyAudio;
    private EnermyHealthBar enermyHealthBar;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        if (hpCanvas != null)
            hpCanvas.enabled = false;
        enermyAudio = GetComponent<EnermyAudio>();
    }

    void Update()
    {
        if (hpCanvas == null) return;

        hpTimer -= Time.deltaTime;

        if (hpTimer <= 0)
            hpCanvas.enabled = false;
    }


    void ShowHP()
    {
        if (hpCanvas == null) return;

        hpCanvas.enabled = true;
        hpTimer = 2f;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;


        Debug.Log(gameObject.name + " HP: " + currentHealth);

        if (enermyAudio != null)
            enermyAudio.PlayHurt();

        ShowHP();

        // Sau này nếu có animation Hurt
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        Debug.Log(gameObject.name + " Dead");

        // Tắt AI
        EnermyMovement movement = GetComponent<EnermyMovement>();

        if (movement != null)
            movement.enabled = false;

        // Tắt đánh
        EnermyAttack attack = GetComponent<EnermyAttack>();

        if (attack != null)
            attack.enabled = false;

        // Tắt Collider
        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
            col.enabled = false;

        // Nếu sau này có animation chết thì thay Destroy bằng Trigger
        if (enermyAudio != null)
            enermyAudio.PlayDeath();

        Destroy(gameObject, deathDelay);
    }
}