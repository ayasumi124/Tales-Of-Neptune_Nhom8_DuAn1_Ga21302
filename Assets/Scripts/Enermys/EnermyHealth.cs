using System.Collections;
using UnityEngine;

public class EnermyHealth : MonoBehaviour
{
    [Header("Health")]
    [Min(1)]
    public int maxHealth = 100;

    public int currentHealth;

    [Header("Death")]
    [Min(0f)]
    public float deathDelay = 1f;

    [Header("HP UI")]
    public Canvas hpCanvas;

    [SerializeField]
    private float hpDisplayTime = 2f;

    [Header("Hurt")]
    [Min(0f)]
    [SerializeField]
    private float hurtLockTime = 0.2f;

    [Header("Coin Reward")]
    [SerializeField]
    private GameObject coinPrefab;

    [Range(0f, 1f)]
    [SerializeField]
    private float dropRate = 0.8f;

    [Min(1)]
    [SerializeField]
    private int minCoinCount = 1;

    [Min(1)]
    [SerializeField]
    private int maxCoinCount = 1;

    [Min(0f)]
    [SerializeField]
    private float coinScatterRadius = 0.25f;

    [Header("Knockback")]
    [Tooltip("Tốc độ đẩy ban đầu.")]
    [Min(0f)]
    [SerializeField]
    private float knockbackForce = 2.5f;
    public float KnockbackForce => knockbackForce;

    [Tooltip("Khoảng thời gian giữ lực đẩy.")]
    [Min(0f)]
    [SerializeField]
    private float knockbackTime = 0.08f;

    private Rigidbody2D rb;
    private Animator animator;
    private EnermyAudio enermyAudio;
    private EnermyMovement movement;
    private EnermyItemDrop itemDrop;

    private EnermyAttack normalAttack;
    private EnermyAttackBase specialAttack;

    private Coroutine hurtCoroutine;
    private Coroutine knockbackCoroutine;
    private Coroutine destroyCoroutine;

    private float hpTimer;

    private bool isDead;
    private bool rewardsGiven;

    public bool IsDead => isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enermyAudio = GetComponent<EnermyAudio>();
        movement = GetComponent<EnermyMovement>();
        itemDrop = GetComponent<EnermyItemDrop>();

        normalAttack = GetComponent<EnermyAttack>();
        specialAttack = GetComponent<EnermyAttackBase>();
    }

    private void Start()
    {
        currentHealth =
            Mathf.Max(
                1,
                maxHealth
            );

        if (hpCanvas != null)
        {
            hpCanvas.enabled = false;
        }
    }

    private void Update()
    {
        if (hpCanvas == null ||
            !hpCanvas.enabled)
        {
            return;
        }

        hpTimer -= Time.deltaTime;

        if (hpTimer <= 0f)
        {
            hpCanvas.enabled = false;
        }
    }

    public void TakeDamage(
     int damage,
     Vector2 knockbackDirection,
     float knockbackStrength)
    {
        if (isDead ||
            damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        currentHealth =
            Mathf.Max(
                0,
                currentHealth
            );

        Debug.Log(
            $"{gameObject.name} HP: " +
            $"{currentHealth}/{maxHealth}"
        );

        StartCoroutine(
            ShowDamagePopup(damage)
        );

        ShowHP();

        if (enermyAudio != null)
        {
            enermyAudio.PlayHurt();
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.SetTrigger("Hurt");
        }

        StartHurt();

        ApplyKnockback(
            knockbackDirection,
            knockbackStrength
        );
    }

    /*
     * Dành cho FireBreath, Tornado và các skill
     * truyền hướng nhưng dùng lực mặc định của enemy.
     */
    public void TakeDamage(
        int damage,
        Vector2 knockbackDirection)
    {
        TakeDamage(
            damage,
            knockbackDirection,
            knockbackForce
        );
    }

    /*
     * Damage không có hướng thì không knockback.
     */
    public void TakeDamage(int damage)
    {
        TakeDamage(
            damage,
            Vector2.zero,
            0f
        );
    }
    private void ApplyKnockback(
    Vector2 direction,
    float strength)
    {
        if (movement == null ||
            direction.sqrMagnitude <= 0.001f ||
            strength <= 0f)
        {
            return;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(
                knockbackCoroutine
            );
        }

        knockbackCoroutine =
            StartCoroutine(
                KnockbackRoutine(
                    direction.normalized,
                    strength
                )
            );
    }

    private IEnumerator KnockbackRoutine(
    Vector2 direction,
    float strength)
    {
        if (movement == null)
            yield break;

        movement.externalVelocity =
            direction *
            Mathf.Max(
                0f,
                strength
            );

        yield return new WaitForSeconds(
            Mathf.Max(
                0.01f,
                knockbackTime
            )
        );

        if (movement != null)
        {
            movement.externalVelocity =
                Vector2.zero;
        }

        knockbackCoroutine = null;
    }

    private void StartHurt()
    {
        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );
        }

        hurtCoroutine =
            StartCoroutine(
                HurtRoutine()
            );
    }

    private IEnumerator HurtRoutine()
    {
        if (normalAttack != null)
        {
            normalAttack.CancelAttack();
        }

        if (specialAttack != null)
        {
            specialAttack.CancelAttack();
        }

        if (movement != null)
        {
            movement.CanMove = false;
            movement.StopMove();
        }

        yield return new WaitForSeconds(
            Mathf.Max(
                0.01f,
                hurtLockTime
            )
        );

        if (isDead)
        {
            hurtCoroutine = null;
            yield break;
        }

        if (movement != null)
        {
            movement.CanMove = true;
            movement.ResumeAI();
        }

        hurtCoroutine = null;
    }

    public void EndHurt()
    {
        if (isDead)
            return;

        if (hurtCoroutine != null)
        {
            StopCoroutine(
                hurtCoroutine
            );

            hurtCoroutine = null;
        }

        if (movement != null)
        {
            movement.CanMove = true;
            movement.ResumeAI();
        }
    }

    private IEnumerator ShowDamagePopup(
        int damage)
    {
        yield return null;

        if (DamagePopupManager.Instance != null)
        {
            DamagePopupManager.Instance.ShowDamage(
                damage,
                transform.position +
                Vector3.up * 0.8f
            );
        }
    }

    private void ShowHP()
    {
        if (hpCanvas == null)
            return;

        hpCanvas.enabled = true;

        hpTimer =
            Mathf.Max(
                0.1f,
                hpDisplayTime
            );
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        currentHealth = 0;

        StopRunningCoroutines();

        if (movement != null)
        {
            movement.externalVelocity =
                Vector2.zero;

            movement.StopImmediately();
            movement.enabled = false;
        }
        else if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        if (normalAttack != null)
        {
            normalAttack.enabled = false;
        }

        if (specialAttack != null)
        {
            specialAttack.enabled = false;
        }

        foreach (
            Collider2D col
            in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        if (hpCanvas != null)
        {
            hpCanvas.enabled = false;
        }

        if (enermyAudio != null)
        {
            enermyAudio.PlayDeath();
        }

        if (animator != null)
        {
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Death");
            animator.SetTrigger("Death");
        }

        Debug.Log(
            $"{gameObject.name} Dead"
        );

        destroyCoroutine =
            StartCoroutine(
                DeathRoutine()
            );
    }

    private void StopRunningCoroutines()
    {
        if (hurtCoroutine != null)
        {
            StopCoroutine(hurtCoroutine);
            hurtCoroutine = null;
        }

        if (knockbackCoroutine != null)
        {
            StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }

        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                deathDelay
            )
        );

        CompleteDeath();
    }

    private void CompleteDeath()
    {
        if (!isDead)
            return;

        GiveDeathRewards();

        if (destroyCoroutine != null)
        {
            StopCoroutine(
                destroyCoroutine
            );

            destroyCoroutine = null;
        }

        Destroy(gameObject);
    }

    private void GiveDeathRewards()
    {
        if (rewardsGiven)
            return;

        rewardsGiven = true;

        /*
         * EnermyItemDrop tự xử lý ItemData
         * và phần thưởng riêng của nó.
         */
        if (itemDrop != null)
        {
            itemDrop.DropItems();
        }

        DropCoins();
    }

    private void DropCoins()
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning(
                $"{name}: chưa gán Coin Prefab.",
                this
            );

            return;
        }

        if (Random.value > dropRate)
            return;

        int minimum =
            Mathf.Max(
                1,
                minCoinCount
            );

        int maximum =
            Mathf.Max(
                minimum,
                maxCoinCount
            );

        int amount =
            Random.Range(
                minimum,
                maximum + 1
            );

        for (int i = 0;
             i < amount;
             i++)
        {
            Vector2 offset =
                Random.insideUnitCircle *
                Mathf.Max(
                    0f,
                    coinScatterRadius
                );

            Instantiate(
                coinPrefab,
                transform.position +
                (Vector3)offset,
                Quaternion.identity
            );
        }

        Debug.Log(
            $"{name} rơi {amount} coin."
        );
    }

    // Animation Event ở cuối animation Death
    public void OnDeathFinished()
    {
        CompleteDeath();
    }

    private void OnValidate()
    {
        maxHealth =
            Mathf.Max(
                1,
                maxHealth
            );

        deathDelay =
            Mathf.Max(
                0f,
                deathDelay
            );

        hpDisplayTime =
            Mathf.Max(
                0.1f,
                hpDisplayTime
            );

        hurtLockTime =
            Mathf.Max(
                0f,
                hurtLockTime
            );

        knockbackForce =
            Mathf.Max(
                0f,
                knockbackForce
            );

        knockbackTime =
            Mathf.Max(
                0.01f,
                knockbackTime
            );

        minCoinCount =
            Mathf.Max(
                1,
                minCoinCount
            );

        maxCoinCount =
            Mathf.Max(
                minCoinCount,
                maxCoinCount
            );

        coinScatterRadius =
            Mathf.Max(
                0f,
                coinScatterRadius
            );
    }
}