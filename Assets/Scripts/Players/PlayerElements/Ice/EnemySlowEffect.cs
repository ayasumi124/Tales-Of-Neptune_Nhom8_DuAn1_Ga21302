using System.Collections;
using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    [Header("Runtime")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float currentSpeedMultiplier = 1f;

    private Coroutine slowCoroutine;

    public float SpeedMultiplier =>
        currentSpeedMultiplier;

    public bool IsSlowed =>
        currentSpeedMultiplier < 1f;

    public void ApplySlow(
        float multiplier,
        float duration)
    {
        multiplier =
            Mathf.Clamp(
                multiplier,
                0.05f,
                1f
            );

        duration =
            Mathf.Max(
                0.05f,
                duration
            );

        /*
         * Nếu đang bị slow mạnh hơn
         * thì giữ slow mạnh hơn.
         *
         * Ví dụ:
         * đang 0.5 mà dính 0.7
         * → vẫn giữ 0.5.
         */
        currentSpeedMultiplier =
            Mathf.Min(
                currentSpeedMultiplier,
                multiplier
            );

        if (slowCoroutine != null)
        {
            StopCoroutine(
                slowCoroutine
            );
        }

        slowCoroutine =
            StartCoroutine(
                SlowRoutine(
                    duration
                )
            );

        Debug.Log(
            $"{name} bị Slow. " +
            $"Speed x{currentSpeedMultiplier} " +
            $"trong {duration:F1}s."
        );
    }

    private IEnumerator SlowRoutine(
        float duration)
    {
        yield return new WaitForSeconds(
            duration
        );

        RemoveSlow();
    }

    public void RemoveSlow()
    {
        if (slowCoroutine != null)
        {
            StopCoroutine(
                slowCoroutine
            );

            slowCoroutine = null;
        }

        currentSpeedMultiplier = 1f;
    }

    private void OnDisable()
    {
        RemoveSlow();
    }
}