using UnityEngine;

public class EnermyMovement : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float detectRange = 6f;

    [HideInInspector]
    public bool CanMove = true;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }
    }

    void Update()
    {
        if (player == null)
            return;

        if (!CanMove)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        float distance =
            Vector2.Distance(transform.position, player.position);

        if (distance > detectRange)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            return;
        }

        Vector2 dir =
            (player.position - transform.position).normalized;

        rb.linearVelocity = dir * moveSpeed;

        animator.SetBool("IsMoving", true);

        // Flip nếu chỉ có 1 animation chạy
        if (dir.x > 0.05f)
            spriteRenderer.flipX = false;
        else if (dir.x < -0.05f)
            spriteRenderer.flipX = true;

        // Hướng Idle + Attack
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animator.SetFloat("LastMoveX", Mathf.Sign(dir.x));
            animator.SetFloat("LastMoveY", 0);
        }
        else
        {
            animator.SetFloat("LastMoveX", 0);
            animator.SetFloat("LastMoveY", Mathf.Sign(dir.y));
        }
    }
}