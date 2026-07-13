using UnityEngine;

public class CloneFollow : MonoBehaviour
{
    public Transform player;

    public float moveSpeed = 4f;
    public float followDistance = 1.8f;
    public float catchDistance = 0.3f;

    public AudioClip footstepSound;
    public AudioClip attackSound;

    private Animator animator;
    private Animator playerAnimator;
    private Players playerScript;

    private AudioSource footstepSource;
    private AudioSource attackSource;

    private bool attackPlaying = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        playerAnimator = player.GetComponent<Animator>();
        playerScript = player.GetComponent<Players>();

        AudioSource[] audio = GetComponents<AudioSource>();

        footstepSource = audio[0];
        attackSource = audio[1];

        footstepSource.clip = footstepSound;
        footstepSource.loop = true;
    }

    void Update()
    {
        if (player == null) return;

        // Luôn ở phía sau Player
        Vector3 target = player.position;
        target -= (Vector3)playerScript.LastDirection * followDistance;
        target.y += 0.4f;

        Vector2 dir = target - transform.position;

        bool moving = dir.magnitude > catchDistance;

        if (moving)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                target,
                moveSpeed * Time.deltaTime);

            Vector2 animDir = dir.normalized;

            // Chỉ lấy 4 hướng
            if (Mathf.Abs(animDir.x) > Mathf.Abs(animDir.y))
            {
                animDir.x = Mathf.Sign(animDir.x);
                animDir.y = 0;
            }
            else
            {
                animDir.y = Mathf.Sign(animDir.y);
                animDir.x = 0;
            }

            animator.SetBool("IsMoving", true);

            animator.SetFloat("MoveX", animDir.x);
            animator.SetFloat("MoveY", animDir.y);

            animator.SetFloat("LastMoveX", animDir.x);
            animator.SetFloat("LastMoveY", animDir.y);

            if (!footstepSource.isPlaying)
                footstepSource.Play();
        }
        else
        {
            animator.SetBool("IsMoving", false);

            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }

        // Đồng bộ Combo
        animator.SetInteger(
            "Combo",
            playerAnimator.GetInteger("Combo"));

        // Đồng bộ Attack
        AnimatorStateInfo state =
            playerAnimator.GetCurrentAnimatorStateInfo(0);

        if (state.IsTag("Attack"))
        {
            if (!attackPlaying)
            {
                attackPlaying = true;

                animator.SetTrigger("Attack");

                attackSource.PlayOneShot(attackSound);
            }
        }
        else
        {
            attackPlaying = false;
        }
    }
}