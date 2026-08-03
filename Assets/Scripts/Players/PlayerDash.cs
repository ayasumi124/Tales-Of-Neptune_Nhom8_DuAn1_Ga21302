using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 2f;

    [Header("Stamina")]
    public float dashStaminaCost = 50f;

    [Header("Ability")]
    public AbilityData skillData;
    public SkillSlotUI slotUI;

    public bool IsDashing { get; private set; }

    private PlayerStamina stamina;
    private Rigidbody2D rb;
    private Players player;
    private Animator animator;
    private Attack attack;
    private Health health;

    private Coroutine dashCoroutine;
[Header("Dash Collision")]
[SerializeField] private LayerMask obstacleLayer;
[SerializeField] private float skinWidth = 0.03f;

private Collider2D playerCollider;

private readonly RaycastHit2D[] dashHits =
    new RaycastHit2D[8];
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Players>();
        animator = GetComponent<Animator>();
        stamina = GetComponent<PlayerStamina>();
        attack = GetComponent<Attack>();
        health = GetComponent<Health>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (slotUI != null &&
            skillData != null)
        {
            slotUI.Setup(skillData);
        }
    }

    private void Update()
    {
        if (IsDashing)
            return;

        if (Time.timeScale <= 0f)
            return;

        if (health != null &&
            (health.IsDead ||
             health.IsHurting))
        {
            return;
        }

        bool pressDash =
            Input.GetKeyDown(KeyCode.L) ||
            Input.GetKeyDown(KeyCode.Mouse1);

        if (!pressDash)
            return;

        if (AbilityManager.Instance == null)
            return;

        if (!AbilityManager.Instance
                .HasAbility(AbilityType.Dash))
        {
            return;
        }

        if (AbilityManager.Instance
                .dash.cooldown > 0f)
        {
            return;
        }

        if (stamina == null ||
            stamina.IsExhausted)
        {
            return;
        }

        if (!stamina.UseStamina(
                dashStaminaCost))
        {
            return;
        }

        dashCoroutine =
            StartCoroutine(
                DashRoutine()
            );
    }

    private IEnumerator DashRoutine()
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

    Vector2 direction =
        GetDashDirection();

    if (animator != null &&
        (attack == null ||
         !attack.IsAttacking))
    {
        animator.ResetTrigger("Dash");
        animator.SetTrigger("Dash");
    }

    ContactFilter2D filter =
        new ContactFilter2D();

    filter.SetLayerMask(
        obstacleLayer
    );

    filter.useTriggers = false;

    float timer =
        Mathf.Max(
            0.01f,
            dashDuration
        );

    while (timer > 0f)
    {
        float moveDistance =
            dashSpeed *
            Time.fixedDeltaTime;

        if (playerCollider != null)
        {
            int hitCount =
                playerCollider.Cast(
                    direction,
                    filter,
                    dashHits,
                    moveDistance + skinWidth
                );

            if (hitCount > 0)
            {
                float closestDistance =
                    moveDistance;

                for (int i = 0;
                     i < hitCount;
                     i++)
                {
                    if (dashHits[i].collider == null)
                        continue;

                    closestDistance =
                        Mathf.Min(
                            closestDistance,
                            dashHits[i].distance
                        );
                }

                float safeDistance =
                    Mathf.Max(
                        0f,
                        closestDistance -
                        skinWidth
                    );

                rb.MovePosition(
                    rb.position +
                    direction *
                    safeDistance
                );

                break;
            }
        }

        rb.MovePosition(
            rb.position +
            direction *
            moveDistance
        );

        timer -=
            Time.fixedDeltaTime;

        yield return new WaitForFixedUpdate();
    }

    FinishDash();
}

    private Vector2 GetDashDirection()
    {
        if (player != null &&
            player.LastDirection.sqrMagnitude >
            0.001f)
        {
            return player.LastDirection.normalized;
        }

        return Vector2.down;
    }

    private void FinishDash()
{
    if (rb != null)
    {
        rb.linearVelocity =
            Vector2.zero;

        rb.angularVelocity = 0f;
    }

    IsDashing = false;
    dashCoroutine = null;
}

    public void CancelDash()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(
                dashCoroutine
            );

            dashCoroutine = null;
        }

        FinishDash();

        if (animator != null)
            animator.ResetTrigger("Dash");
    }

    public void PlayDashSound()
    {
        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance.PlaySFX(
            AudioManager.Instance.dashSound,
            AudioManager.Instance.dashVolume
        );
    }

    private void OnDisable()
    {
        if (IsDashing)
            CancelDash();
    }
}