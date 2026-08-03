using UnityEngine;

public class EnermyAudio : MonoBehaviour
{
    public AudioSource footstepSource;
    public AudioSource sfxSource;

    public AudioClip footstepClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;
    public AudioClip deathClip;

    public void PlayFootstep(
        bool moving)
    {
        if (footstepSource == null)
            return;

        if (!moving)
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        if (footstepClip == null)
            return;

        if (footstepSource.clip !=
            footstepClip)
        {
            footstepSource.clip =
                footstepClip;
        }

        footstepSource.loop = true;

        if (!footstepSource.isPlaying)
            footstepSource.Play();
    }

    public void PlayAttack()
    {
        if (!gameObject.activeInHierarchy ||
            sfxSource == null ||
            attackClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            attackClip
        );
    }

    public void PlayHurt()
    {
        if (!gameObject.activeInHierarchy ||
            sfxSource == null ||
            hurtClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            hurtClip
        );
    }

    public void PlayDeath()
    {
        if (!gameObject.activeInHierarchy ||
            sfxSource == null ||
            deathClip == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            deathClip
        );
    }

    public void StopAudio()
    {
        if (footstepSource != null)
        {
            footstepSource.Stop();
            footstepSource.clip = null;
        }

        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
    }

    private void OnDisable()
    {
        StopAudio();
    }

    private void OnDestroy()
    {
        StopAudio();
    }
}