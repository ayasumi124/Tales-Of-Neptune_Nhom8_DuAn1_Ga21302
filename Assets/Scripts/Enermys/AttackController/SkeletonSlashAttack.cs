using System.Collections;
using UnityEngine;

public class SkeletonSlashAttack :
    EnermyAttackBase
{
    [Header("Slash Projectile")]
    [SerializeField]
    private Transform slashSpawnPoint;

    [SerializeField]
    private GameObject slashPrefab;

    [Min(1)]
    [SerializeField]
    private int projectileDamage = 2;

    [Min(0f)]
    [SerializeField]
    private float projectileSpeed = 6f;

    [Header("Timing")]
    [Tooltip(
        "Nếu dùng Animation Event để spawn " +
        "projectile thì bật ô này."
    )]
    [SerializeField]
    private bool useAnimationEvent = true;

    [Tooltip(
        "Nếu không dùng Animation Event, " +
        "projectile sẽ spawn sau thời gian này."
    )]
    [Min(0f)]
    [SerializeField]
    private float projectileDelay = 0.25f;

    private bool projectileSpawned;
    private EnermyHealth health;

    protected override void Awake()
    {
        base.Awake();

        health =
            GetComponent<EnermyHealth>();
    }
    protected override void OnAttackStarted()
    {
        projectileSpawned = false;
    }

    protected override IEnumerator PerformAttack()
    {
        if (!useAnimationEvent)
        {
            yield return new WaitForSeconds(
                Mathf.Max(
                    0f,
                    projectileDelay
                )
            );

            SpawnSlashProjectile();
        }

        /*
         * Chờ tới khi projectile đã spawn,
         * hoặc tới hết attackLockTime.
         */
        float waitTimer =
            Mathf.Max(
                0.1f,
                attackLockTime
            );

        while (useAnimationEvent &&
               !projectileSpawned &&
               waitTimer > 0f)
        {
            waitTimer -= Time.deltaTime;
            yield return null;
        }
    }

    // Animation Event đặt tại frame vung kiếm
    public void SpawnSlashProjectile()
    {

        if (!isAttacking)
            return;

        if (health != null &&
            health.IsHurting)
        {
            CancelAttack();
            return;
        }

        if (projectileSpawned)
            return;
        if (slashPrefab == null)
        {
            Debug.LogError(
                $"{name}: chưa gán Slash Prefab.",
                this
            );

            return;
        }

        if (slashSpawnPoint == null)
        {
            Debug.LogError(
                $"{name}: chưa gán Slash Spawn Point.",
                this
            );

            return;
        }

        projectileSpawned = true;

        Vector2 direction = Vector2.right;

        /*
         * Ưu tiên lấy hướng tới target.
         */
        if (movement != null &&
            movement.HasTarget() &&
            movement.Target != null)
        {
            direction =
                (
                    (Vector2)movement.Target.position -
                    (Vector2)slashSpawnPoint.position
                ).normalized;
        }
        else if (movement != null)
        {
            direction =
                movement.LastMoveDirection;
        }

        /*
         * Chống trường hợp direction = (0,0).
         */
        if (direction.sqrMagnitude <= 0.001f)
        {
            direction =
                transform.localScale.x < 0f
                    ? Vector2.left
                    : Vector2.right;
        }

        GameObject slashObject =
            Instantiate(
                slashPrefab,
                slashSpawnPoint.position,
                Quaternion.identity
            );

        EnermySlashProjectile projectile =
            slashObject.GetComponent<
                EnermySlashProjectile
            >();

        if (projectile == null)
        {
            Debug.LogError(
                $"{slashObject.name} thiếu " +
                "EnermySlashProjectile.",
                slashObject
            );

            Destroy(slashObject);
            return;
        }

        projectile.Initialize(
            direction,
            gameObject,
            projectileDamage,
            projectileSpeed
        );

        Debug.Log(
            $"{name} spawn slash | " +
            $"Direction: {direction} | " +
            $"Speed: {projectileSpeed}",
            slashObject
        );
    }


    public override void EndAttack()
    {
        projectileSpawned = false;

        base.EndAttack();
    }

    public override void CancelAttack()
    {
        projectileSpawned = false;

        base.CancelAttack();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            Mathf.Max(
                0f,
                attackRange
            )
        );

        if (slashSpawnPoint != null)
        {
            Gizmos.color =
                Color.cyan;

            Gizmos.DrawWireSphere(
                slashSpawnPoint.position,
                0.1f
            );
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        projectileDamage =
            Mathf.Max(
                1,
                projectileDamage
            );

        projectileSpeed =
            Mathf.Max(
                0f,
                projectileSpeed
            );

        projectileDelay =
            Mathf.Max(
                0f,
                projectileDelay
            );
    }
}