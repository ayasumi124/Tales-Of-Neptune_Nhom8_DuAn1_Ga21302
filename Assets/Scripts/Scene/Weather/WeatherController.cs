using UnityEngine;

public class WeatherController : MonoBehaviour
{
    [Header("Weather Particle")]
    [SerializeField]
    private ParticleSystem weatherParticle;

    [Header("Weather Audio")]
    [SerializeField]
    private AudioClip weatherSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float weatherVolume = 0.5f;

    private void Start()
    {
        // Phát particle nếu map này có
        if (weatherParticle != null)
        {
            weatherParticle.Play();
        }

        // Có particle + có sound thì phát weather
        if (weatherParticle != null &&
            weatherSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWeather(
                weatherSound,
                weatherVolume
            );
        }
        else
        {
            // Map không có weather
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.StopWeather();
            }
        }
    }
}