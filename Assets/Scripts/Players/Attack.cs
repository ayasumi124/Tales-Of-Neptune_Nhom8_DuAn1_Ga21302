using UnityEngine;

public class Attack : MonoBehaviour
{
    private Animator animator;
    private bool isAttacking;
    private Rigidbody2D rb;
    private int combo = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        isAttacking = true;
    }

    void Update()
    {

        

        //phát âm thanh đánh khi di chuyển
        if (Input.GetKeyDown(KeyCode.J) && isAttacking && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            isAttacking = true;
            animator.SetInteger("Combo", combo);

            animator.SetTrigger("Attack");
            combo = (combo + 1) % 2;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
            Debug.Log("player is moving and attacking");
        }
        //phát âm thanh đánh khi đứng im lặng, không di chuyển
        if (Input.GetKeyDown(KeyCode.J) && isAttacking && rb.linearVelocity.sqrMagnitude <= 0.1f)
        {
            isAttacking = true;
            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSound);
            animator.SetInteger("Combo", combo);

            animator.SetTrigger("Attack");

            combo = (combo + 1) % 2;

        }

        if (!isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        //tốc độ đánh khi nhấn nút J, nếu nhấn liên tục thì tốc độ đánh sẽ nhanh hơn

    }

    public void EndAttack()
    {
        isAttacking = false;
    }

}