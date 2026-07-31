using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FireTornado : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float duration = 5f;

    [Header("Damage")]
    [SerializeField] private int damagePerTick = 15;
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Collision")]
    [SerializeField] private LayerMask enemyLayer;

    private Rigidbody2D rb;
    private Vector2 direction;
    private GameObject owner;

    private readonly Dictionary<
        EnermyHealth,
        float
    > lastDamageTimes =
        new Dictionary<EnermyHealth, float>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    public void Initialize(
        Vector2 moveDirection,
        GameObject skillOwner)
    {
        direction =
            moveDirection.sqrMagnitude > 0.001f
                ? moveDirection.normalized
                : Vector2.down;

        owner = skillOwner;

        Destroy(
            gameObject,
            Mathf.Max(0.1f, duration)
        );
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity =
                direction * speed;
        }
    }

    private void OnTriggerStay2D(
        Collider2D other)
    {
        if (!IsInLayer(
                other.gameObject,
                enemyLayer))
        {
            return;
        }

        EnermyHealth enemy =
            other.GetComponentInParent<
                EnermyHealth
            >();

        if (enemy == null)
            return;

        if (lastDamageTimes.TryGetValue(
                enemy,
                out float lastTime))
        {
            if (Time.time - lastTime <
                tickInterval)
            {
                return;
            }
        }

        lastDamageTimes[enemy] =
            Time.time;

        enemy.TakeDamage(
            damagePerTick,
            direction
        );
    }

    private bool IsInLayer(
        GameObject target,
        LayerMask mask)
    {
        return (
            mask.value &
            (1 << target.layer)
        ) != 0;
    }
}