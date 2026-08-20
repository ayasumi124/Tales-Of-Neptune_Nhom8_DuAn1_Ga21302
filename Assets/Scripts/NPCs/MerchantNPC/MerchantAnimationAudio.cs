using UnityEngine;

public class MerchantAnimationAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip animationLoopSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float volume = 1f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource =
                GetComponent<AudioSource>();
        }

        SetupAudio();
    }

    private void Start()
    {
        PlayLoop();
    }

    private void SetupAudio()
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = volume;
    }

    public void PlayLoop()
    {
        if (audioSource == null)
        {
            Debug.LogWarning(
                "Merchant chưa có AudioSource."
            );

            return;
        }

        if (animationLoopSound == null)
        {
            Debug.LogWarning(
                "Merchant chưa được gán Animation Loop Sound."
            );

            return;
        }

        audioSource.clip =
            animationLoopSound;

        audioSource.loop = true;
        audioSource.volume = volume;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public void StopLoop()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
    }

    private void OnDisable()
    {
        StopLoop();
    }
}