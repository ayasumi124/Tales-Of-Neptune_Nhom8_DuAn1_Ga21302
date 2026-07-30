using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    [Header("Scene Music")]
    [SerializeField] private AudioClip sceneMusic;

    [Tooltip(
        "Tắt nhạc nếu scene này không dùng nhạc."
    )]
    [SerializeField] private bool stopMusic;

    private void Start()
    {
        ApplySceneMusic();
    }

    private void ApplySceneMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy AudioManager."
            );

            return;
        }

        if (stopMusic)
        {
            AudioManager.Instance.StopMusic();
            return;
        }

        if (sceneMusic == null)
        {
            Debug.LogWarning(
                $"{name}: Scene Music chưa được gán."
            );

            return;
        }

        AudioManager.Instance.ChangeMusic(
            sceneMusic
        );
    }
}