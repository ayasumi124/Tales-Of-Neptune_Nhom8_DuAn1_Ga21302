using UnityEngine;

public class Players : MonoBehaviour
{
    private Vector3 lastPos;

    private float tocDo;
    private float tocDoChay;

    private float moveX;
    private float moveY;

    public Rigidbody2D rb;
    public Animator animator;

    private PlayerStamina stamina;
    private Attack attack;
    private PlayerDash dash;

    public float FacingDirection { get; private set; } = 1f;
    public Vector2 LastDirection { get; private set; } = Vector2.down;

    public static Players Instance;
    public Transform pickupPoint;

    public bool IsControlLocked { get; private set; }

    public bool AutoMove { get; private set; }

    private Vector2 autoMoveDir;
    private float autoMoveSpeed;

    private void Awake()
    {
        tocDo = 1.5f;
    }

    private void Start()
    {
        dash = GetComponent<PlayerDash>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attack = GetComponent<Attack>();
        stamina = GetComponent<PlayerStamina>();

        tocDoChay = tocDo;

        Debug.Log("Players script is running.");
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        if (AutoMove)
        {
            rb.linearVelocity =
                autoMoveDir * autoMoveSpeed;

            return;
        }

        if (IsControlLocked)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Health health = GetComponent<Health>();

        if (health != null && health.IsDead)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (attack != null &&
            attack.IsAttacking)
        {
            rb.linearVelocity *= 0.9f;
            return;
        }

        if (dash != null &&
            dash.IsDashing)
        {
            return;
        }

        rb.linearVelocity =
            new Vector2(
                moveX * tocDo,
                moveY * tocDo
            );
    }

    private void Update()
    {
        if (AutoMove)
        {
            if (AudioManager.Instance != null)
            {
                bool moving =
                    rb != null &&
                    rb.linearVelocity.sqrMagnitude > 0.05f;

                AudioManager.Instance.PlayFootstep(moving);
            }

            return;
        }
        // Khi đang load scene thì không nhận input
        if (IsControlLocked)
        {
            moveX = 0f;
            moveY = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsRunning", false);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFootstep(false);

            return;
        }

        if (attack != null && attack.IsAttacking)
            return;

        if (dash != null && dash.IsDashing)
        {
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            return;
        }

        Health health = GetComponent<Health>();

        if (health != null && health.IsDead)
        {
            moveX = 0f;
            moveY = 0f;

            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetBool("IsRunning", false);
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayFootstep(false);

            return;
        }

        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        if (moveX > 0f)
            FacingDirection = 1f;
        else if (moveX < 0f)
            FacingDirection = -1f;

        if (stamina != null)
            stamina.Recover();

        bool isRunning =
            rb != null &&
            rb.linearVelocity.sqrMagnitude > 0.01f;

        bool canRun =
            Input.GetKey(KeyCode.LeftShift) &&
            stamina != null &&
            !stamina.IsExhausted &&
            stamina.currentStamina > 0f &&
            isRunning &&
            (attack == null || !attack.IsAttacking);

        if (animator != null)
            animator.SetBool("IsRunning", canRun);

        if (canRun)
        {
            tocDo = tocDoChay * 2f;
            stamina.Drain();
        }
        else
        {
            tocDo = tocDoChay;
        }

        if (animator != null)
        {
            animator.SetFloat("MoveX", moveX);
            animator.SetFloat("MoveY", moveY);
        }

        bool isMoving = moveX != 0f || moveY != 0f;

        if (isMoving)
        {
            if (animator != null)
            {
                animator.SetFloat("LastMoveX", moveX);
                animator.SetFloat("LastMoveY", moveY);
            }

            LastDirection =
                new Vector2(moveX, moveY).normalized;
        }

        if (animator != null)
            animator.SetBool("IsMoving", isMoving);

        if (AudioManager.Instance != null)
        {
            bool moving =
                rb != null &&
                rb.linearVelocity.sqrMagnitude > 0.1f;

            AudioManager.Instance.PlayFootstep(moving);
        }

        // Test mất máu
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (health != null)
                health.TakeDamage(1f);
        }
    }

    private void LateUpdate()
    {
        if (transform.position != lastPos)
        {
            Debug.Log(
                Time.frameCount + " " +
                gameObject.name + " " +
                transform.position
            );

            lastPos = transform.position;
        }
    }
    public void LockControl()
    {
        IsControlLocked = true;

        moveX = 0f;
        moveY = 0f;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFootstep(false);
    }

    public void AutoWalk(
    Vector2 dir,
    float speed)
    {
        if (dir.sqrMagnitude <= 0.001f)
            return;

        AutoMove = true;
        IsControlLocked = false;
        autoMoveDir = dir.normalized;
        autoMoveSpeed = speed;

        moveX = 0f;
        moveY = 0f;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (autoMoveDir.x > 0f)
            FacingDirection = 1f;
        else if (autoMoveDir.x < 0f)
            FacingDirection = -1f;

        LastDirection = autoMoveDir;

        if (animator != null)
        {
            animator.SetFloat(
                "MoveX",
                autoMoveDir.x
            );

            animator.SetFloat(
                "MoveY",
                autoMoveDir.y
            );

            animator.SetFloat(
                "LastMoveX",
                autoMoveDir.x
            );

            animator.SetFloat(
                "LastMoveY",
                autoMoveDir.y
            );

            animator.SetBool(
                "IsMoving",
                true
            );

            animator.SetBool(
                "IsRunning",
                false
            );
        }
    }
    public void StopAutoWalk()
    {
        AutoMove = false;

        autoMoveDir = Vector2.zero;
        autoMoveSpeed = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.SetBool(
                "IsMoving",
                false
            );

            animator.SetBool(
                "IsRunning",
                false
            );
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayFootstep(false);
    }

    public void UnlockControl()
    {
        IsControlLocked = false;

        moveX = 0f;
        moveY = 0f;

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsRunning", false);
        }
    }
}