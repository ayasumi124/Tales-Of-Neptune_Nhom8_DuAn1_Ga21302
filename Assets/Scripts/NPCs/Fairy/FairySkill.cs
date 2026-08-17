using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FairySkill : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private KeyCode skillKey = KeyCode.X;

    [Header("References")]
    [SerializeField]
    private Fairy fairy;

    [SerializeField]
    private Transform player;

    [SerializeField]
    private Health playerHealth;

    // =====================================================
    // COOLDOWN
    // =====================================================

    [Header("Cooldown")]
    [Min(0f)]
    [SerializeField]
    private float cooldownDuration = 600f;

    // =====================================================
    // CAST
    // =====================================================

    [Header("Cast")]
    [Min(0.1f)]
    [SerializeField]
    private float castDuration = 2f;

    [SerializeField]
    private float orbitRadius = 0.9f;

    [SerializeField]
    private float orbitYOffset = 0.3f;

    [SerializeField]
    private float orbitCount = 2f;

    // =====================================================
    // PARTICLE
    // =====================================================

    [Header("Effects")]

    [Tooltip(
        "Particle rắc bụi khi Fairy bay vòng lúc bấm X."
    )]
    [SerializeField]
    private ParticleSystem castDust;

    [Tooltip(
        "Aura tồn tại quanh Fairy khi Player " +
        "đang giữ Fairy Revive."
    )]
    [SerializeField]
    private ParticleSystem reviveAura;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [SerializeField]
    private AudioClip castSound;

    [SerializeField]
    private AudioClip reviveSound;

    [Range(0f, 2f)]
    [SerializeField]
    private float castVolume = 1f;

    [Range(0f, 2f)]
    [SerializeField]
    private float reviveVolume = 1f;

    // =====================================================
    // UI
    // =====================================================

    [Header("UI - Cooldown")]
    [SerializeField]
    private Image cooldownMask;

    [SerializeField]
    private TextMeshProUGUI cooldownText;

    [Header("UI - Cast Duration")]
    [SerializeField]
    private Image durationMask;

    [Header("UI - Revive Effect")]
    [Tooltip(
        "Icon báo Player đang có Fairy Revive."
    )]
    [SerializeField]
    private GameObject reviveReadyIcon;

    // =====================================================
    // RUNTIME
    // =====================================================

    private float cooldownRemaining;

    private bool isCasting;

    /*
     * true:
     * Player đang giữ 1 Fairy Revive.
     *
     * false:
     * Player không có Fairy Revive.
     */
    private bool hasReviveEffect;

    public bool HasReviveEffect =>
        hasReviveEffect;

    public bool IsCasting =>
        isCasting;

    public float CooldownRemaining =>
        cooldownRemaining;

    /*
     * Chỉ sử dụng Fairy Skill khi:
     *
     * - Không đang cast.
     * - Cooldown đã hết.
     * - Player chưa có Fairy Revive.
     */
    public bool CanUseSkill =>
        !isCasting &&
        cooldownRemaining <= 0f &&
        !hasReviveEffect;

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (fairy == null)
        {
            fairy =
                GetComponent<Fairy>();
        }

        FindPlayer();

        /*
         * Khi bắt đầu game không được
         * tự chạy particle.
         */
        StopCastDust();
        StopReviveAura();

        UpdateUI();
    }

    private void Update()
    {
        UpdateCooldown();

        if (Input.GetKeyDown(skillKey))
        {
            TryUseSkill();
        }
    }

    // =====================================================
    // USE SKILL
    // =====================================================

    public void TryUseSkill()
    {
        FindPlayer();

        if (player == null ||
            playerHealth == null)
        {
            Debug.Log(
                "Fairy Skill không tìm thấy Player."
            );

            return;
        }

        /*
         * Player chết rồi thì không cho
         * chủ động bấm X.
         *
         * Revive khi chết sẽ do Health
         * gọi TryConsumeRevive().
         */
        if (playerHealth.IsDead)
        {
            return;
        }

        if (isCasting)
        {
            return;
        }

        /*
         * Quan trọng:
         *
         * Nếu vẫn còn Fairy Revive thì
         * không được stack thêm revive.
         */
        if (hasReviveEffect)
        {
            Debug.Log(
                "Player vẫn còn Fairy Revive. " +
                "Không thể dùng Fairy Skill."
            );

            return;
        }

        if (cooldownRemaining > 0f)
        {
            Debug.Log(
                $"Fairy Skill đang hồi: " +
                $"{cooldownRemaining:F1}s"
            );

            return;
        }

        StartCoroutine(
            CastRoutine()
        );
    }

    // =====================================================
    // CAST
    // =====================================================

    private IEnumerator CastRoutine()
    {
        isCasting = true;

        /*
         * Cooldown bắt đầu ngay
         * khi bấm X.
         */
        cooldownRemaining =
            cooldownDuration;

        /*
         * Tạm ngừng script follow để
         * Fairy có thể tự bay vòng.
         */
        if (fairy != null)
        {
            fairy.SetFollowEnabled(
                false
            );
        }

        // -------------------------
        // CAST PARTICLE
        // -------------------------

        PlayCastDust();

        // -------------------------
        // CAST SOUND
        // -------------------------

        if (AudioManager.Instance != null &&
            castSound != null)
        {
            AudioManager.Instance.PlaySFX(
                castSound,
                castVolume
            );
        }

        // -------------------------
        // ORBIT
        // -------------------------

        float timer = 0f;

        float safeDuration =
            Mathf.Max(
                0.1f,
                castDuration
            );

        while (timer < safeDuration)
        {
            timer +=
                Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer /
                    safeDuration
                );

            float angle =
                progress *
                Mathf.PI *
                2f *
                orbitCount;

            Vector3 orbit =
                new Vector3(
                    Mathf.Cos(angle) *
                    orbitRadius,

                    Mathf.Sin(angle) *
                    orbitRadius *
                    0.55f,

                    0f
                );

            if (player != null)
            {
                transform.position =
                    player.position +
                    orbit +
                    Vector3.up *
                    orbitYOffset;
            }

            /*
             * Duration mask giảm
             * từ 1 -> 0.
             */
            if (durationMask != null)
            {
                durationMask.fillAmount =
                    1f - progress;
            }

            yield return null;
        }

        // =================================================
        // CAST HOÀN THÀNH
        // =================================================

        /*
         * Dừng bụi đang rắc.
         */
        StopCastDust();

        /*
         * 1. Hồi FULL HP.
         */
        if (playerHealth != null &&
            !playerHealth.IsDead)
        {
            playerHealth.Heal(
                999999f
            );
        }

        /*
         * 2. Cấp đúng 1 Fairy Revive.
         */
        hasReviveEffect = true;

        /*
         * 3. Bật Aura để báo Player
         * đang giữ mạng Fairy Revive.
         *
         * Aura này KHÔNG phụ thuộc
         * cooldown.
         */
        StartReviveAura();

        if (durationMask != null)
        {
            durationMask.fillAmount = 0f;
        }

        /*
         * Cho Fairy follow Player
         * trở lại.
         */
        if (fairy != null)
        {
            fairy.SetFollowEnabled(
                true
            );
        }

        isCasting = false;

        Debug.Log(
            "Fairy Skill hoàn thành: " +
            "Full HP + Fairy Revive x1."
        );

        UpdateUI();
    }

    // =====================================================
    // REVIVE
    // =====================================================

    /*
     * Health.cs gọi hàm này khi
     * HP Player xuống 0.
     *
     * return true:
     * Fairy đã cứu Player.
     *
     * return false:
     * Không có Fairy Revive.
     */
    public bool TryConsumeRevive()
    {
        if (!hasReviveEffect)
        {
            return false;
        }

        FindPlayer();

        if (playerHealth == null)
        {
            return false;
        }

        /*
         * Consume trước để chắc chắn
         * không revive nhiều lần.
         */
        hasReviveEffect = false;

        /*
         * QUAN TRỌNG:
         *
         * Player vừa dùng mất Fairy Revive
         * -> aura phải biến mất ngay.
         */
        StopReviveAura();

        /*
         * Hồi sinh Player FULL HP.
         */
        playerHealth.ReviveFull();

        /*
         * Đây là âm thanh lúc Player
         * THỰC SỰ được hồi sinh.
         */
        if (AudioManager.Instance != null &&
            reviveSound != null)
        {
            AudioManager.Instance.PlaySFX(
                reviveSound,
                reviveVolume
            );
        }

        Debug.Log(
            "Fairy Revive đã kích hoạt. " +
            "Player hồi sinh FULL HP. " +
            "Revive Aura đã tắt."
        );

        UpdateUI();

        return true;
    }

    // =====================================================
    // CAST PARTICLE
    // =====================================================

    private void PlayCastDust()
    {
        if (castDust == null)
        {
            return;
        }

        /*
         * Clear particle cũ trước.
         */
        castDust.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );

        castDust.Play();
    }

    private void StopCastDust()
    {
        if (castDust == null)
        {
            return;
        }

        castDust.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );
    }

    // =====================================================
    // REVIVE AURA
    // =====================================================

    private void StartReviveAura()
    {
        if (reviveAura == null)
        {
            return;
        }

        /*
         * Clear trước để tránh particle
         * cũ bị chồng lên nhau.
         */
        reviveAura.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );

        reviveAura.Play();
    }

    private void StopReviveAura()
    {
        if (reviveAura == null)
        {
            return;
        }

        /*
         * StopEmittingAndClear:
         *
         * Không chỉ ngừng spawn particle
         * mà xóa luôn particle đang hiện.
         *
         * Vì vậy khi Player revive,
         * aura biến mất NGAY.
         */
        reviveAura.Stop(
            true,
            ParticleSystemStopBehavior
                .StopEmittingAndClear
        );
    }

    // =====================================================
    // COOLDOWN
    // =====================================================

    private void UpdateCooldown()
    {
        if (cooldownRemaining <= 0f)
        {
            cooldownRemaining = 0f;

            UpdateUI();

            return;
        }

        cooldownRemaining -=
            Time.deltaTime;

        cooldownRemaining =
            Mathf.Max(
                0f,
                cooldownRemaining
            );

        UpdateUI();
    }

    // =====================================================
    // UI
    // =====================================================

    private void UpdateUI()
    {
        // -------------------------
        // COOLDOWN MASK
        // -------------------------

        if (cooldownMask != null)
        {
            if (cooldownDuration <= 0f)
            {
                cooldownMask.fillAmount =
                    0f;
            }
            else
            {
                cooldownMask.fillAmount =
                    cooldownRemaining /
                    cooldownDuration;
            }
        }

        // -------------------------
        // COOLDOWN TEXT
        // -------------------------

        if (cooldownText != null)
        {
            if (cooldownRemaining <= 0f)
            {
                cooldownText.text = "";
            }
            else
            {
                int totalSeconds =
                    Mathf.CeilToInt(
                        cooldownRemaining
                    );

                int minutes =
                    totalSeconds / 60;

                int seconds =
                    totalSeconds % 60;

                cooldownText.text =
                    $"{minutes}:{seconds:00}";
            }
        }

        // -------------------------
        // REVIVE READY ICON
        // -------------------------

        if (reviveReadyIcon != null)
        {
            reviveReadyIcon.SetActive(
                hasReviveEffect
            );
        }
    }

    // =====================================================
    // PLAYER
    // =====================================================

    private void FindPlayer()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.Player == null)
        {
            return;
        }

        player =
            GameManager.Instance.Player
                .transform;

        playerHealth =
            GameManager.Instance.Player
                .GetComponent<Health>();
    }

    // =====================================================
    // DEBUG
    // =====================================================

    [ContextMenu(
        "Reset Fairy Cooldown"
    )]
    public void ResetCooldown()
    {
        /*
         * Chỉ reset cooldown.
         *
         * KHÔNG xóa Fairy Revive.
         * KHÔNG tắt Aura.
         */
        cooldownRemaining = 0f;

        UpdateUI();
    }

    [ContextMenu(
        "Remove Fairy Revive"
    )]
    public void RemoveReviveEffect()
    {
        hasReviveEffect = false;

        StopReviveAura();

        UpdateUI();
    }

    // =====================================================
    // VALIDATE
    // =====================================================

    private void OnValidate()
    {
        cooldownDuration =
            Mathf.Max(
                0f,
                cooldownDuration
            );

        castDuration =
            Mathf.Max(
                0.1f,
                castDuration
            );

        orbitRadius =
            Mathf.Max(
                0f,
                orbitRadius
            );

        orbitCount =
            Mathf.Max(
                0f,
                orbitCount
            );
    }
}