using UnityEngine;

public class EnermyAudio : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioSource sfxSource;

    public AudioClip footstepClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;

    public void PlayFootstep(bool moving)
    {
        if (moving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = footstepClip;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    public void PlayAttack()
    {
        sfxSource.PlayOneShot(attackClip);
    }

    public void PlayHurt()
    {
        sfxSource.PlayOneShot(hurtClip);
    }

    public void PlayDeath()
    {
        sfxSource.PlayOneShot(deathClip);
    }
}