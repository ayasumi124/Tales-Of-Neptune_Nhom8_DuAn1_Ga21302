using UnityEngine;
using System.Collections;
public class Attack : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    private bool isAttacking;
    public bool IsAttacking => isAttacking;


    public Transform[] attackPoint;
    public LayerMask enermyLayer;

    [Header("Attack")]
    public float attackRadius = 0.6f;
    public float attackDistance = 0.6f;
    public float attackCooldown = 0.35f;
    public int damage = 20;

    [Header("Combo")]
    private int combo = 0;

    public int maxCombo = 3;


    private bool queueNextAttack;

    private bool comboWindowOpen;

    [Header("Lunge")]
    public float lungeForce = 2f;
    public float lungeTime = 0.06f;
    public float[] comboLunge =
{
    3.5f,
    5f,
    8f // hit cuối
};





    [Header("Combo Speed")]
    public float[] comboCooldown =
{
    0.22f,
    0.16f,
    0.12f
};

    public float[] comboAnimationSpeed =
{
    1.4f,
    1.7f,
    2.0f
};

    [Header("Combo Damage")]
    public int[] comboDamage =
    {
    20, // Attack1
    25, // Attack2
    35  // Attack3
};

    [Header("Combo Knockback")]
    public float[] comboKnockback =
    {
    4f, // Attack1
    6f, // Attack2
    8f  // Attack3
};

    public float comboFinishDelay = 0.45f;
    public AbilityData skillData;

    public SkillSlotUI slotUI;

    IEnumerator Lunge(Vector2 dir, float speed)
    {
        float t = 0.08f;

        while (t > 0)
        {
            rb.linearVelocity = dir * speed;
            t -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
    }

    void Start()
    {
        slotUI.Setup(skillData);
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        isAttacking = false;
    }

    void Update()
    {
        Health hp = GetComponent<Health>();

        if (hp != null && hp.IsHurting)
            return;

        PlayerDash dash = GetComponent<PlayerDash>();

        if (dash != null && dash.IsDashing)
            return;

        if (Input.GetKeyDown(KeyCode.J) ||
            Input.GetKeyDown(KeyCode.Mouse0))
        {
            if (!isAttacking)
            {
                if (AbilityManager.Instance.attack.cooldown <= 0)
                    StartAttack();
            }
            else if (comboWindowOpen)
            {
                queueNextAttack = true;
            }
        }
    }
    public void CancelAttack()
    {
        isAttacking = false;

        combo = 0;

        queueNextAttack = false;

        comboWindowOpen = false;

        animator.speed = 1f;

        animator.ResetTrigger("Attack");
    }
    void StartAttack()
    {

        queueNextAttack = false;
        comboWindowOpen = false;

        isAttacking = true;

        int index = Mathf.Clamp(combo, 0, maxCombo - 1);

        animator.speed = comboAnimationSpeed[index];

        animator.SetInteger("Combo", combo);

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Attack");


        AbilityManager.Instance.attack.cooldown = comboCooldown[index];
        AbilityManager.Instance.attack.maxCooldown = comboCooldown[index];

        AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
    }


    // Animation Event
    public void DealDamage()
    {
        Players player = GetComponent<Players>();

        int currentCombo = Mathf.Clamp(
            combo,
            0,
            comboLunge.Length - 1
        );

        StopAllCoroutines();
        StartCoroutine(
            Lunge(player.LastDirection, comboLunge[currentCombo])
        );

        foreach (Transform point in attackPoint)
        {
            if (point == null)
                continue;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                point.position,
                attackRadius,
                enermyLayer);

            foreach (Collider2D hit in hits)
            {
                EnermyHealth hp = hit.GetComponent<EnermyHealth>();

                if (hp == null)
                    continue;

                Vector2 dir =
                    (hp.transform.position - transform.position).normalized;

                hp.knockbackForce = comboKnockback[currentCombo];

                hp.TakeDamage(comboDamage[currentCombo], dir);
            }
        }
    }

    // Animation Event (đặt ở frame cuối animation)
    public void EndAttack()
    {
        isAttacking = false;

        animator.speed = 1f;

        comboWindowOpen = false;

        if (queueNextAttack)
        {
            queueNextAttack = false;

            combo++;

            if (combo >= maxCombo)
            {
                combo = 0;

                AbilityManager.Instance.attack.maxCooldown =
                    comboFinishDelay;

                AbilityManager.Instance.attack.cooldown =
                    comboFinishDelay;

                comboWindowOpen = false;

                return;
            }

            StartAttack();

            return;
        }

        combo = 0;
    }

    public void OpenComboWindow()
    {
        comboWindowOpen = true;
    }

    public void CloseComboWindow()
    {
        comboWindowOpen = false;
    }


}