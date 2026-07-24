using UnityEngine;

public class CloneAudio : MonoBehaviour
{
    public AudioClip hurtSound;
    public AudioClip deathSound;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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