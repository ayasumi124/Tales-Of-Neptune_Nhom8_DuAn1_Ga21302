using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Khởi tạo Singleton để các script khác dễ dàng gọi AudioManager.Instance
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;   // Dùng để phát nhạc nền (BGM) - luôn Loop
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource sfxSource;     // Dùng để phát hiệu ứng âm thanh (SFX) - dùng PlayOneShot

    [Header("Audio Clips")]
    [Header("Player")]
    public AudioClip footstepSound;
    public AudioClip attackSound;
    public AudioClip jumpSound;
    [Header("Audio Music")]
    public AudioClip backgroundMusic; // Nhạc nền của trò chơi
    // Bạn có thể thêm nhiều clip khác ở đây (ví dụ: coinSound, hitSound...)
    [Header("Item")]
    public AudioClip coinPickupSound;
    public AudioClip coinDropSound;
    public AudioClip coinBounceSound;
    
    [Header("UI")]
    public AudioClip buttonSound;
    public AudioClip openInventorySound;

    private void Awake()
    {
        // Kiểm tra và thiết lập Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ AudioManager không bị xóa khi chuyển Scene
        }
        else
        {
            Destroy(gameObject); // Xóa bản trùng lặp nếu có
        }
    }


    private void Start()
    {
        // Phát nhạc nền khi bắt đầu trò chơi
        if (backgroundMusic != null)
            PlayMusic(backgroundMusic);
    }

    // Hàm dùng để phát nhạc nền
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Hàm dùng để phát âm thanh hiệu ứng (bước chân, đánh, ăn vàng...)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // Hàm riêng cho tiếng bước chân để có thể dừng lại khi nhân vật đứng yên
    public void PlayFootstep(bool isMoving)
    {
        if (isMoving)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.clip = footstepSound;
                footstepSource.loop = true;
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
            {
                footstepSource.Stop();
            }
        }
    }

    public void ChangeMusic(AudioClip clip)
    {
        if (clip == null)
            return;
        if (musicSource.clip == clip)
            return;

        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }
}