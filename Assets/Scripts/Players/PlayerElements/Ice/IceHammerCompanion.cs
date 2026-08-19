using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceHammerCompanion : MonoBehaviour
{
    // =====================================================
    // FOLLOW
    // =====================================================

    [Header("Follow")]
    [SerializeField]
    private float followSpeed = 6f;

    [Tooltip("Khoảng cách ngang so với Player.")]
    [SerializeField]
    private float horizontalOffset = 0.45f;

    [Tooltip("Độ cao Hammer bay trên Player.")]
    [SerializeField]
    private float verticalOffset = 1.2f;

    [SerializeField]
    private float floatHeight = 0.12f;

    [SerializeField]
    private float floatSpeed = 3f;


    // =====================================================
    // SLAM
    // =====================================================

    [Header("Slam")]
    [SerializeField]
    private float attackForwardDistance = 0.9f;

    [SerializeField]
    private float moveToAttackSpeed = 12f;

    [SerializeField]
    private float returnDelay = 0.1f;

    [SerializeField]
    private float slamCooldown = 0.25f;


    // =====================================================
    // DAMAGE
    // =====================================================

    [Header("AOE Damage")]
    [SerializeField]
    private int damage = 30;

    [SerializeField]
    private float impactRadius = 1.2f;

    [SerializeField]
    private float knockbackStrength = 5f;

    [SerializeField]
    private LayerMask enemyLayer;


    // =====================================================
    // SLOW
    // =====================================================

    [Header("Slow")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float slowMultiplier = 0.4f;

    [SerializeField]
    private float slowDuration = 2.5f;

    [SerializeField]
    private GameObject slowEffectPrefab;


    // =====================================================
    // IMPACT
    // =====================================================

    [Header("Impact VFX")]
    [SerializeField]
    private GameObject impactEffectPrefab;

    [Header("Impact Audio")]
    [SerializeField]
    private AudioClip impactSound;

    [Range(0f, 5f)]
    [SerializeField]
    private float impactVolume = 3f;


    // =====================================================
    // FALLING ICE SPIKES
    // =====================================================

    [Header("Falling Ice Spikes")]
    [SerializeField]
    private GameObject fallingIceSpikePrefab;

    [Min(0)]
    [SerializeField]
    private int fallingSpikeCount = 5;

    [Tooltip(
        "Khoảng cách tối thiểu để spike " +
        "không rơi sát tâm impact."
    )]
    [SerializeField]
    private float fallingSpikeMinRadius = 0.7f;

    [Tooltip(
        "Khoảng cách tối đa của spike."
    )]
    [SerializeField]
    private float fallingSpikeRadius = 1.4f;

    [SerializeField]
    private float fallingSpikeHeight = 1.8f;

    [Header("Hammer Swing Audio")]
    [SerializeField]
    private AudioClip swingSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float swingVolume = 1f;

    [Header("Slow Audio")]
    [SerializeField]
    private AudioClip slowSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float slowSoundVolume = 0.5f;
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private SpriteRenderer hammerSprite;

    [Tooltip(
        "Nếu Animator/Sprite nằm ở child Visual, " +
        "kéo Visual vào đây."
    )]
    [SerializeField]
    private Transform visualRoot;

    private Transform player;
    private Players players;
    private Attack playerAttack;


    // =====================================================
    // STATE
    // =====================================================

    private bool initialized;
    private bool isSlamming;

    private float nextSlamTime;

    private Vector2 attackDirection =
        Vector2.down;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        GameObject playerObject)
    {
        if (playerObject == null)
        {
            Debug.LogError(
                "IceHammerCompanion: Player null."
            );

            Destroy(gameObject);
            return;
        }

        player =
            playerObject.transform;

        players =
            playerObject.GetComponent<Players>();

        playerAttack =
            playerObject.GetComponent<Attack>();

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        if (hammerSprite == null)
        {
            hammerSprite =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (visualRoot == null &&
            hammerSprite != null)
        {
            visualRoot =
                hammerSprite.transform;
        }

        if (playerAttack == null)
        {
            Debug.LogError(
                "Player thiếu Attack.cs."
            );
        }
        else
        {
            playerAttack.OnAttackImpactFrame +=
                HandlePlayerAttack;
        }

        initialized = true;

        UpdateFacing();

        SnapToIdlePosition();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!initialized ||
            player == null)
        {
            return;
        }

        if (isSlamming)
            return;

        UpdateFacing();

        FollowPlayer();
    }


    // =====================================================
    // FOLLOW
    // =====================================================

    private void FollowPlayer()
    {
        Vector3 target =
            GetIdlePosition();

        target.y +=
            Mathf.Sin(
                Time.time *
                floatSpeed
            ) *
            floatHeight;

        transform.position =
            Vector3.Lerp(
                transform.position,
                target,
                followSpeed *
                Time.deltaTime
            );
    }


    private Vector3 GetIdlePosition()
    {
        if (player == null)
            return transform.position;

        float side = 1f;

        if (players != null)
        {
            /*
             * FacingDirection của bạn:
             * 1 = phải
             * -1 = trái.
             *
             * Hammer nằm chếch phía trên
             * theo hướng Player đang nhìn.
             */
            side =
                players.FacingDirection >= 0
                    ? 1f
                    : -1f;
        }

        return
            player.position +
            new Vector3(
                horizontalOffset * side,
                verticalOffset,
                0f
            );
    }


    private void SnapToIdlePosition()
    {
        transform.position =
            GetIdlePosition();
    }


    // =====================================================
    // FLIP HAMMER
    // =====================================================

    private void UpdateFacing()
    {
        if (hammerSprite == null ||
            players == null)
        {
            return;
        }

        /*
         * Nếu sprite gốc Hammer đang nhìn PHẢI:
         *
         * FacingDirection > 0
         * → không flip
         *
         * FacingDirection < 0
         * → flip X.
         */
        hammerSprite.flipX =
            players.FacingDirection < 0;
    }


    private void UpdateAttackFacing()
    {
        if (hammerSprite == null)
            return;

        /*
         * Trái / phải:
         * flip theo hướng tấn công.
         */
        if (attackDirection == Vector2.left)
        {
            hammerSprite.flipX = true;
        }
        else if (
            attackDirection == Vector2.right)
        {
            hammerSprite.flipX = false;
        }

        /*
         * Up / Down giữ flip ngang gần nhất.
         * Không xoay cả Hammer vì animation
         * Slam của asset vốn đập từ trên xuống.
         */
    }


    // =====================================================
    // PLAYER ATTACK EVENT
    // =====================================================

    private void HandlePlayerAttack()
    {
        if (!initialized ||
            player == null)
        {
            return;
        }

        if (isSlamming)
            return;

        if (Time.time <
            nextSlamTime)
        {
            return;
        }

        nextSlamTime =
            Time.time +
            slamCooldown;

        attackDirection =
            GetPlayerAttackDirection();

        UpdateAttackFacing();

        StartCoroutine(
            SlamRoutine()
        );
    }


    // =====================================================
    // GET PLAYER DIRECTION
    // =====================================================

    private Vector2 GetPlayerAttackDirection()
    {
        if (players == null)
            return Vector2.down;

        Vector2 direction =
            players.LastDirection;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return
                players.FacingDirection >= 0
                    ? Vector2.right
                    : Vector2.left;
        }

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
    // SLAM
    // =====================================================

    private IEnumerator SlamRoutine()
    {
        isSlamming = true;

        /*
         * Chốt vị trí attack ngay lúc
         * Player tới hit-frame.
         *
         * Player chạy tiếp cũng không
         * làm Hammer đổi target giữa chừng.
         */
        Vector3 targetPosition =
            player.position +
            (Vector3)(
                attackDirection *
                attackForwardDistance
            );

        while (
            Vector2.Distance(
                transform.position,
                targetPosition
            ) > 0.05f)
        {
            if (player == null)
            {
                isSlamming = false;
                yield break;
            }

            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPosition,
                    moveToAttackSpeed *
                    Time.deltaTime
                );

            yield return null;
        }

        transform.position =
            targetPosition;

        if (animator != null)
        {
            PlaySwingSound();
            animator.ResetTrigger("Slam");
            animator.SetTrigger("Slam");
        }
        else
        {
            Debug.LogWarning(
                "Ice Hammer thiếu Animator. " +
                "Dùng Impact trực tiếp để test."
            );

            Impact();
            EndSlam();
        }

        while (isSlamming)
        {
            yield return null;
        }
    }


    // =====================================================
    // ANIMATION EVENT - IMPACT
    // =====================================================

    public void Impact()
    {
        if (!isSlamming)
            return;

        DamageEnemies();

        SpawnImpactEffect();

        SpawnFallingIceSpikes();

        PlayImpactSound();
    }

    private void PlaySwingSound()
    {
        if (swingSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    swingSound,
                    swingVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            swingSound,
            transform.position,
            swingVolume
        );
    }


    // =====================================================
    // ANIMATION EVENT - END
    // =====================================================

    public void EndSlam()
    {
        if (!isSlamming)
            return;

        StartCoroutine(
            EndSlamRoutine()
        );
    }


    private IEnumerator EndSlamRoutine()
    {
        if (returnDelay > 0f)
        {
            yield return new WaitForSeconds(
                returnDelay
            );
        }

        isSlamming = false;

        UpdateFacing();
    }


    // =====================================================
    // DAMAGE ENEMY
    // =====================================================

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                impactRadius,
                enemyLayer
            );

        HashSet<EnermyHealth>
            damagedEnemies =
                new HashSet<EnermyHealth>();

        foreach (
            Collider2D hit
            in hits)
        {
            if (hit == null)
                continue;

            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null)
                continue;

            if (damagedEnemies.Contains(
                    enemy))
            {
                continue;
            }

            damagedEnemies.Add(
                enemy
            );

            Vector2 knockDirection =
                (
                    enemy.transform.position -
                    transform.position
                ).normalized;

            if (knockDirection.sqrMagnitude <
                0.001f)
            {
                knockDirection =
                    attackDirection;
            }

            /*
             * Dùng overload giống Player Attack:
             * damage + direction +
             * knockback + có popup.
             */
            enemy.TakeDamage(
                damage,
                knockDirection,
                knockbackStrength,
                true
            );

            ApplySlow(
                enemy
            );
        }
    }


    // =====================================================
    // SLOW
    // =====================================================

    private void ApplySlow(
        EnermyHealth enemy)
    {
        if (enemy == null)
            return;

        EnemySlowEffect slow =
            enemy.GetComponent<
                EnemySlowEffect
            >();

        if (slow == null)
        {
            slow =
                enemy.gameObject
                    .AddComponent<
                        EnemySlowEffect
                    >();
        }

        slow.ApplySlow(
    slowMultiplier,
    slowDuration,
    slowEffectPrefab,
    slowSound,
    slowSoundVolume
);
    }


    // =====================================================
    // IMPACT VFX
    // =====================================================

    private void SpawnImpactEffect()
    {
        if (impactEffectPrefab == null)
            return;

        GameObject effect =
            Instantiate(
                impactEffectPrefab,
                transform.position,
                Quaternion.identity
            );

        Destroy(
            effect,
            2f
        );
    }


    // =====================================================
    // IMPACT SOUND
    // =====================================================

    private void PlayImpactSound()
    {
        if (impactSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    impactSound,
                    impactVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            impactSound,
            transform.position,
            impactVolume
        );
    }


    // =====================================================
    // FALLING ICE SPIKES
    // =====================================================

    private void SpawnFallingIceSpikes()
    {
        if (fallingIceSpikePrefab == null ||
            fallingSpikeCount <= 0)
        {
            return;
        }

        float minRadius =
            Mathf.Max(
                0f,
                fallingSpikeMinRadius
            );

        float maxRadius =
            Mathf.Max(
                minRadius,
                fallingSpikeRadius
            );

        for (int i = 0;
             i < fallingSpikeCount;
             i++)
        {
            Vector2 direction =
                Random.insideUnitCircle;

            if (direction.sqrMagnitude <
                0.001f)
            {
                direction =
                    Vector2.right;
            }

            direction.Normalize();

            float distance =
                Random.Range(
                    minRadius,
                    maxRadius
                );

            Vector2 randomOffset =
                direction * distance;

            Vector3 targetPosition =
                transform.position +
                new Vector3(
                    randomOffset.x,
                    randomOffset.y,
                    0f
                );

            Vector3 spawnPosition =
                targetPosition +
                Vector3.up *
                fallingSpikeHeight;

            GameObject spikeObject =
                Instantiate(
                    fallingIceSpikePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            IceHammerFallingSpike spike =
                spikeObject.GetComponent<
                    IceHammerFallingSpike
                >();

            if (spike == null)
            {
                Debug.LogError(
                    "Falling Ice Spike Prefab " +
                    "thiếu IceHammerFallingSpike.cs."
                );

                Destroy(
                    spikeObject
                );

                continue;
            }

            spike.Initialize(
                targetPosition,
                gameObject
            );
        }
    }


    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        if (playerAttack != null)
        {
            playerAttack
                .OnAttackImpactFrame -=
                HandlePlayerAttack;
        }
    }


    // =====================================================
    // GIZMO
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            impactRadius
        );
    }
}