using System.Collections;
using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip activateSound;

    [Header("Damage")]
    [Min(1)]
    [SerializeField]
    private int damage = 1;

    [Header("Cooldown")]
    [Min(0.1f)]
    [SerializeField]
    private float cooldown = 1f;

    private bool canActivate = true;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (!canActivate)
            return;

        Health health =
            other.GetComponentInParent<Health>();

        if (health == null ||
            health.IsDead)
        {
            return;
        }

        StartCoroutine(
            ActivateTrap(health)
        );
    }

    private IEnumerator ActivateTrap(
        Health playerHealth)
    {
        canActivate = false;

        if (animator != null)
        {
            animator.ResetTrigger("Activate");
            animator.SetTrigger("Activate");
        }

        if (audioSource != null &&
            activateSound != null)
        {
            audioSource.PlayOneShot(
                activateSound
            );
        }

        /*
         * Delay nhỏ để damage đúng lúc gai nhô lên.
         * Sau này có thể chuyển thành Animation Event.
         */
        yield return new WaitForSeconds(
            0.12f
        );

        if (playerHealth != null &&
            !playerHealth.IsDead)
        {
            playerHealth.TakeDamage(
                damage
            );
        }

        yield return new WaitForSeconds(
            Mathf.Max(
                0.1f,
                cooldown
            )
        );

        canActivate = true;
    }
} 