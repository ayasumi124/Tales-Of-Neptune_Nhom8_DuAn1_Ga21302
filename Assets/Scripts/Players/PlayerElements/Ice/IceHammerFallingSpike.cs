using UnityEngine;

public class IceHammerFallingSpike :
    MonoBehaviour
{
    // =====================================================
    // MOVEMENT
    // =====================================================

    [Header("Fall")]
    [SerializeField]
    private float fallSpeed = 7f;

    [SerializeField]
    private float stopDistance = 0.05f;


    // =====================================================
    // VISUAL
    // =====================================================

    [Header("Visual")]
    [SerializeField]
    private Transform visual;

    [Tooltip(
        "Asset gốc hướng sang phải " +
        "thì -90 độ sẽ quay xuống."
    )]
    [SerializeField]
    private float visualRotationZ = -90f;


    // =====================================================
    // DAMAGE
    // =====================================================

    [Header("Damage")]
    [SerializeField]
    private int damage = 10;

    [SerializeField]
    private float hitRadius = 0.35f;

    [SerializeField]
    private float knockbackStrength = 2f;

    [SerializeField]
    private LayerMask enemyLayer;


    // =====================================================
    // EFFECT
    // =====================================================

    [Header("Impact")]
    [SerializeField]
    private GameObject impactEffectPrefab;

    [SerializeField]
    private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float impactVolume = 0.5f;


    // =====================================================
    // STATE
    // =====================================================

    private Vector3 targetPosition;

    private GameObject owner;

    private bool initialized;
    private bool impacted;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        Vector3 target,
        GameObject spikeOwner)
    {
        targetPosition =
            target;

        owner =
            spikeOwner;

        if (visual == null)
        {
            SpriteRenderer sr =
                GetComponentInChildren<
                    SpriteRenderer
                >();

            if (sr != null)
            {
                visual =
                    sr.transform;
            }
        }

        /*
         * Asset gốc đang bay ngang →
         * quay visual xuống dưới.
         */
        if (visual != null)
        {
            visual.localRotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    visualRotationZ
                );
        }

        initialized = true;
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void Update()
    {
        if (!initialized ||
            impacted)
        {
            return;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                fallSpeed *
                Time.deltaTime
            );

        if (Vector2.Distance(
                transform.position,
                targetPosition
            ) <= stopDistance)
        {
            Impact();
        }
    }


    // =====================================================
    // IMPACT
    // =====================================================

    private void Impact()
    {
        if (impacted)
            return;

        impacted = true;

        transform.position =
            targetPosition;

        DamageEnemies();

        SpawnEffect();

        PlaySound();

        Destroy(
            gameObject,
            0.15f
        );
    }


    // =====================================================
    // DAMAGE
    // =====================================================

    private void DamageEnemies()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                hitRadius,
                enemyLayer
            );

        foreach (
            Collider2D hit
            in hits)
        {
            if (hit == null)
                continue;

            if (owner != null &&
                (
                    hit.gameObject == owner ||
                    hit.transform.IsChildOf(
                        owner.transform
                    )
                ))
            {
                continue;
            }

            EnermyHealth enemy =
                hit.GetComponentInParent<
                    EnermyHealth
                >();

            if (enemy == null)
                continue;

            enemy.TakeDamage(
                damage,
                Vector2.down,
                knockbackStrength,
                true
            );

            /*
             * Spike nhỏ chỉ damage.
             *
             * Slow chính đã được Hammer
             * AOE áp dụng rồi.
             */
        }
    }


    // =====================================================
    // EFFECT
    // =====================================================

    private void SpawnEffect()
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
            1f
        );
    }


    private void PlaySound()
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
        }
    }


    // =====================================================
    // GIZMO
    // =====================================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            hitRadius
        );
    }
}