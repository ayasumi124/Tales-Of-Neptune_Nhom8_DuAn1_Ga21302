using System.Collections;
using UnityEngine;

public class FrostNova : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private IceGroundDamage iceGroundDamage;

    [SerializeField]
    private ParticleSystem[] loopingParticles;

    [Header("Timing")]
    [Min(0.1f)]
    [SerializeField]
    private float lifeTime = 6f;

    [Min(0.05f)]
    [SerializeField]
    private float fadeOutDuration = 0.5f;

    [Header("Cast Audio")]
    [SerializeField]
    private AudioClip castSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float castVolume = 0.8f;

    [Header("Impact Audio")]
    [SerializeField]
    private AudioClip impactSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float impactVolume = 1f;

    [Min(0f)]
    [SerializeField]
    private float impactSoundDelay = 0.18f;

    private GameObject owner;

    private float impactSoundTimer;

    private bool initialized;
    private bool impactSoundPlayed;

    private void Awake()
    {
        if (iceGroundDamage == null)
        {
            iceGroundDamage =
                GetComponentInChildren<
                    IceGroundDamage
                >();
        }

        if (loopingParticles == null ||
            loopingParticles.Length == 0)
        {
            loopingParticles =
                GetComponentsInChildren<
                    ParticleSystem
                >();
        }
    }

    public void Initialize(
        GameObject novaOwner)
    {
        owner = novaOwner;

        initialized = true;
        impactSoundPlayed = false;

        impactSoundTimer =
            Mathf.Max(
                0f,
                impactSoundDelay
            );

        PlayCastSound();

        StartCoroutine(
            LifeRoutine()
        );
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (impactSoundPlayed)
            return;

        impactSoundTimer -=
            Time.deltaTime;

        if (impactSoundTimer <= 0f)
        {
            impactSoundPlayed = true;

            PlayImpactSound();
        }
    }

    private IEnumerator LifeRoutine()
    {
        float safeLife =
            Mathf.Max(
                0.1f,
                lifeTime
            );

        float safeFade =
            Mathf.Clamp(
                fadeOutDuration,
                0.05f,
                safeLife
            );

        float normalDuration =
            safeLife -
            safeFade;

        if (normalDuration > 0f)
        {
            yield return new WaitForSeconds(
                normalDuration
            );
        }

        yield return StartCoroutine(
            FadeOutRoutine(
                safeFade
            )
        );

        Destroy(gameObject);
    }

    private IEnumerator FadeOutRoutine(
    float duration)
    {
        float timer = 0f;

        AudioSource groundAudio = null;
        float startGroundVolume = 0f;

        if (iceGroundDamage != null)
        {
            groundAudio =
                iceGroundDamage
                    .GetComponent<AudioSource>();

            if (groundAudio != null)
            {
                startGroundVolume =
                    groundAudio.volume;
            }
        }

        float[] originalEmissionRates =
            new float[
                loopingParticles.Length
            ];

        for (int i = 0;
             i < loopingParticles.Length;
             i++)
        {
            ParticleSystem ps =
                loopingParticles[i];

            if (ps == null)
                continue;

            ParticleSystem.EmissionModule
                emission =
                    ps.emission;

            originalEmissionRates[i] =
                emission.rateOverTimeMultiplier;
        }

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer /
                    duration
                );

            float fade =
                1f - t;

            // =========================
            // AUDIO
            // =========================

            if (groundAudio != null)
            {
                groundAudio.volume =
                    startGroundVolume *
                    fade;
            }

            // =========================
            // PARTICLES
            // =========================

            for (int i = 0;
                 i < loopingParticles.Length;
                 i++)
            {
                ParticleSystem ps =
                    loopingParticles[i];

                if (ps == null)
                    continue;

                // Giảm emission.
                ParticleSystem.EmissionModule
                    emission =
                        ps.emission;

                emission.rateOverTimeMultiplier =
                    originalEmissionRates[i] *
                    fade;

                // Giảm alpha của tất cả
                // particle đang tồn tại.
                ParticleSystem.Particle[] particles =
                    new ParticleSystem.Particle[
                        ps.main.maxParticles
                    ];

                int count =
                    ps.GetParticles(
                        particles
                    );

                for (int p = 0;
                     p < count;
                     p++)
                {
                    Color color =
                        particles[p]
                            .GetCurrentColor(ps);

                    color.a *= fade;

                    particles[p].startColor =
                        color;
                }

                ps.SetParticles(
                    particles,
                    count
                );
            }

            yield return null;
        }

        // =========================
        // STOP AUDIO
        // =========================

        if (groundAudio != null)
        {
            groundAudio.volume = 0f;
            groundAudio.Stop();
        }

        // =========================
        // STOP EMISSION
        // =========================

        for (int i = 0;
             i < loopingParticles.Length;
             i++)
        {
            ParticleSystem ps =
                loopingParticles[i];

            if (ps == null)
                continue;

            ParticleSystem.EmissionModule
                emission =
                    ps.emission;

            emission.rateOverTimeMultiplier =
                0f;

            ps.Stop(
                true,
                ParticleSystemStopBehavior
                    .StopEmitting
            );
        }

        /*
         * Cho hạt cuối có thời gian tan.
         */
        yield return new WaitForSeconds(
            0.25f
        );
    }
    private void PlayCastSound()
    {
        if (castSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    castSound,
                    castVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            castSound,
            transform.position,
            castVolume
        );
    }

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

    private void OnValidate()
    {
        lifeTime =
            Mathf.Max(
                0.1f,
                lifeTime
            );

        fadeOutDuration =
            Mathf.Clamp(
                fadeOutDuration,
                0.05f,
                lifeTime
            );

        impactSoundDelay =
            Mathf.Max(
                0f,
                impactSoundDelay
            );

        castVolume =
            Mathf.Clamp01(
                castVolume
            );

        impactVolume =
            Mathf.Clamp01(
                impactVolume
            );
    }
}