using System.Collections;
using UnityEngine;

public class FireSkillController : MonoBehaviour
{
    [Header("Fire Element Data")]
    [SerializeField] private ElementData fireData;

    [Header("Spawn Points")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Điểm bắt đầu của Fire Breath.")]
    [SerializeField] private Transform breathPoint;

    [Tooltip("Khoảng cách BreathPoint so với tâm Player.")]
    [SerializeField] private float breathPointDistance = 0.5f;

    [Tooltip("Bù vị trí theo world space để BreathPoint nằm ở phần thân trên.")]
    [SerializeField]
    private Vector2 breathCenterOffset =
        new Vector2(0f, 0.25f);

    [Header("Prefabs")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject fireMeteorPrefab;
    [SerializeField] private GameObject fireBreathPrefab;
    [SerializeField] private GameObject fireTornadoPrefab;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float castLockTime = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip fireBallSound;
    [SerializeField] private AudioClip fireMeteorCastSound;
    [SerializeField] private AudioClip fireBreathSound;
    [SerializeField] private AudioClip fireTornadoSound;

    // =====================================================
    // FIRE SKILL 2 - METEOR RAIN
    // =====================================================

    [Header("Fire Skill 2 - Meteor Rain")]

    [Range(1, 20)]
    [SerializeField] private int meteorCount = 7;

    [Tooltip("Khoảng thời gian giữa từng thiên thạch.")]
    [SerializeField] private float meteorSpawnInterval = 0.1f;

    [Tooltip("Khoảng cách tâm vùng thiên thạch so với Player.")]
    [SerializeField] private float meteorTargetDistance = 3f;

    [Tooltip("Thiên thạch phân tán trong bán kính này.")]
    [SerializeField] private float meteorImpactSpread = 2f;

    [Tooltip("Độ cao xuất hiện của thiên thạch.")]
    [SerializeField] private float meteorSpawnHeight = 6f;

    [Tooltip("Độ lệch ngang tối thiểu để thiên thạch rơi chéo.")]
    [SerializeField] private float meteorHorizontalOffsetMin = 2.5f;

    [Tooltip("Độ lệch ngang tối đa để thiên thạch rơi chéo.")]
    [SerializeField] private float meteorHorizontalOffsetMax = 4.5f;

    [Tooltip("Hệ số kích thước nhỏ nhất.")]
    [SerializeField] private float meteorScaleMin = 1.1f;

    [Tooltip("Hệ số kích thước lớn nhất.")]
    [SerializeField] private float meteorScaleMax = 1.6f;

    [Tooltip("Viên cuối cùng lớn hơn để tạo cảm giác kết thúc.")]
    [SerializeField] private bool makeLastMeteorBigger = true;

    [Tooltip("Hệ số phóng to viên thiên thạch cuối.")]
    [SerializeField] private float lastMeteorScaleMultiplier = 1.35f;

    // =====================================================
    // FIRE SKILL 3 - CONTINUOUS BREATH
    // =====================================================

    [Header("Fire Skill 3 - Continuous Breath")]

    [Tooltip("Khoảng nghỉ sau mỗi đợt phun.")]
    [SerializeField] private float breathSpawnInterval = 0.08f;

    [Tooltip("Số lớp lửa trong mỗi đợt.")]
    [SerializeField] private int breathSegmentCount = 5;

    [Tooltip("Khoảng cách giữa các lớp lửa.")]
    [SerializeField] private float breathSegmentSpacing = 0.28f;

    [Tooltip("Độ trễ giữa từng lớp lửa.")]
    [SerializeField] private float breathLayerDelay = 0.035f;

    [Tooltip("Phím giữ để duy trì Fire Breath.")]
    [SerializeField]
    private KeyCode fireBreathKey =
        KeyCode.Alpha3;

    [SerializeField]
    private float breathHorizontalDistance = 0.55f;

    [SerializeField]
    private float breathVerticalDistance = 0.75f;
    // =====================================================
    // FIRE SKILL 4 - TORNADO
    // =====================================================

    [Header("Fire Skill 4 - Tornado")]
    [SerializeField] private float tornadoSpawnDistance = 2f;

    // =====================================================
    // REFERENCES AND STATE
    // =====================================================

    private PlayerMana mana;
    private Players player;
    private Attack attack;
    private PlayerDash dash;

    private bool isCasting;
    private bool isBreathing;
    private bool breathActuallyCast;
    private bool breathSoundPlayed;

    private Coroutine normalCastCoroutine;
    private Coroutine meteorRainCoroutine;
    private Coroutine breathCoroutine;

    private ElementSkillData activeBreathSkill;

    private readonly float[] cooldownTimers =
        new float[4];

    private readonly float[] durationTimers =
        new float[4];

    private readonly float[] maxDurationTimers =
        new float[4];

    public bool IsCasting => isCasting;
    public bool IsBreathing => isBreathing;

    // =====================================================
    // UNITY METHODS
    // =====================================================

    private void Awake()
    {
        mana = GetComponent<PlayerMana>();
        player = GetComponent<Players>();
        attack = GetComponent<Attack>();
        dash = GetComponent<PlayerDash>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Start()
    {
        UpdateBreathPoint();
    }

    private void Update()
    {
        UpdateCooldowns();
        UpdateDurations();

        if (isBreathing &&
            !Input.GetKey(fireBreathKey))
        {
            StopFireBreath();
        }
    }

    private void LateUpdate()
    {
        UpdateBreathPoint();
    }

    // =====================================================
    // BREATH POINT
    // =====================================================

    private void UpdateBreathPoint()
    {
        if (breathPoint == null)
            return;

        Vector2 direction =
            GetCardinalDirection(
                GetCastDirection()
            );

        float distance =
            Mathf.Abs(direction.x) > 0f
                ? breathHorizontalDistance
                : breathVerticalDistance;

        breathPoint.position =
            transform.position +
            (Vector3)breathCenterOffset +
            (Vector3)(
                direction * distance
            );

        breathPoint.rotation =
            Quaternion.identity;
    }
    // =====================================================
    // TIMER SYSTEM
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
                cooldownTimers[i] = 0f;
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

            // Skill 3 giữ duration đầy khi đang channel.
            if (i == 2 && isBreathing)
            {
                durationTimers[i] =
                    Mathf.Max(
                        0.01f,
                        maxDurationTimers[i]
                    );

                continue;
            }

            durationTimers[i] -=
                Time.deltaTime;

            if (durationTimers[i] < 0f)
                durationTimers[i] = 0f;
        }
    }

    private void StartSkillDuration(
        ElementSkillData skillData)
    {
        if (skillData == null)
            return;

        int index =
            skillData.skillIndex - 1;

        if (index < 0 ||
            index >= durationTimers.Length)
        {
            return;
        }

        float duration =
            Mathf.Max(
                0f,
                skillData.duration
            );

        durationTimers[index] =
            duration;

        maxDurationTimers[index] =
            duration;
    }

    // =====================================================
    // PUBLIC CAST
    // =====================================================

    public void TryCast(
        ElementSkillData skillData)
    {
        if (skillData == null)
            return;

        if (skillData.elementType !=
            ElementType.Fire)
        {
            return;
        }

        if (skillData.skillIndex == 3)
        {
            StartFireBreath(skillData);
            return;
        }

        if (!CanUseNormalSkill(skillData))
            return;

        int index =
            skillData.skillIndex - 1;

        if (!TrySpendMana(
                skillData.manaCost))
        {
            return;
        }

        cooldownTimers[index] =
            Mathf.Max(
                0f,
                skillData.cooldown
            );

        if (normalCastCoroutine != null)
        {
            StopCoroutine(
                normalCastCoroutine
            );
        }

        normalCastCoroutine =
            StartCoroutine(
                CastRoutine(skillData)
            );
    }

    private bool CanUseNormalSkill(
        ElementSkillData skillData)
    {
        if (!skillData.unlocked)
        {
            Debug.Log(
                $"{skillData.skillName} chưa mở khóa."
            );

            return false;
        }

        if (isCasting || isBreathing)
            return false;

        if (player != null &&
            player.IsControlLocked)
        {
            return false;
        }

        if (attack != null &&
            attack.IsAttacking)
        {
            return false;
        }

        if (dash != null &&
            dash.IsDashing)
        {
            return false;
        }

        int index =
            skillData.skillIndex - 1;

        if (index < 0 ||
            index >= cooldownTimers.Length)
        {
            Debug.LogError(
                $"{skillData.skillName} có " +
                $"Skill Index không hợp lệ: " +
                $"{skillData.skillIndex}"
            );

            return false;
        }

        if (cooldownTimers[index] > 0f)
        {
            Debug.Log(
                $"{skillData.skillName} còn hồi " +
                $"{cooldownTimers[index]:F1} giây."
            );

            return false;
        }

        return true;
    }

    private bool TrySpendMana(
        float amount)
    {
        if (mana == null)
            mana = GetComponent<PlayerMana>();

        if (mana == null)
        {
            Debug.LogError(
                "Player không có PlayerMana."
            );

            return false;
        }

        if (mana.UseMana(amount))
            return true;

        if (ManaUI.Instance != null)
            ManaUI.Instance.ShowNoMana();

        return false;
    }

    private IEnumerator CastRoutine(
        ElementSkillData skillData)
    {
        isCasting = true;

        Vector2 direction =
            GetCastDirection();

        if (player != null)
            player.LockControl();

        DisableCombatActions();
        PlayCastAnimation(direction);

        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                castLockTime * 0.5f
            )
        );

        StartSkillDuration(skillData);

        switch (skillData.skillIndex)
        {
            case 1:
                CastFireBall(direction);
                break;

            case 2:
                CastFireMeteorRain(direction);
                break;

            case 4:
                CastFireTornado(direction);
                break;
        }

        yield return new WaitForSeconds(
            Mathf.Max(
                0f,
                castLockTime * 0.5f
            )
        );

        EndNormalCast();

        normalCastCoroutine = null;
    }

    // =====================================================
    // FIRE SKILL 1 - FIREBALL
    // =====================================================

    private void CastFireBall(
        Vector2 direction)
    {
        if (fireBallPrefab == null)
        {
            Debug.LogError(
                "Fire Ball Prefab chưa được gán."
            );

            return;
        }

        GameObject fireBall =
            Instantiate(
                fireBallPrefab,
                GetSpawnPosition(direction),
                Quaternion.identity
            );

        FireProjectile projectile =
            fireBall.GetComponent<
                FireProjectile
            >();

        if (projectile == null)
        {
            Debug.LogError(
                "Fire Ball Prefab thiếu FireProjectile."
            );

            Destroy(fireBall);
            return;
        }

        projectile.Initialize(
            direction,
            gameObject
        );

        PlaySound(fireBallSound);
    }

    // =====================================================
    // FIRE SKILL 2 - METEOR RAIN
    // =====================================================

    private void CastFireMeteorRain(
        Vector2 direction)
    {
        if (fireMeteorPrefab == null)
        {
            Debug.LogError(
                "Fire Meteor Prefab chưa được gán."
            );

            return;
        }

        if (meteorRainCoroutine != null)
        {
            StopCoroutine(
                meteorRainCoroutine
            );
        }

        meteorRainCoroutine =
            StartCoroutine(
                SpawnMeteorRain(direction)
            );

        PlaySound(fireMeteorCastSound);
    }

    private IEnumerator SpawnMeteorRain(
        Vector2 direction)
    {
        direction =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        Vector3 areaCenter =
            transform.position +
            (Vector3)(
                direction *
                meteorTargetDistance
            );

        int count =
            Mathf.Max(
                1,
                meteorCount
            );

        float minimumHorizontal =
            Mathf.Min(
                meteorHorizontalOffsetMin,
                meteorHorizontalOffsetMax
            );

        float maximumHorizontal =
            Mathf.Max(
                meteorHorizontalOffsetMin,
                meteorHorizontalOffsetMax
            );

        float minimumScale =
            Mathf.Min(
                meteorScaleMin,
                meteorScaleMax
            );

        float maximumScale =
            Mathf.Max(
                meteorScaleMin,
                meteorScaleMax
            );

        for (int i = 0;
             i < count;
             i++)
        {
            Vector2 impactOffset =
                Random.insideUnitCircle *
                Mathf.Max(
                    0f,
                    meteorImpactSpread
                );

            Vector3 impactPosition =
                areaCenter +
                (Vector3)impactOffset;

            // Mỗi meteor rơi từ một phía khác nhau.
            float side =
                Random.value < 0.5f
                    ? -1f
                    : 1f;

            float horizontalOffset =
                Random.Range(
                    minimumHorizontal,
                    maximumHorizontal
                ) * side;

            float heightVariation =
                Random.Range(
                    -0.5f,
                    1f
                );

            Vector3 spawnPosition =
                impactPosition +
                new Vector3(
                    horizontalOffset,
                    meteorSpawnHeight +
                    heightVariation,
                    0f
                );

            GameObject meteor =
                Instantiate(
                    fireMeteorPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            FireMeteor meteorSkill =
                meteor.GetComponent<
                    FireMeteor
                >();

            if (meteorSkill == null)
            {
                Debug.LogError(
                    "Fire Meteor Prefab thiếu FireMeteor."
                );

                Destroy(meteor);
                continue;
            }

            float randomScale =
                Random.Range(
                    minimumScale,
                    maximumScale
                );

            if (makeLastMeteorBigger &&
                i == count - 1)
            {
                randomScale *=
                    Mathf.Max(
                        1f,
                        lastMeteorScaleMultiplier
                    );
            }

            meteorSkill.Initialize(
                spawnPosition,
                impactPosition,
                gameObject,
                randomScale
            );

            if (meteorSpawnInterval > 0f &&
                i < count - 1)
            {
                yield return new WaitForSeconds(
                    meteorSpawnInterval
                );
            }
        }

        meteorRainCoroutine = null;
    }

    // =====================================================
    // FIRE SKILL 3 - FIRE BREATH
    // =====================================================

    public void StartFireBreath(
    ElementSkillData skillData)
    {
        if (skillData == null ||
            skillData.elementType != ElementType.Fire ||
            skillData.skillIndex != 3)
        {
            return;
        }

        if (!skillData.unlocked)
        {
            Debug.Log(
                $"{skillData.skillName} chưa mở khóa."
            );

            return;
        }

        if (isBreathing || isCasting)
            return;

        if (player != null &&
            player.IsControlLocked)
        {
            return;
        }

        if (attack != null &&
            attack.IsAttacking)
        {
            return;
        }

        if (dash != null &&
            dash.IsDashing)
        {
            return;
        }

        int cooldownIndex =
            skillData.skillIndex - 1;

        if (cooldownIndex < 0 ||
            cooldownIndex >= cooldownTimers.Length)
        {
            return;
        }

        if (cooldownTimers[cooldownIndex] > 0f)
        {
            Debug.Log(
                $"{skillData.skillName} còn hồi " +
                $"{cooldownTimers[cooldownIndex]:F1} giây."
            );

            return;
        }

        if (mana == null)
            mana = GetComponent<PlayerMana>();

        if (mana == null)
        {
            Debug.LogError(
                "Không tìm thấy PlayerMana."
            );

            return;
        }

        if (fireBreathPrefab == null)
        {
            Debug.LogError(
                "Fire Breath Prefab chưa được gán."
            );

            return;
        }

        if (breathPoint == null)
        {
            Debug.LogError(
                "Player chưa được gán BreathPoint."
            );

            return;
        }

        /*
         * Tính lượng mana tối thiểu cần cho
         * đợt phun đầu tiên.
         */
        float firstCycleCost =
            CalculateBreathCycleManaCost(
                skillData
            );

        if (mana.currentMana < firstCycleCost)
        {
            if (ManaUI.Instance != null)
                ManaUI.Instance.ShowNoMana();

            Debug.Log(
                $"Không đủ mana để bắt đầu Fire Breath. " +
                $"Cần {firstCycleCost:F1}, " +
                $"hiện có {mana.currentMana:F1}."
            );

            return;
        }

        isBreathing = true;
        isCasting = true;

        breathActuallyCast = false;
        breathSoundPlayed = false;

        activeBreathSkill = skillData;

        DisableCombatActions();

        breathCoroutine =
            StartCoroutine(
                FireBreathRoutine()
            );
    }

    private float CalculateBreathCycleManaCost(
        ElementSkillData skillData)
    {
        if (skillData == null)
            return 0f;

        float layerDuration =
            Mathf.Max(
                0f,
                breathLayerDelay
            ) *
            Mathf.Max(
                1,
                breathSegmentCount
            );

        float cycleDuration =
            layerDuration +
            Mathf.Max(
                0f,
                breathSpawnInterval
            );

        return skillData.manaCost *
               Mathf.Max(
                   0.02f,
                   cycleDuration
               );
    }

    private IEnumerator FireBreathRoutine()
    {
        while (isBreathing &&
               activeBreathSkill != null)
        {
            float manaCostThisCycle =
                CalculateBreathCycleManaCost(
                    activeBreathSkill
                );

            if (!TrySpendMana(
                    manaCostThisCycle))
            {
                isBreathing = false;
                break;
            }

            /*
             * Từ đây skill mới được tính là
             * đã cast thật.
             */
            if (!breathActuallyCast)
            {
                breathActuallyCast = true;

                StartSkillDuration(
                    activeBreathSkill
                );
            }

            if (!breathSoundPlayed)
            {
                breathSoundPlayed = true;

                PlaySound(
                    fireBreathSound
                );
            }

            yield return StartCoroutine(
                SpawnContinuousBreath()
            );

            if (!isBreathing)
                break;

            yield return new WaitForSeconds(
                Mathf.Max(
                    0f,
                    breathSpawnInterval
                )
            );
        }

        FinishFireBreath();

        breathCoroutine = null;
    }

    private IEnumerator SpawnContinuousBreath()
    {
        if (fireBreathPrefab == null ||
            breathPoint == null)
        {
            yield break;
        }

        int count =
            Mathf.Max(
                1,
                breathSegmentCount
            );

        for (int i = 0;
             i < count;
             i++)
        {
            if (!isBreathing)
                yield break;

            Vector2 direction =
                GetCardinalDirection(
                    GetCastDirection()
                );

            UpdateBreathPoint();

            float distanceFromPoint =
                i * breathSegmentSpacing;

            Vector3 spawnPosition =
                breathPoint.position +
                (Vector3)(
                    direction *
                    distanceFromPoint
                );

            GameObject segment =
                Instantiate(
                    fireBreathPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            FireBreathSegment breathSegment =
                segment.GetComponent<
                    FireBreathSegment
                >();

            if (breathSegment == null)
            {
                Debug.LogError(
                    "Fire Breath Prefab thiếu " +
                    "FireBreathSegment."
                );

                Destroy(segment);
            }
            else
            {
                breathSegment.Initialize(
                    direction,
                    gameObject,
                    breathPoint,
                    distanceFromPoint,
                    i
                );
            }

            if (breathLayerDelay > 0f)
            {
                yield return new WaitForSeconds(
                    breathLayerDelay
                );
            }
        }
    }

    public void StopFireBreath()
    {
        if (!isBreathing &&
            activeBreathSkill == null)
        {
            return;
        }

        isBreathing = false;
    }

    private void FinishFireBreath()
    {
        ElementSkillData finishedSkill =
            activeBreathSkill;

        bool didCast =
            breathActuallyCast;

        isBreathing = false;
        isCasting = false;

        activeBreathSkill = null;

        breathActuallyCast = false;
        breathSoundPlayed = false;

        if (finishedSkill != null)
        {
            int index =
                finishedSkill.skillIndex - 1;

            if (index >= 0 &&
                index < durationTimers.Length)
            {
                durationTimers[index] = 0f;
            }

            /*
             * Chỉ hồi chiêu nếu đã tạo được
             * ít nhất một đợt Fire Breath.
             */
            if (didCast &&
                index >= 0 &&
                index < cooldownTimers.Length)
            {
                cooldownTimers[index] =
                    Mathf.Max(
                        0f,
                        finishedSkill.cooldown
                    );
            }
        }

        EnableCombatActions();
    }

    // =====================================================
    // FIRE SKILL 4 - TORNADO
    // =====================================================

    private void CastFireTornado(
        Vector2 direction)
    {
        if (fireTornadoPrefab == null)
        {
            Debug.LogError(
                "Fire Tornado Prefab chưa được gán."
            );

            return;
        }

        direction =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        Vector3 spawnPosition =
            transform.position +
            (Vector3)(
                direction *
                tornadoSpawnDistance
            );

        GameObject tornado =
            Instantiate(
                fireTornadoPrefab,
                spawnPosition,
                Quaternion.identity
            );

        FireTornado tornadoSkill =
            tornado.GetComponent<
                FireTornado
            >();

        if (tornadoSkill == null)
        {
            Debug.LogError(
                "Fire Tornado Prefab thiếu FireTornado."
            );

            Destroy(tornado);
            return;
        }

        tornadoSkill.Initialize(
            direction,
            gameObject
        );

        PlaySound(fireTornadoSound);
    }

    // =====================================================
    // HELPERS
    // =====================================================

    private Vector2 GetCastDirection()
    {
        if (player != null &&
            player.LastDirection.sqrMagnitude >
            0.001f)
        {
            return player.LastDirection.normalized;
        }

        return Vector2.down;
    }

    private Vector2 GetCardinalDirection(
        Vector2 input)
    {
        if (Mathf.Abs(input.x) >
            Mathf.Abs(input.y))
        {
            return input.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }

        return input.y >= 0f
            ? Vector2.up
            : Vector2.down;
    }

    private Vector3 GetSpawnPosition(
        Vector2 direction)
    {
        if (firePoint != null)
            return firePoint.position;

        return transform.position +
               (Vector3)(
                   direction * 0.6f
               );
    }

    private void PlayCastAnimation(
        Vector2 direction)
    {
        if (animator == null)
            return;

        animator.SetFloat(
            "CastX",
            direction.x
        );

        animator.SetFloat(
            "CastY",
            direction.y
        );

        animator.ResetTrigger(
            "FireCast"
        );

        animator.SetTrigger(
            "FireCast"
        );
    }

    private void DisableCombatActions()
    {
        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        if (dash != null)
            dash.enabled = false;
    }

    private void EnableCombatActions()
    {
        Health health =
            GetComponent<Health>();

        if (health != null &&
            health.IsDead)
        {
            return;
        }

        if (attack != null)
            attack.enabled = true;

        if (dash != null)
            dash.enabled = true;
    }

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null ||
            AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(
            clip
        );
    }

    private void EndNormalCast()
    {
        isCasting = false;

        EnableCombatActions();

        if (player != null)
            player.UnlockControl();
    }

    // =====================================================
    // UI GETTERS
    // =====================================================

    public float GetCooldownNormalized(
        ElementSkillData data)
    {
        if (data == null ||
            data.skillIndex < 1 ||
            data.skillIndex > 4 ||
            data.cooldown <= 0f)
        {
            return 0f;
        }

        int index =
            data.skillIndex - 1;

        return Mathf.Clamp01(
            cooldownTimers[index] /
            data.cooldown
        );
    }

    public float GetRemainingCooldown(
        ElementSkillData data)
    {
        if (data == null ||
            data.skillIndex < 1 ||
            data.skillIndex > 4)
        {
            return 0f;
        }

        return cooldownTimers[
            data.skillIndex - 1
        ];
    }

    public float GetDurationNormalized(
        ElementSkillData data)
    {
        if (data == null ||
            data.skillIndex < 1 ||
            data.skillIndex > 4)
        {
            return 0f;
        }

        int index =
            data.skillIndex - 1;

        if (index == 2 &&
            isBreathing)
        {
            return 1f;
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

    public float GetRemainingDuration(
        ElementSkillData data)
    {
        if (data == null ||
            data.skillIndex < 1 ||
            data.skillIndex > 4)
        {
            return 0f;
        }

        int index =
            data.skillIndex - 1;

        if (index == 2 &&
            isBreathing)
        {
            return maxDurationTimers[index];
        }

        return durationTimers[index];
    }

    public bool IsSkillActive(
        ElementSkillData data)
    {
        if (data == null ||
            data.skillIndex < 1 ||
            data.skillIndex > 4)
        {
            return false;
        }

        int index =
            data.skillIndex - 1;

        if (index == 2)
            return isBreathing;

        return durationTimers[index] > 0f;
    }

    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDisable()
    {
        if (normalCastCoroutine != null)
        {
            StopCoroutine(
                normalCastCoroutine
            );

            normalCastCoroutine = null;
        }

        if (meteorRainCoroutine != null)
        {
            StopCoroutine(
                meteorRainCoroutine
            );

            meteorRainCoroutine = null;
        }

        if (breathCoroutine != null)
        {
            StopCoroutine(
                breathCoroutine
            );

            breathCoroutine = null;
        }

        bool wasNormalCasting =
            isCasting && !isBreathing;

        isBreathing = false;
        isCasting = false;
        activeBreathSkill = null;

        EnableCombatActions();

        if (wasNormalCasting &&
            player != null)
        {
            player.UnlockControl();
        }
    }
}