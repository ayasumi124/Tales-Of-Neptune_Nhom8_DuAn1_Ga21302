using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;

    [Header("Stamina")]
    public float dashStaminaCost = 50f;

    private PlayerStamina stamina;

    public bool IsDashing { get; private set; }

    Rigidbody2D rb;
    Players player;
    Animator animator;

    float cooldownTimer;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Players>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        if (IsDashing)
            return;

        if (Input.GetKeyDown(KeyCode.L) &&
    cooldownTimer <= 0 &&
    !stamina.IsExhausted)
{
    if (stamina.UseStamina(dashStaminaCost))
    {
        StartCoroutine(Dash());
    }
}
    }

    IEnumerator Dash()
    {
        IsDashing = true;
        cooldownTimer = dashCooldown;

        Vector2 dir = player.LastDirection;

        rb.linearVelocity = dir * dashSpeed;

        animator.SetTrigger("Dash");

        float timer = dashDuration;

        while (timer > 0)
        {
            rb.linearVelocity = dir * dashSpeed;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        IsDashing = false;
    }
}