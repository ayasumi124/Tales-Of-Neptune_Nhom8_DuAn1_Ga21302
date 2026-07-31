using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance
    {
        get;
        private set;
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource sfxSource;

    public AudioSource SFXSource => sfxSource;
    [Range(0f, 20f)]
    public float dashVolume = 20f;
    [Header("Music Settings")]
    [SerializeField] private float musicVolume = 1f;
    [SerializeField] private float musicFadeDuration = 0.5f;

    [Header("Player")]
    public AudioClip footstepSound;
    public AudioClip attackSound;
    public AudioClip jumpSound;
    public AudioClip dashSound;

    [Header("Default Music")]
    public AudioClip backgroundMusic;

    [Header("Item")]
    public AudioClip coinPickupSound;
    public AudioClip coinDropSound;
    public AudioClip coinBounceSound;

    [Header("UI")]
    public AudioClip buttonSound;
    public AudioClip openInventorySound;
    public AudioClip skillUnlockSound;
    public AudioClip skillCloseSound;
    public AudioClip errorSound;

    [Header("Chest")]
    public AudioClip chestOpenSound;

    private Coroutine musicCoroutine;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void Start()
    {
        if (backgroundMusic != null &&
            musicSource != null &&
            musicSource.clip == null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    private void SetupAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = musicVolume;
        }

        if (footstepSource != null)
        {
            footstepSource.loop = true;
            footstepSource.playOnAwake = false;
        }

        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicCoroutine = null;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void ChangeMusic(AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        /*
         * Nếu đang phát đúng bài này thì
         * không restart lại từ đầu.
         */
        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }

        musicCoroutine =
            StartCoroutine(
                ChangeMusicRoutine(clip)
            );
    }

    private IEnumerator ChangeMusicRoutine(
        AudioClip newClip)
    {
        float duration =
            Mathf.Max(0f, musicFadeDuration);

        if (musicSource.isPlaying &&
            musicSource.clip != null &&
            duration > 0f)
        {
            float startVolume =
                musicSource.volume;

            float timer = 0f;

            while (timer < duration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                float progress =
                    Mathf.Clamp01(
                        timer / duration
                    );

                musicSource.volume =
                    Mathf.Lerp(
                        startVolume,
                        0f,
                        progress
                    );

                yield return null;
            }
        }

        musicSource.Stop();

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        if (duration <= 0f)
        {
            musicSource.volume =
                musicVolume;

            musicCoroutine = null;
            yield break;
        }

        float fadeTimer = 0f;

        while (fadeTimer < duration)
        {
            fadeTimer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    fadeTimer / duration
                );

            musicSource.volume =
                Mathf.Lerp(
                    0f,
                    musicVolume,
                    progress
                );

            yield return null;
        }

        musicSource.volume = musicVolume;
        musicCoroutine = null;
    }

    public void StopMusic(bool fadeOut = true)
    {
        if (musicSource == null)
            return;

        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
        }

        if (fadeOut)
        {
            musicCoroutine =
                StartCoroutine(
                    StopMusicRoutine()
                );
        }
        else
        {
            musicSource.Stop();
            musicSource.clip = null;
            musicSource.volume = musicVolume;
        }
    }

    private IEnumerator StopMusicRoutine()
    {
        float duration =
            Mathf.Max(0f, musicFadeDuration);

        float startVolume =
            musicSource.volume;

        float timer = 0f;

        while (timer < duration)
        {
            timer +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / duration
                );

            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    progress
                );

            yield return null;
        }

        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = musicVolume;

        musicCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume =
            Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null ||
            sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(
        AudioClip clip,
        float volume)
    {
        if (clip == null ||
            sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    public void PlayFootstep(bool isMoving)
    {
        if (footstepSource == null)
            return;

        if (!isMoving)
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();

            return;
        }

        if (footstepSound == null)
            return;

        if (footstepSource.clip !=
            footstepSound)
        {
            footstepSource.clip =
                footstepSound;
        }

        footstepSource.loop = true;

        if (!footstepSource.isPlaying)
            footstepSource.Play();
    }

    public void StopFootstep()
    {
        if (footstepSource != null)
            footstepSource.Stop();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}