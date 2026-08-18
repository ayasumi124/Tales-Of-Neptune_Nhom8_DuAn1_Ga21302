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

    // =====================================================
    // SKILL 1 - ICE SPIKE
    // =====================================================

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

    [Header("Skill 1 Audio")]
    [SerializeField]
    private AudioClip iceSpikeSound;

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

        if (skill.elementType !=
            ElementType.Ice)
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
                $"{skill.skillName}: " +
                $"Skill Index {skill.skillIndex} " +
                "không hợp lệ."
            );

            return;
        }

        // =================================================
        // COOLDOWN CHECK
        // =================================================

        if (cooldownTimers[index] > 0f)
        {
            Debug.Log(
                $"{skill.skillName} còn cooldown " +
                $"{cooldownTimers[index]:F1}s."
            );

            return;
        }

        // =================================================
        // MANA
        // =================================================

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

        /*
         * Kiểm tra prefab TRƯỚC khi trừ mana.
         * Như vậy nếu quên gán prefab
         * sẽ không bị mất mana oan.
         */
        if (skill.skillIndex == 1 &&
            iceSpikePrefab == null)
        {
            Debug.LogError(
                "IceSkillController chưa gán " +
                "Ice Spike Prefab."
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

        // =================================================
        // CAST SKILL
        // =================================================

        bool castSuccess = false;

        switch (skill.skillIndex)
        {
            case 1:

                castSuccess =
                    CastIceSpike();

                break;


            case 2:

                Debug.Log(
                    "Ice Hammer chưa làm."
                );

                break;


            case 3:

                Debug.Log(
                    "Ice Blizzard chưa làm."
                );

                break;


            case 4:

                Debug.Log(
                    "Frost Nova chưa làm."
                );

                break;
        }

        if (!castSuccess)
        {
            /*
             * Skill chưa cast được thì
             * hoàn mana lại.
             */
            mana.RestoreMana(
                skill.manaCost
            );

            return;
        }

        // =================================================
        // START COOLDOWN
        // =================================================

        cooldownTimers[index] =
            Mathf.Max(
                0f,
                skill.cooldown
            );

        // =================================================
        // START DURATION
        // =================================================

        durationTimers[index] =
            Mathf.Max(
                0f,
                skill.duration
            );

        maxDurationTimers[index] =
            durationTimers[index];

        Debug.Log(
            $"ICE CAST: {skill.skillName}"
        );
    }

    // =====================================================
    // SKILL 1 - ICE SPIKE
    // =====================================================

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

        PlayIceSpikeSound();

        Debug.Log(
            $"Spawn Ice Spike tại " +
            $"{spawnPosition}, " +
            $"hướng {direction}"
        );

        return true;
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
    // AUDIO
    // =====================================================

    private void PlayIceSpikeSound()
    {
        if (iceSpikeSound == null)
            return;

        if (AudioManager.Instance == null)
            return;

        AudioManager.Instance
            .PlayElementSkillSFX(
                iceSpikeSound
            );
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