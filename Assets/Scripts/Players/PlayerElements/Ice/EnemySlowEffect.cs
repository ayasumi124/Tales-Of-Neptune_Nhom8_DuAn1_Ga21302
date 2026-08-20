using System.Collections;
using UnityEngine;

public class EnemySlowEffect : MonoBehaviour
{
    [Header("Runtime")]
    [Range(0.05f, 1f)]
    [SerializeField]
    private float currentSpeedMultiplier = 1f;

    [Header("VFX")]
    [SerializeField]
    private GameObject activeSlowEffect;

    [Header("Audio")]
    [SerializeField]
    private AudioSource slowAudioSource;

    private Coroutine slowCoroutine;

    public float SpeedMultiplier =>
        currentSpeedMultiplier;

    public bool IsSlowed =>
        currentSpeedMultiplier < 1f;

    public void ApplySlow(
        float multiplier,
        float duration,
        GameObject slowEffectPrefab,
        AudioClip slowSound = null,
        float slowVolume = 1f)
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

        SpawnSlowEffect(
            slowEffectPrefab
        );

        PlaySlowSound(
            slowSound,
            slowVolume
        );

        slowCoroutine =
            StartCoroutine(
                SlowRoutine(
                    duration
                )
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

    private void SpawnSlowEffect(
        GameObject slowEffectPrefab)
    {
        if (slowEffectPrefab == null)
            return;

        if (activeSlowEffect != null)
        {
            Destroy(
                activeSlowEffect
            );
        }

        activeSlowEffect =
            Instantiate(
                slowEffectPrefab,
                transform
            );

        activeSlowEffect.transform
            .localPosition =
            Vector3.zero;
    }

    private void PlaySlowSound(
        AudioClip clip,
        float volume)
    {
        if (clip == null)
            return;

        if (slowAudioSource == null)
        {
            slowAudioSource =
                gameObject.AddComponent<AudioSource>();

            slowAudioSource.playOnAwake =
                false;

            slowAudioSource.loop =
                true;

            slowAudioSource.spatialBlend =
                0f;
        }

        slowAudioSource.clip =
            clip;

        slowAudioSource.volume =
            Mathf.Clamp01(
                volume
            );

        if (!slowAudioSource.isPlaying)
        {
            slowAudioSource.Play();
        }
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

        if (activeSlowEffect != null)
        {
            Destroy(
                activeSlowEffect
            );

            activeSlowEffect = null;
        }

        if (slowAudioSource != null)
        {
            slowAudioSource.Stop();
            slowAudioSource.clip = null;
        }
    }

    private void OnDisable()
    {
        RemoveSlow();
    }
}