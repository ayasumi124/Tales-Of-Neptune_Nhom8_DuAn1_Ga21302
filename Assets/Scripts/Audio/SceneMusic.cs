using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip sceneMusic;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ChangeMusic(sceneMusic);
        }
    }
}