using UnityEngine;
using System.Collections;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 2f;

    [Header("Stamina")]
    public float dashStaminaCost = 50f;

    private PlayerStamina stamina;

    public bool IsDashing { get; private set; }

    Rigidbody2D rb;
    Players player;
    Animator animator;
    public AbilityData skillData;

    public SkillSlotUI slotUI;

    void Start()
    {
        slotUI.Setup(skillData);
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Players>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina>();
    }

    void Update()
    {
        if (IsDashing)
            return;

        bool pressDash =
            Input.GetKeyDown(KeyCode.L) ||
            Input.GetKeyDown(KeyCode.Mouse1);

        if (pressDash &&
            AbilityManager.Instance != null &&
            AbilityManager.Instance.HasAbility(AbilityType.Dash) &&
            AbilityManager.Instance.dash.cooldown <= 0f &&
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

        AbilityManager.Instance.dash.maxCooldown =
            dashCooldown;

        AbilityManager.Instance.dash.cooldown =
            dashCooldown;

        AbilityManager.Instance.dash.maxDuration =
            dashDuration;

        AbilityManager.Instance.dash.duration =
            dashDuration;

        Vector2 dir = player.LastDirection;

        rb.linearVelocity = dir * dashSpeed;
        animator.SetTrigger("Dash");

        float timer = dashDuration;

        while (timer > 0f)
        {
            rb.linearVelocity = dir * dashSpeed;

            timer -= Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        IsDashing = false;
    }

    public void PlayDashSound()
    {
        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.dashSound,
            20f
        );
    }
}