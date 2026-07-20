using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Players : MonoBehaviour
{
    private float tocDo;
    private float tocDoChay;
    float moveX;
    float moveY;
    public Rigidbody2D rb;
    public Animator animator;
    public float FacingDirection { get; private set; } = 1;
    public Vector2 LastDirection { get; private set; } = Vector2.down;

    private Attack attack;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        tocDo = 1.5f;
    }
    void Start()
    {
        Debug.Log("Players script is running.");
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        attack = GetComponent<Attack>();
        tocDoChay = tocDo;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveX * tocDo, moveY * tocDo);
    }
    void Update()
    {
        //Player Movement based on axis input
        moveX = Input.GetAxisRaw("Horizontal");
        moveY = Input.GetAxisRaw("Vertical");

        if (moveX > 0)
            FacingDirection = 1;
        else if (moveX < 0)
            FacingDirection = -1;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            tocDo = tocDoChay * 2;
        }
        else
        {
            tocDo = tocDoChay;
        }
        //Player Animation running based on blend tree but when standing animation idle still working, Paramater MoveX and MoveY is float type, so we can use the value of moveX and moveY to set the parameter in the animator

        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveY", moveY);
        //Player Animation idle based on blend tree, Paramater MoveX and MoveY is float type, so we can use the value of moveX and moveY to set the parameter in the animator
        bool isMoving = moveX != 0 || moveY != 0;

        if (isMoving)
        {
            animator.SetFloat("LastMoveX", moveX);
            animator.SetFloat("LastMoveY", moveY);
            LastDirection = new Vector2(moveX, moveY).normalized;
        }

        animator.SetBool("IsMoving", isMoving);

        

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            AudioManager.Instance.PlayFootstep(true); // Nhân vật chạy -> Bật tiếng chân
        }
        else
        {
            AudioManager.Instance.PlayFootstep(false); // Nhân vật đứng yên -> Tắt tiếng chân
        }

        // decrease health amount when the player presses the "H" key
         if (Input.GetKeyDown(KeyCode.H))
         {
             Health health = GetComponent<Health>();
             if (health != null)
             {
                health.TakeDamage(1f);
            }
         }
        



        //     //animation attack 4 on idle direction
        //     if (Input.GetKeyDown(KeyCode.Space))
        //     {
        //         animator.SetTrigger("Attack");
        //     }
        //     //attack up
        //     if (Input.GetKeyDown(KeyCode.Space) && animator.GetFloat("LastMoveY") > 0)
        //     {
        //         animator.SetTrigger("AttackUp");
        //     }
        //     //attack down
        //     if (Input.GetKeyDown(KeyCode.Space) && animator.GetFloat("LastMoveY") < 0)
        //     {
        //         animator.SetTrigger("AttackDown");
        //     }
        //     //attack left
        //     if (Input.GetKeyDown(KeyCode.Space) && animator.GetFloat("LastMoveX") < 0)
        //     {
        //         animator.SetTrigger("AttackLeft");
        //     }
        //     //attack right
        //     if (Input.GetKeyDown(KeyCode.Space) && animator.GetFloat("LastMoveX") > 0)
        //     {
        //         animator.SetTrigger("AttackRight");
        //     }
    }
}
