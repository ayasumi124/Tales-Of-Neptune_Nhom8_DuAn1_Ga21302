using System.Collections;
using UnityEngine;

public class EnemyBurnEffect : MonoBehaviour
{
    private EnermyHealth enemyHealth;

    private Coroutine burnCoroutine;
    private GameObject activeVisual;

    private void Awake()
    {
        enemyHealth =
            GetComponent<EnermyHealth>();
    }

    public void ApplyBurn(
        int damagePerTick,
        float interval,
        float duration,
        GameObject effectPrefab)
    {
        if (enemyHealth == null)
            return;

        if (burnCoroutine != null)
            StopCoroutine(burnCoroutine);

        if (activeVisual == null &&
            effectPrefab != null)
        {
            activeVisual = Instantiate(
                effectPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

            activeVisual.transform.localPosition =
                Vector3.zero;
        }

        burnCoroutine =
            StartCoroutine(
                BurnRoutine(
                    damagePerTick,
                    interval,
                    duration
                )
            );
    }

    private IEnumerator BurnRoutine(
        int damage,
        float interval,
        float duration)
    {
        float timer = 0f;

        interval =
            Mathf.Max(0.05f, interval);

        while (timer < duration)
        {
            if (enemyHealth == null)
                break;

            enemyHealth.TakeDamage(
                damage,
                Vector2.zero
            );

            yield return new WaitForSeconds(
                interval
            );

            timer += interval;
        }

        if (activeVisual != null)
        {
            Destroy(activeVisual);
            activeVisual = null;
        }

        burnCoroutine = null;
    }

    private void OnDisable()
    {
        if (burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
            burnCoroutine = null;
        }

        if (activeVisual != null)
        {
            Destroy(activeVisual);
            activeVisual = null;
        }
    }
}