using UnityEngine;
using System.Collections;

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

    [SerializeField] GameObject coinPrefab;

    [SerializeField]
    [Range(0, 1)]
    float dropRate = 0.8f;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    public float knockbackTime = 0.1f;

    private Rigidbody2D rb;



    private Animator animator;

    private bool isDead;
    private EnermyAudio enermyAudio;
    private EnermyHealthBar enermyHealthBar;
    [Header("Mana Reward")]
    public float manaReward = 10f;


    void Start()
    {

        currentHealth = maxHealth;
        if (hpCanvas != null)
            hpCanvas.enabled = false;
        enermyAudio = GetComponent<EnermyAudio>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (hpCanvas == null) return;

        hpTimer -= Time.deltaTime;

        if (hpTimer <= 0)
            hpCanvas.enabled = false;
    }


    IEnumerator ShowDamagePopup(int damage)
    {
        yield return null;

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(
                damage,
                transform.position + Vector3.up * 0.8f);
        }
    }
    void ShowHP()
    {
        if (hpCanvas == null) return;

        hpCanvas.enabled = true;
        hpTimer = 2f;
    }

    public void TakeDamage(int damage, Vector2 knockbackDir)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " HP: " + currentHealth);

        StartCoroutine(ShowDamagePopup(damage));
        Debug.Log(gameObject.name + " HP: " + currentHealth);

        if (enermyAudio != null)
            enermyAudio.PlayHurt();

        ShowHP();


        animator.SetTrigger("Hurt");

        // Knockback
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            EnermyMovement movement = GetComponent<EnermyMovement>();

            if (movement != null)
            {
                movement.externalVelocity =
                    knockbackDir.normalized * knockbackForce;
            }
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;

        animator.SetTrigger("Death");

        Debug.Log(gameObject.name + " Dead");



        EnermyMovement movement = GetComponent<EnermyMovement>();
        if (movement != null)
            movement.enabled = false;

        EnermyAttack attack = GetComponent<EnermyAttack>();
        if (attack != null)
            attack.enabled = false;

        foreach (Collider2D col in GetComponents<Collider2D>())
        {
            col.enabled = false;
        }

        if (enermyAudio != null)
            enermyAudio.PlayDeath();

        
    }

    // Animation Event
    public void OnDeathFinished()
    {

        PlayerMana mana = FindFirstObjectByType<PlayerMana>();

if (mana != null)
{
    mana.RestoreMana(5);
}
        if (Random.value <= dropRate)
        {
            Instantiate(
                coinPrefab,
                transform.position,
                Quaternion.identity);
        }

        Destroy(gameObject);
    }
}