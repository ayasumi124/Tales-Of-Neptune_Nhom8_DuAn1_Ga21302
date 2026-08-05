using System.Collections;
using UnityEngine;

public class EnermyAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource footstepSource;

    [SerializeField]
    private AudioSource voiceSource;

    [SerializeField]
    private AudioSource impactSource;

    [Header("Footstep")]
    [SerializeField]
    private AudioClip footstepClip;

    [Range(0f, 2f)]
    [SerializeField]
    private float footstepVolume = 1f;

    [Header("Attack")]
    [Tooltip("Tiếng gầm hoặc tiếng chuẩn bị tấn công.")]
    [SerializeField]
    private AudioClip attackVoiceClip;

    [Tooltip("Tiếng vung rìu, kiếm hoặc vũ khí.")]
    [SerializeField]
    private AudioClip attackSwingClip;

    [Tooltip("Tiếng đòn đánh va chạm mục tiêu.")]
    [SerializeField]
    private AudioClip attackImpactClip;

    [Header("Hurt")]
    [Tooltip("Tiếng la khi nhận damage.")]
    [SerializeField]
    private AudioClip hurtVoiceClip;

    [Tooltip("Tiếng va chạm vật lý khi nhận damage.")]
    [SerializeField]
    private AudioClip hurtImpactClip;

    [Tooltip(
        "Độ trễ giữa tiếng va chạm và tiếng la khi Hurt."
    )]
    [Min(0f)]
    [SerializeField]
    private float hurtVoiceDelay = 0.05f;

    [Header("Death")]
    [Tooltip("Tiếng la khi chết.")]
    [SerializeField]
    private AudioClip deathVoiceClip;

    [Tooltip("Tiếng cơ thể hoặc giáp va xuống đất.")]
    [SerializeField]
    private AudioClip deathImpactClip;

    [Header("Volume")]
    [Range(0f, 2f)]
    [SerializeField]
    private float voiceVolume = 1f;

    [Range(0f, 2f)]
    [SerializeField]
    private float impactVolume = 1f;

    [Header("Event Protection")]
    [Tooltip(
        "Chống Animation Event Attack Voice " +
        "bị gọi liên tục trong thời gian quá ngắn."
    )]
    [Min(0f)]
    [SerializeField]
    private float attackVoiceMinimumInterval = 0.15f;

    [Tooltip(
        "Chống Animation Event Attack Swing " +
        "bị gọi liên tục trong thời gian quá ngắn."
    )]
    [Min(0f)]
    [SerializeField]
    private float attackSwingMinimumInterval = 0.1f;

    [Tooltip(
        "Chống âm thanh Hurt bị spam liên tục."
    )]
    [Min(0f)]
    [SerializeField]
    private float hurtMinimumInterval = 0.05f;

    private float nextAttackVoiceTime;
    private float nextAttackSwingTime;
    private float nextHurtTime;

    private Coroutine hurtAudioCoroutine;

    private void Awake()
    {
        CacheOrCreateAudioSources();
    }

    private void CacheOrCreateAudioSources()
    {
        AudioSource[] sources =
            GetComponents<AudioSource>();

        if (footstepSource == null)
        {
            if (sources.Length > 0)
            {
                footstepSource = sources[0];
            }
            else
            {
                footstepSource =
                    gameObject.AddComponent<AudioSource>();
            }
        }

        if (voiceSource == null)
        {
            if (sources.Length > 1 &&
                sources[1] != footstepSource)
            {
                voiceSource = sources[1];
            }
            else
            {
                voiceSource =
                    gameObject.AddComponent<AudioSource>();
            }
        }

        if (impactSource == null)
        {
            if (sources.Length > 2 &&
                sources[2] != footstepSource &&
                sources[2] != voiceSource)
            {
                impactSource = sources[2];
            }
            else
            {
                impactSource =
                    gameObject.AddComponent<AudioSource>();
            }
        }

        ConfigureSource(footstepSource);
        ConfigureSource(voiceSource);
        ConfigureSource(impactSource);
    }

    private void ConfigureSource(
        AudioSource source)
    {
        if (source == null)
            return;

        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }

    // =====================================================
    // FOOTSTEP
    // =====================================================

    public void PlayFootstep(
        bool moving)
    {
        if (footstepSource == null)
            return;

        if (!moving)
        {
            StopFootstep();
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
        footstepSource.volume =
            footstepVolume;

        if (!footstepSource.isPlaying)
        {
            footstepSource.Play();
        }
    }

    public void StopFootstep()
    {
        if (footstepSource == null)
            return;

        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    // =====================================================
    // ATTACK
    // =====================================================

    /*
     * Animation Event:
     * đặt tại frame boss gầm hoặc chuẩn bị đánh.
     */
    public void PlayAttackVoice()
    {
        if (Time.time <
            nextAttackVoiceTime)
        {
            return;
        }

        nextAttackVoiceTime =
            Time.time +
            Mathf.Max(
                0f,
                attackVoiceMinimumInterval
            );

        PlayVoice(
            attackVoiceClip
        );
    }

    /*
     * Animation Event:
     * đặt tại frame vũ khí bắt đầu vung.
     */
    public void PlayAttackSwing()
    {
        if (Time.time <
            nextAttackSwingTime)
        {
            return;
        }

        nextAttackSwingTime =
            Time.time +
            Mathf.Max(
                0f,
                attackSwingMinimumInterval
            );

        PlayImpact(
            attackSwingClip
        );
    }

    /*
     * Chỉ gọi khi đòn đánh thật sự
     * gây damage lên Player hoặc Clone.
     */
    public void PlayAttackImpact()
    {
        PlayImpact(
            attackImpactClip
        );
    }

    /*
     * Hàm tương thích cho Slime, Skeleton,
     * Mushroom và các enemy cũ.
     *
     * Enemy cũ gọi PlayAttack() sẽ chỉ phát
     * tiếng Swing, không tự phát thêm Voice.
     */
    public void PlayAttack()
    {
        PlayAttackSwing();
    }

    // =====================================================
    // HURT
    // =====================================================

    public void PlayHurtVoice()
    {
        PlayVoice(
            hurtVoiceClip
        );
    }

    public void PlayHurtImpact()
    {
        PlayImpact(
            hurtImpactClip
        );
    }

    /*
     * Khi nhận damage:
     * 1. Phát tiếng va chạm ngay.
     * 2. Sau một khoảng rất ngắn mới phát tiếng la.
     */
    // Mặc định dành cho skill nguyên tố:
    // chỉ phát tiếng enemy la.
    public void PlayHurt()
    {
        PlayHurtVoice();
    }

    // Dành riêng cho đòn đánh vật lý của Player:
    // tiếng va chạm + tiếng enemy la.
    public void PlayPhysicalHurt()
    {
        if (Time.time < nextHurtTime)
            return;

        nextHurtTime =
            Time.time +
            Mathf.Max(
                0f,
                hurtMinimumInterval
            );

        if (hurtAudioCoroutine != null)
        {
            StopCoroutine(hurtAudioCoroutine);
        }

        hurtAudioCoroutine =
            StartCoroutine(
                PhysicalHurtAudioRoutine()
            );
    }

    private IEnumerator PhysicalHurtAudioRoutine()
    {
        PlayHurtImpact();

        if (hurtVoiceDelay > 0f)
        {
            yield return new WaitForSeconds(
                hurtVoiceDelay
            );
        }

        PlayHurtVoice();

        hurtAudioCoroutine = null;
    }


    // =====================================================
    // DEATH
    // =====================================================

    public void PlayDeathVoice()
    {
        PlayVoice(
            deathVoiceClip
        );
    }

    /*
     * Animation Event có thể đặt ở frame
     * xác boss hoặc enemy chạm đất.
     */
    public void PlayDeathImpact()
    {
        PlayImpact(
            deathImpactClip
        );
    }

    public void PlayDeath()
    {
        PlayDeathVoice();
    }

    // =====================================================
    // COMMON
    // =====================================================

    private void PlayVoice(
        AudioClip clip)
    {
        if (!gameObject.activeInHierarchy ||
            voiceSource == null ||
            clip == null)
        {
            return;
        }

        voiceSource.PlayOneShot(
            clip,
            voiceVolume
        );
    }

    private void PlayImpact(
        AudioClip clip)
    {
        if (!gameObject.activeInHierarchy ||
            impactSource == null ||
            clip == null)
        {
            return;
        }

        impactSource.PlayOneShot(
            clip,
            impactVolume
        );
    }

    public void StopAudio()
    {
        if (hurtAudioCoroutine != null)
        {
            StopCoroutine(
                hurtAudioCoroutine
            );

            hurtAudioCoroutine = null;
        }

        if (footstepSource != null)
        {
            footstepSource.Stop();
            footstepSource.clip = null;
        }

        if (voiceSource != null)
        {
            voiceSource.Stop();
        }

        if (impactSource != null)
        {
            impactSource.Stop();
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

    private void OnValidate()
    {
        footstepVolume =
            Mathf.Clamp(
                footstepVolume,
                0f,
                2f
            );

        voiceVolume =
            Mathf.Clamp(
                voiceVolume,
                0f,
                2f
            );

        impactVolume =
            Mathf.Clamp(
                impactVolume,
                0f,
                2f
            );

        hurtVoiceDelay =
            Mathf.Max(
                0f,
                hurtVoiceDelay
            );

        attackVoiceMinimumInterval =
            Mathf.Max(
                0f,
                attackVoiceMinimumInterval
            );

        attackSwingMinimumInterval =
            Mathf.Max(
                0f,
                attackSwingMinimumInterval
            );

        hurtMinimumInterval =
            Mathf.Max(
                0f,
                hurtMinimumInterval
            );
    }
}