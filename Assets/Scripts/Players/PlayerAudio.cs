using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource source;

    public AudioClip hurtSound;
    public AudioClip deathSound;

    public void PlayHurt()
    {
        source.PlayOneShot(hurtSound);
    }

    public void PlayDeath()
    {
        source.PlayOneShot(deathSound);
    }
}