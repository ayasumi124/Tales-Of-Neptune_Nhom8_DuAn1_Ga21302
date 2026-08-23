using UnityEngine;

public class WeatherController : MonoBehaviour
{
    // =====================================================
    // PARTICLE
    // =====================================================

    [Header("Weather Particle")]
    [SerializeField]
    private ParticleSystem weatherParticle;


    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Weather Audio")]
    [SerializeField]
    private AudioClip weatherSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float weatherVolume = 0.5f;


    // =====================================================
    // START
    // =====================================================

    private void Start()
    {
        // -----------------------------
        // PARTICLE
        // -----------------------------

        if (weatherParticle != null)
        {
            weatherParticle.Play();
        }

        // -----------------------------
        // AUDIO
        // -----------------------------

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                $"{name}: Không tìm thấy AudioManager.",
                this
            );

            return;
        }

        /*
         * Scene này có weather sound.
         */
        if (weatherSound != null)
        {
            AudioManager.Instance.PlayWeather(
                weatherSound,
                weatherVolume
            );
        }
        else
        {
            /*
             * Có WeatherController nhưng không có sound
             * => đảm bảo weather cũ đã tắt.
             */
            AudioManager.Instance.StopWeather();
        }
    }


    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        /*
         * WeatherController thuộc scene bị disable/destroy
         * thì dừng sound của nó.
         *
         * Đây là lớp bảo hiểm ngoài việc AudioManager
         * tự cleanup khi đổi scene.
         */
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopWeather();
        }
    }
}