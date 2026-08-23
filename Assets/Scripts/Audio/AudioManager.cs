using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField] private AudioSource elementSkillSource;

    [SerializeField] private AudioSource weatherSource;

    public AudioSource SFXSource => sfxSource;

    [Header("Volume")]
    [Range(0f, 1f)]
    public float dashVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 1f;

    [SerializeField] private float musicFadeDuration = 0.5f;
    [SerializeField]
    private float itemVolumeMultiplier = 2f;

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
    public AudioClip skillUnlockSound;
    public AudioClip skillCloseSound;
    public AudioClip errorSound;

    [Header("Chest")]
    public AudioClip chestOpenSound;

    private Coroutine musicCoroutine;
    [Header("Inventory")]
    public AudioClip inventoryOpenSound;

    public AudioClip inventoryCloseSound;
    public AudioClip inventoryMoveSound;      // di chuyển giữa các ô
    public AudioClip inventorySelectSound;    // click chọn item
    public AudioClip inventoryUseSound;       // Use
    public AudioClip inventoryEquipSound;     // Equip Sword/Armor
    public AudioClip inventoryShortcutSound;  // Equip Shortcut
    public AudioClip inventoryDropSound;      // Drop item (sau này)

    [Header("Shop")]
    public AudioClip shopOpenSound;
    public AudioClip shopCloseSound;
    public AudioClip shopBuySound;
    public AudioClip shopErrorSound;


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

    private void OnEnable()
    {
        SceneManager.sceneUnloaded +=
            OnSceneUnloaded;

        SceneManager.sceneLoaded +=
            OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -=
            OnSceneUnloaded;

        SceneManager.sceneLoaded -=
            OnSceneLoaded;
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
        SetupSource(
            musicSource,
            true
        );

        SetupSource(
            footstepSource,
            true
        );

        SetupSource(
            sfxSource,
            false
        );

        SetupSource(
            elementSkillSource,
            false
        );

        SetupSource(
            weatherSource,
             true
);  

        if (musicSource != null)
            musicSource.volume = musicVolume;
    }

    private void SetupSource(
        AudioSource source,
        bool loop)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
    }

    private void OnSceneUnloaded(
    Scene scene)
{
    /*
     * Hủy meteor trước để AudioSource riêng
     * trên từng meteor được dừng ngay.
     */
    FireMeteor.StopAllMeteors();

    /*
     * Dừng toàn bộ âm thanh gameplay
     * thuộc scene cũ.
     */
    StopGameplaySounds();

    /*
     * Weather Source nằm trên AudioManager
     * DontDestroyOnLoad nên BẮT BUỘC
     * phải dừng thủ công.
     */
    StopWeather();
}

    private void OnSceneLoaded(
    Scene scene,
    LoadSceneMode mode)
{
    /*
     * Dọn thêm lần nữa để chắc chắn
     * không còn sound của scene cũ.
     *
     * WeatherController của scene mới
     * sẽ gọi PlayWeather() trong Start()
     * nếu scene đó có thời tiết.
     */
    StopGameplaySounds();
    StopWeather();
}

    // =====================================================
    // GAMEPLAY SOUND CLEANUP
    // =====================================================

    public void StopGameplaySounds()
{
    StopSFX();
    StopElementSkillSound();
    StopFootstep();
}

public void StopAllSFX()
{
    StopGameplaySounds();
    StopWeather();
}


    private void StopSFX()
{
    if (sfxSource == null)
        return;

    sfxSource.Stop();
    sfxSource.clip = null;
    sfxSource.loop = false;
}
    public void StopElementSkillSound()
{
    if (elementSkillSource == null)
        return;

    elementSkillSource.Stop();
    elementSkillSource.clip = null;
    elementSkillSource.loop = false;
}

   public void StopFootstep()
{
    if (footstepSource == null)
        return;

    footstepSource.Stop();
    footstepSource.clip = null;
}


    // =====================================================
    // ELEMENT SKILL AUDIO
    // =====================================================

    public void PlayElementSkillSFX(
        AudioClip clip,
        float volume = 1f)
    {
        if (clip == null)
            return;

        if (elementSkillSource == null)
        {
            Debug.LogError(
                "AudioManager chưa được gán Element Skill Source."
            );

            return;
        }

        /*
         * PlayOneShot cho phép nhiều meteor impact
         * phát gần nhau trên cùng một source.
         * Stop() sẽ dừng toàn bộ các OneShot này.
         */
        elementSkillSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }

    public void PlayItemSFX(AudioClip clip, float volume)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume * itemVolumeMultiplier)
        );
    }

    public void PlayElementSkillLoop(
        AudioClip clip,
        float volume = 1f)
    {
        if (clip == null ||
            elementSkillSource == null)
        {
            return;
        }

        elementSkillSource.Stop();

        elementSkillSource.clip = clip;
        elementSkillSource.volume =
            Mathf.Clamp01(volume);

        elementSkillSource.loop = true;
        elementSkillSource.Play();
    }

    // =====================================================
    // NORMAL SFX
    // =====================================================

    public void PlaySFX(
        AudioClip clip)
    {
        PlaySFX(
            clip,
            1f
        );
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

    // =====================================================
    // FOOTSTEP
    // =====================================================

    public void PlayFootstep(
        bool isMoving)
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

    // =====================================================
    // WEATHER
    // =====================================================

   
public void PlayWeather(
    AudioClip clip,
    float volume = 0.5f)
{
    if (weatherSource == null)
    {
        Debug.LogWarning(
            "AudioManager: Weather Source chưa được gán.",
            this
        );

        return;
    }

    if (clip == null)
    {
        StopWeather();
        return;
    }

    /*
     * Nếu đang phát đúng weather hiện tại
     * thì chỉ cập nhật volume.
     *
     * Không restart clip để tránh tiếng mưa
     * bị giật.
     */
    if (weatherSource.clip == clip &&
        weatherSource.isPlaying)
    {
        weatherSource.volume =
            Mathf.Clamp01(volume);

        return;
    }

    weatherSource.Stop();

    weatherSource.clip = clip;
    weatherSource.loop = true;
    weatherSource.playOnAwake = false;

    weatherSource.volume =
        Mathf.Clamp01(volume);

    weatherSource.Play();
}
    public void StopWeather()
{
    if (weatherSource == null)
        return;

    weatherSource.Stop();
    weatherSource.clip = null;

    /*
     * Weather luôn là loop.
     * Không bắt buộc nhưng giữ source
     * đúng cấu hình sau khi Stop.
     */
    weatherSource.loop = true;
}
    // =====================================================
    // MUSIC
    // =====================================================

    public void PlayMusic(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
        }

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void ChangeMusic(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            return;
        }

        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );
        }

        musicCoroutine =
            StartCoroutine(
                ChangeMusicRoutine(clip)
            );
    }

    public void PlayInventoryOpen()
    {
        PlaySFX(inventoryOpenSound);
    }

    public void PlayInventoryClose()
    {
        PlaySFX(inventoryCloseSound);
    }

    public void PlayInventoryMove()
    {
        PlaySFX(inventoryMoveSound);
    }

    public void PlayInventorySelect()
    {
        PlaySFX(inventorySelectSound);
    }

    public void PlayInventoryUse()
    {
        PlaySFX(inventoryUseSound);
    }

    public void PlayInventoryEquip()
    {
        PlaySFX(inventoryEquipSound);
    }

    public void PlayInventoryShortcut()
    {
        PlaySFX(inventoryShortcutSound);
    }

    public void PlayInventoryDrop()
    {
        PlaySFX(inventoryDropSound);
    }

    // =====================================================
    // SHOP
    // =====================================================

    public void PlayShopOpen()
    {
        PlaySFX(shopOpenSound);
    }

    public void PlayShopClose()
    {
        PlaySFX(shopCloseSound);
    }

    public void PlayShopBuy()
    {
        PlaySFX(shopBuySound);
    }

    public void PlayShopError()
    {
        PlaySFX(shopErrorSound);
    }


    private IEnumerator ChangeMusicRoutine(
        AudioClip newClip)
    {
        float duration =
            Mathf.Max(
                0f,
                musicFadeDuration
            );

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

        musicSource.volume =
            musicVolume;

        musicCoroutine = null;
    }

    public void StopMusic(
        bool fadeOut = true)
    {
        if (musicSource == null)
            return;

        if (musicCoroutine != null)
        {
            StopCoroutine(
                musicCoroutine
            );

            musicCoroutine = null;
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
            musicSource.volume =
                musicVolume;
        }
    }

    private IEnumerator StopMusicRoutine()
    {
        float duration =
            Mathf.Max(
                0f,
                musicFadeDuration
            );

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
        musicSource.volume =
            musicVolume;

        musicCoroutine = null;
    }

    public void SetMusicVolume(
        float volume)
    {
        musicVolume =
            Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneUnloaded -=
            OnSceneUnloaded;

        SceneManager.sceneLoaded -=
            OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }
}