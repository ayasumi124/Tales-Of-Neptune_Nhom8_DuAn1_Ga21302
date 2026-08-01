using System.Collections;
using UnityEngine;

public class FireSkillController : MonoBehaviour
{
    [Header("Fire Element Data")]
    [SerializeField] private ElementData fireData;

    [Header("Spawn")]
    [SerializeField] private Transform firePoint;

    [Header("Prefabs")]
    [SerializeField] private GameObject fireBallPrefab;
    [SerializeField] private GameObject fireSpearPrefab;
    [SerializeField] private GameObject fireBreathPrefab;
    [SerializeField] private GameObject fireTornadoPrefab;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float castLockTime = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip fireBallSound;
    [SerializeField] private AudioClip fireSpearSound;
    [SerializeField] private AudioClip fireBreathSound;
    [SerializeField] private AudioClip fireTornadoSound;

    private PlayerMana mana;
    private Players player;
    private Attack attack;
    private PlayerDash dash;

    private bool isCasting;

    private readonly float[] cooldownTimers =
        new float[4];

    public bool IsCasting => isCasting;

    [Header("Fire Skill 2")]
    [SerializeField] private GameObject fireSparkPrefab;

    [Range(1, 15)]
    [SerializeField] private int fireSparkCount = 7;

    [Tooltip("Tổng góc tỏa của toàn bộ chùm FireSpark.")]
    [SerializeField] private float fireSparkSpreadAngle = 36f;

    [SerializeField] private float fireSparkSpawnOffset = 0.55f;

    [SerializeField] private float fireSparkSpawnInterval = 0.025f;
    private void Awake()
    {
        mana = GetComponent<PlayerMana>();
        player = GetComponent<Players>();
        attack = GetComponent<Attack>();
        dash = GetComponent<PlayerDash>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    private void Update()
    {
        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;

                if (cooldownTimers[i] < 0f)
                    cooldownTimers[i] = 0f;
            }
        }
    }

    public void TryCast(ElementSkillData skillData)
    {
        if (skillData == null)
            return;

        if (skillData.elementType != ElementType.Fire)
            return;

        if (!skillData.unlocked)
        {
            Debug.Log($"{skillData.skillName} chưa mở khóa.");
            return;
        }

        if (isCasting)
            return;

        if (player != null && player.IsControlLocked)
            return;

        if (attack != null && attack.IsAttacking)
            return;

        if (dash != null && dash.IsDashing)
            return;

        int index = skillData.skillIndex - 1;

        if (index < 0 || index >= cooldownTimers.Length)
        {
            Debug.LogError(
                $"{skillData.skillName} có Skill Index không hợp lệ."
            );

            return;
        }

        if (cooldownTimers[index] > 0f)
        {
            Debug.Log(
                $"{skillData.skillName} còn hồi " +
                $"{cooldownTimers[index]:F1} giây."
            );

            return;
        }

        if (mana == null ||
            !mana.UseMana(skillData.manaCost))
        {
            if (ManaUI.Instance != null)
                ManaUI.Instance.ShowNoMana();

            return;
        }

        cooldownTimers[index] =
            Mathf.Max(0f, skillData.cooldown);

        StartCoroutine(
            CastRoutine(skillData)
        );
    }

    private IEnumerator CastRoutine(
        ElementSkillData skillData)
    {
        isCasting = true;

        Vector2 direction = GetCastDirection();

        if (player != null)
            player.LockControl();

        if (attack != null)
        {
            attack.CancelAttack();
            attack.enabled = false;
        }

        if (dash != null)
            dash.enabled = false;

        PlayCastAnimation(direction);

        yield return new WaitForSeconds(
            Mathf.Max(0f, castLockTime * 0.5f)
        );

        switch (skillData.skillIndex)
        {
            case 1:
                CastFireBall(direction);
                break;

            case 2:
                CastFireSpear(direction);
                break;

            case 3:
                CastFireBreath(direction);
                break;

            case 4:
                CastFireTornado(direction);
                break;
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, castLockTime * 0.5f)
        );

        EndCast();
    }

    private Vector2 GetCastDirection()
    {
        if (player != null &&
            player.LastDirection.sqrMagnitude > 0.001f)
        {
            return player.LastDirection.normalized;
        }

        return Vector2.down;
    }

    private Vector3 GetSpawnPosition(
        Vector2 direction)
    {
        if (firePoint != null)
            return firePoint.position;

        return transform.position +
               (Vector3)(direction * 0.6f);
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

        animator.ResetTrigger("FireCast");
        animator.SetTrigger("FireCast");
    }

    private void CastFireBall(
        Vector2 direction)
    {
        if (fireBallPrefab == null)
            return;

        GameObject fireBall = Instantiate(
            fireBallPrefab,
            GetSpawnPosition(direction),
            Quaternion.identity
        );

        FireProjectile projectile =
            fireBall.GetComponent<FireProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(
                direction,
                gameObject
            );
        }

        PlaySound(fireBallSound);
    }

    private void CastFireSpear(
    Vector2 direction)
    {
        if (fireSparkPrefab == null)
        {
            Debug.LogError(
                "Fire Skill 2 chưa được gán FireSpark Prefab."
            );

            return;
        }

        SpawnFireSparkFan(direction);

        PlaySound(fireSpearSound);
    }

    private void SpawnFireSparkFan(
    Vector2 direction)
    {
        direction =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        int count =
            Mathf.Max(1, fireSparkCount);

        float totalSpread =
            Mathf.Max(0f, fireSparkSpreadAngle);

        float startAngle =
            count > 1
                ? -totalSpread * 0.5f
                : 0f;

        float angleStep =
            count > 1
                ? totalSpread / (count - 1)
                : 0f;

        for (int i = 0; i < count; i++)
        {
            float currentAngle =
                startAngle + angleStep * i;

            Vector2 sparkDirection =
                RotateDirection(
                    direction,
                    currentAngle
                ).normalized;

            Vector3 spawnPosition =
                transform.position +
                (Vector3)(
                    sparkDirection *
                    fireSparkSpawnOffset
                );

            GameObject spark =
                Instantiate(
                    fireSparkPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            FireSparkProjectile projectile =
                spark.GetComponent<
                    FireSparkProjectile
                >();

            if (projectile == null)
            {
                Debug.LogError(
                    "FireSpark Prefab thiếu FireSparkProjectile."
                );

                Destroy(spark);
                continue;
            }

            projectile.Initialize(
                sparkDirection,
                gameObject
            );
        }
    }

    private void CastFireBreath(
    Vector2 direction)
    {
        if (fireBreathPrefab == null)
            return;

        StartCoroutine(
            SpawnFireBreathSequence(direction)
        );

        PlaySound(fireBreathSound);
    }

    private IEnumerator SpawnFireBreathSequence(
        Vector2 direction)
    {
        direction =
            direction.sqrMagnitude > 0.001f
                ? direction.normalized
                : Vector2.down;

        int segmentCount = 5;
        float segmentSpacing = 0.45f;
        float spawnInterval = 0.07f;

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 spawnPosition =
                transform.position +
                (Vector3)(
                    direction *
                    (0.45f + i * segmentSpacing)
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

            if (breathSegment != null)
            {
                breathSegment.Initialize(
                    direction,
                    gameObject,
                    i
                );
            }

            yield return new WaitForSeconds(
                spawnInterval
            );
        }
    }

    private void CastFireTornado(
        Vector2 direction)
    {
        if (fireTornadoPrefab == null)
            return;

        GameObject tornado = Instantiate(
            fireTornadoPrefab,
            GetSpawnPosition(direction),
            Quaternion.identity
        );

        FireTornado tornadoSkill =
            tornado.GetComponent<FireTornado>();

        if (tornadoSkill != null)
        {
            tornadoSkill.Initialize(
                direction,
                gameObject
            );
        }

        PlaySound(fireTornadoSound);
    }

    private Vector2 RotateDirection(
        Vector2 direction,
        float angle)
    {
        return Quaternion.Euler(
            0f,
            0f,
            angle
        ) * direction;
    }

    private void PlaySound(
        AudioClip clip)
    {
        if (clip == null ||
            AudioManager.Instance == null)
        {
            return;
        }

        AudioManager.Instance.PlaySFX(clip);
    }

    private void EndCast()
    {
        isCasting = false;

        if (attack != null)
            attack.enabled = true;

        if (dash != null)
            dash.enabled = true;

        if (player != null)
            player.UnlockControl();
    }

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

        int index = data.skillIndex - 1;

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
}