using UnityEngine;

public class EnermyAudio : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    public void PlayAttack()
    {
        if (attackSound != null)
            audioSource.PlayOneShot(attackSound);
    }

    public void PlayHurt()
    {
        if (hurtSound != null)
            audioSource.PlayOneShot(hurtSound);
    }

    public void PlayDeath()
    {
        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);
    }
}