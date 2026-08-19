using UnityEngine;

public class IceSkillController : MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField]
    private PlayerMana mana;

    [SerializeField]
    private Players player;

    [Header("Skill 1 - Ice Spike")]
    [SerializeField]
    private GameObject iceSpikePrefab;

    [Tooltip(
        "Khoảng cách Ice Spike xuất hiện " +
        "tính từ tâm Player."
    )]
    [Min(0f)]
    [SerializeField]
    private float iceSpikeSpawnDistance = 1.1f;

    [Header("Skill 2 - Ice Hammer")]
    [SerializeField]
    private GameObject iceHammerCompanionPrefab;

    private IceHammerCompanion activeIceHammer;

    [Header("Skill 3 - Blizzard")]
    [SerializeField]
    private GameObject blizzardAuraPrefab;

    [SerializeField]
    private GameObject blizzardProjectilePrefab;

    [SerializeField]
    private float blizzardProjectileSpawnDistance =
        0.8f;

    private BlizzardAura activeBlizzard;

    [Header("Skill 4 - Frost Nova")]
    [SerializeField]
    private GameObject frostNovaPrefab;

    // =====================================================
    // RUNTIME TIMERS
    // =====================================================

    private readonly float[]
        cooldownTimers =
            new float[4];

    private readonly float[]
        durationTimers =
            new float[4];

    private readonly float[]
        maxDurationTimers =
            new float[4];

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        if (mana == null)
        {
            mana =
                GetComponent<PlayerMana>();
        }

        if (player == null)
        {
            player =
                GetComponent<Players>();
        }
    }

    private void Update()
    {
        UpdateCooldowns();

        UpdateDurations();
    }

    // =====================================================
    // TIMERS
    // =====================================================

    private void UpdateCooldowns()
    {
        for (int i = 0;
             i < cooldownTimers.Length;
             i++)
        {
            if (cooldownTimers[i] <= 0f)
                continue;

            cooldownTimers[i] -=
                Time.deltaTime;

            if (cooldownTimers[i] < 0f)
            {
                cooldownTimers[i] = 0f;
            }
        }
    }

    private void UpdateDurations()
    {
        for (int i = 0;
             i < durationTimers.Length;
             i++)
        {
            if (durationTimers[i] <= 0f)
                continue;

            durationTimers[i] -=
                Time.deltaTime;

            if (durationTimers[i] < 0f)
            {
                durationTimers[i] = 0f;
            }
        }
    }

    // =====================================================
    // CAST
    // =====================================================

    public void TryCast(
    ElementSkillData skill)
    {
        if (skill == null)
        {
            Debug.LogError(
                "IceSkillController nhận Skill null."
            );
            return;
        }

        if (skill.elementType != ElementType.Ice)
        {
            Debug.LogWarning(
                $"{skill.skillName} không phải Ice Skill."
            );
            return;
        }

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= 4)
        {
            Debug.LogError(
                $"{skill.skillName}: Skill Index " +
                $"{skill.skillIndex} không hợp lệ."
            );
            return;
        }

        // =========================================
        // SKILL 3 - BLIZZARD RECAST
        // =========================================

        /*
         * Blizzard đang tồn tại:
         * nhấn 3 lần nữa sẽ Release.
         *
         * Không:
         * - check cooldown
         * - trừ mana lần 2
         */
        if (skill.skillIndex == 3 &&
            activeBlizzard != null)
        {
            bool released =
                ReleaseBlizzardProjectile();

            if (!released)
                return;

            // Aura kết thúc.
            durationTimers[index] = 0f;
            maxDurationTimers[index] = 0f;

            StartSkillCooldown(
    skill
);

            /*
             * Không PlaySkillSound ở đây.
             *
             * BlizzardProjectile.Initialize()
             * sẽ tự phát releaseSound.
             */

            Debug.Log(
                "BLIZZARD RELEASE!"
            );
            return;
        }

        // =========================================
        // NORMAL COOLDOWN CHECK
        // =========================================

        if (cooldownTimers[index] > 0f)
        {
            Debug.Log(
                $"{skill.skillName} còn cooldown " +
                $"{cooldownTimers[index]:F1}s."
            );

            return;
        }

        // =========================================
        // MANA
        // =========================================

        if (mana == null)
        {
            mana =
                GetComponent<PlayerMana>();
        }

        if (mana == null)
        {
            Debug.LogError(
                "Player thiếu PlayerMana."
            );
            return;
        }

        // Kiểm tra prefab trước khi mất Mana.

        if (skill.skillIndex == 1 &&
            iceSpikePrefab == null)
        {
            Debug.LogError(
                "Chưa gán Ice Spike Prefab."
            );
            return;
        }

        if (skill.skillIndex == 2 &&
            iceHammerCompanionPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Ice Hammer Companion Prefab."
            );
            return;
        }

        if (skill.skillIndex == 3 &&
            blizzardAuraPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Blizzard Aura Prefab."
            );
            return;
        }

        if (skill.skillIndex == 4 &&
    frostNovaPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Frost Nova Prefab."
            );

            return;
        }

        if (!mana.UseMana(
                skill.manaCost))
        {
            if (ManaUI.Instance != null)
            {
                ManaUI.Instance
                    .ShowNoMana();
            }

            return;
        }

        // =========================================
        // CAST
        // =========================================

        bool castSuccess = false;

        switch (skill.skillIndex)
        {
            case 1:

                castSuccess =
                    CastIceSpike();

                break;


            case 2:

                castSuccess =
                    CastIceHammer(
                        skill
                    );

                break;


            case 3:

                castSuccess =
                    CastBlizzard(
                        skill
                    );

                break;


            case 4:

                castSuccess =
                    CastFrostNova();

                break;
        }

        if (!castSuccess)
        {
            mana.RestoreMana(
                skill.manaCost
            );

            return;
        }
        // Skill 3:
        // âm thanh Aura sẽ phát ở lần cast đầu.
        // Projectile có Release Sound riêng trong prefab.
        //
        // Skill 1, 2:
        // phát Cast Sound bình thường.
        if (skill.skillIndex != 3 &&
    skill.skillIndex != 4)
        {
            PlaySkillSound(
                skill
            );
        }

        // =========================================
        // BLIZZARD LẦN 1
        // =========================================

        if (skill.skillIndex == 3)
        {
            /*
             * Đây là âm thanh khi TRIỆU HỒI
             * Blizzard Aura.
             *
             * Projectile lần 2 đã có
             * releaseSound riêng.
             */
            PlaySkillSound(
                skill
            );

            StartSkillDuration(
                skill
            );

            Debug.Log(
                "BLIZZARD AURA START!"
            );

            return;
        }

        // =========================================
        // SKILL 1 / 2 / 4
        // =========================================

        StartSkillCooldown(
            skill
        );

        StartSkillDuration(
            skill
        );

        Debug.Log(
            $"ICE CAST: {skill.skillName}"
        );
    }

    private bool CastFrostNova()
    {
        if (frostNovaPrefab == null)
        {
            Debug.LogError(
                "Frost Nova Prefab đang null."
            );

            return false;
        }

        GameObject novaObject =
            Instantiate(
                frostNovaPrefab,
                transform.position,
                Quaternion.identity
            );

        if (novaObject == null)
        {
            Debug.LogError(
                "Instantiate Frost Nova thất bại."
            );

            return false;
        }

        FrostNova nova =
            novaObject.GetComponent<
                FrostNova
            >();

        if (nova == null)
        {
            Debug.LogError(
                "Frost Nova Prefab thiếu " +
                "FrostNova.cs."
            );

            Destroy(
                novaObject
            );

            return false;
        }

        nova.Initialize(
            gameObject
        );

        return true;
    }

    private void StartSkillCooldown(
        ElementSkillData skill)
    {
        if (skill == null)
            return;

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= cooldownTimers.Length)
        {
            return;
        }

        cooldownTimers[index] =
            Mathf.Max(
                0f,
                skill.cooldown
            );
    }

    private void StartSkillDuration(
        ElementSkillData skill)
    {
        if (skill == null)
            return;

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= durationTimers.Length)
        {
            return;
        }

        durationTimers[index] =
            Mathf.Max(
                0f,
                skill.duration
            );

        maxDurationTimers[index] =
            durationTimers[index];
    }

    private void PlaySkillSound(
    ElementSkillData skill)
    {
        if (skill == null ||
            skill.castSound == null)
        {
            return;
        }

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy AudioManager."
            );

            return;
        }

        AudioManager.Instance
            .PlayElementSkillSFX(
                skill.castSound,
                skill.castVolume
            );
    }

    private bool CastIceSpike()
    {
        if (iceSpikePrefab == null)
        {
            Debug.LogError(
                "Ice Spike Prefab đang null."
            );

            return false;
        }

        Vector2 direction =
            GetCastDirection();

        Vector3 spawnPosition =
            transform.position +
            (Vector3)(
                direction *
                iceSpikeSpawnDistance
            );

        GameObject spawnedSpike =
            Instantiate(
                iceSpikePrefab,
                spawnPosition,
                Quaternion.identity
            );

        if (spawnedSpike == null)
        {
            Debug.LogError(
                "Instantiate Ice Spike thất bại."
            );

            return false;
        }

        IceSpike iceSpike =
            spawnedSpike.GetComponent<
                IceSpike
            >();

        if (iceSpike == null)
        {
            Debug.LogError(
                "Ice Spike Prefab thiếu " +
                "component IceSpike.cs."
            );

            Destroy(
                spawnedSpike
            );

            return false;
        }

        iceSpike.Initialize(
            direction,
            gameObject
        );


        Debug.Log(
            $"Spawn Ice Spike tại " +
            $"{spawnPosition}, " +
            $"hướng {direction}"
        );

        return true;
    }

    private bool CastIceHammer(
    ElementSkillData skill)
    {
        if (iceHammerCompanionPrefab == null)
        {
            Debug.LogError(
                "Chưa gán Ice Hammer Companion Prefab."
            );

            return false;
        }

        /*
         * Không cho summon thêm Hammer
         * nếu Hammer cũ còn tồn tại.
         */
        if (activeIceHammer != null)
        {
            Debug.Log(
                "Ice Hammer vẫn đang được triệu hồi."
            );

            return false;
        }

        GameObject hammerObject =
            Instantiate(
                iceHammerCompanionPrefab,
                transform.position,
                Quaternion.identity
            );

        activeIceHammer =
            hammerObject.GetComponent<
                IceHammerCompanion
            >();

        if (activeIceHammer == null)
        {
            Debug.LogError(
                "Prefab thiếu IceHammerCompanion.cs."
            );

            Destroy(
                hammerObject
            );

            return false;
        }

        activeIceHammer.Initialize(
            gameObject
        );

        /*
         * Duration của ElementSkillData
         * quyết định Hammer tồn tại bao lâu.
         */
        StartCoroutine(
            IceHammerDurationRoutine(
                skill.duration
            )
        );

        return true;
    }

    private System.Collections.IEnumerator
    IceHammerDurationRoutine(
        float duration)
    {
        float safeDuration =
            Mathf.Max(
                0.1f,
                duration
            );

        yield return new WaitForSeconds(
            safeDuration
        );

        if (activeIceHammer != null)
        {
            Destroy(
                activeIceHammer.gameObject
            );

            activeIceHammer = null;
        }
    }

    private bool CastBlizzard(
    ElementSkillData skill)
    {
        /*
         * LẦN 1:
         * chưa có Blizzard
         * → tạo Aura.
         */
        if (activeBlizzard == null)
        {
            if (blizzardAuraPrefab == null)
            {
                Debug.LogError(
                    "Chưa gán Blizzard Aura Prefab."
                );

                return false;
            }

            GameObject auraObject =
                Instantiate(
                    blizzardAuraPrefab,
                    transform.position,
                    Quaternion.identity
                );

            activeBlizzard =
                auraObject.GetComponent<
                    BlizzardAura
                >();

            if (activeBlizzard == null)
            {
                Debug.LogError(
                    "Blizzard Aura Prefab thiếu BlizzardAura.cs."
                );

                Destroy(auraObject);
                return false;
            }

            activeBlizzard.Initialize(
                transform
            );

            StartCoroutine(
    BlizzardDurationRoutine(
        skill
    )
);

            return true;
        }

        /*
         * LẦN 2:
         * Blizzard đang hoạt động
         * → chuyển thành projectile.
         */
        return ReleaseBlizzardProjectile();
    }

    private bool ReleaseBlizzardProjectile()
    {
        if (activeBlizzard == null)
            return false;

        if (blizzardProjectilePrefab == null)
        {
            Debug.LogError(
                "Chưa gán Blizzard Projectile Prefab."
            );

            return false;
        }

        Vector2 direction =
            GetCastDirection();

        Vector3 spawnPosition =
            transform.position +
            (Vector3)(
                direction *
                blizzardProjectileSpawnDistance
            );

        GameObject projectileObject =
            Instantiate(
                blizzardProjectilePrefab,
                spawnPosition,
                Quaternion.identity
            );

        BlizzardProjectile projectile =
            projectileObject.GetComponent<
                BlizzardProjectile
            >();

        if (projectile == null)
        {
            Debug.LogError(
                "Blizzard Projectile thiếu BlizzardProjectile.cs."
            );

            Destroy(projectileObject);
            return false;
        }

        projectile.Initialize(
            direction,
            gameObject
        );

        Destroy(
            activeBlizzard.gameObject
        );

        activeBlizzard = null;

        return true;
    }

    private System.Collections.IEnumerator
    BlizzardDurationRoutine(
        ElementSkillData skill)
    {
        if (skill == null)
            yield break;

        float duration =
            Mathf.Max(
                0.1f,
                skill.duration
            );

        yield return new WaitForSeconds(
            duration
        );

        /*
         * Nếu Player chưa Release Blizzard
         * thì Aura tự kết thúc.
         */
        if (activeBlizzard != null)
        {
            Destroy(
                activeBlizzard.gameObject
            );

            activeBlizzard = null;

            int index =
                skill.skillIndex - 1;

            if (index >= 0 &&
                index < durationTimers.Length)
            {
                durationTimers[index] = 0f;
                maxDurationTimers[index] = 0f;
            }

            StartSkillCooldown(
                skill
            );

            Debug.Log(
                "Blizzard Aura hết thời gian → Cooldown."
            );
        }
    }

    // =====================================================
    // DIRECTION
    // =====================================================

    private Vector2 GetCastDirection()
    {
        if (player == null)
        {
            player =
                GetComponent<Players>();
        }

        if (player == null)
        {
            return Vector2.down;
        }

        Vector2 direction =
            player.LastDirection;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return Vector2.down;
        }

        return GetCardinalDirection(
            direction
        );
    }

    private Vector2 GetCardinalDirection(
        Vector2 direction)
    {
        /*
         * Vì game của bạn dùng 4 hướng,
         * Ice Spike cũng khóa về 4 hướng.
         */

        if (Mathf.Abs(direction.x) >
            Mathf.Abs(direction.y))
        {
            return direction.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }

        return direction.y >= 0f
            ? Vector2.up
            : Vector2.down;
    }

    // =====================================================
    // UI - COOLDOWN
    // =====================================================

    public float GetCooldownNormalized(
        ElementSkillData skill)
    {
        if (skill == null ||
            skill.cooldown <= 0f)
        {
            return 0f;
        }

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= 4)
        {
            return 0f;
        }

        return Mathf.Clamp01(
            cooldownTimers[index] /
            skill.cooldown
        );
    }

    public float GetRemainingCooldown(
        ElementSkillData skill)
    {
        if (skill == null)
            return 0f;

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= 4)
        {
            return 0f;
        }

        return cooldownTimers[index];
    }

    // =====================================================
    // UI - DURATION
    // =====================================================

    public float GetDurationNormalized(
        ElementSkillData skill)
    {
        if (skill == null)
            return 0f;

        int index =
            skill.skillIndex - 1;

        if (index < 0 ||
            index >= 4)
        {
            return 0f;
        }

        float maxDuration =
            maxDurationTimers[index];

        if (maxDuration <= 0f)
            return 0f;

        return Mathf.Clamp01(
            durationTimers[index] /
            maxDuration
        );
    }
}