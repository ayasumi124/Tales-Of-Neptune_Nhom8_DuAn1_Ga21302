using UnityEngine;
using UnityEngine.UIElements;

public class Slime : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float chaseRadius = 5f;  // Khoảng cách phát hiện Player
    public float attackRadius = 1.2f; // Khoảng cách để dừng lại chém

    [Header("Attack Settings")]
    public float attackCooldown = 1.5f; // Thời gian hồi chiêu (1.5 giây chém 1 lần)
    private float nextAttackTime = 0f;

    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        // Tự động tìm nhân vật chính bằng Tag "Player"
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Tính khoảng cách đến người chơi
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Nếu nằm trong tầm nhìn và chưa chạm tầm đánh -> Đuổi theo
        if (distanceToPlayer <= chaseRadius && distanceToPlayer > attackRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            
            // Di chuyển slime sao cho nó có vật lý vừa nhảy về phía trước (Chuẩn vật lý Unity 6)
            rb.MovePosition(rb.position + direction * moveSpeed * Time.fixedDeltaTime);
            

            // Lập mặt quái theo hướng di chuyển
            FlipSprite(direction.x);

            // Bật animation chạy nhảy của slime
            if (anim != null) anim.SetBool("IsMoving", true);
        }
        else
        {
            // Dừng lại khi mất dấu hoặc khi đã đứng sát để chuẩn bị chém
            rb.linearVelocity = Vector2.zero; 
            if (anim != null) anim.SetBool("IsMoving", false);

            // Nếu đứng sát sạt trong tầm đánh -> Vung đao chém
            if (distanceToPlayer <= attackRadius)
            {
                Attack();
            }
        }
    }

    void FlipSprite(float directionX)
    {
        if (directionX > 0.1f)
        {
            spriteRenderer.flipX = false; // Quay phải
        }
        else if (directionX < -0.1f)
        {
            spriteRenderer.flipX = true;  // Quay trái
        }
    }

    void Attack()
    {
        // Kiểm tra xem đã hết thời gian hồi chiêu chưa
        if (Time.time >= nextAttackTime)
        {
            if (anim != null)
            {
                // Kích hoạt Trigger "Attack" bạn vừa tạo trong Animator
                anim.SetTrigger("Attack"); 
            }

            // Tính toán thời gian cho phát chém tiếp theo
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    // Vẽ vòng tròn tầm nhìn (Vàng) và tầm đánh (Đỏ) ngoài Scene để dễ nhìn
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
        Gizmos.color = Color.red;    
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}