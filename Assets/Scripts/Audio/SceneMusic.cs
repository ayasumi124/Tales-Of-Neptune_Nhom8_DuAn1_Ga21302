using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip sceneMusic;

    void Start()
    {
        if(AudioManager.Instance != null)
            AudioManager.Instance.ChangeMusic(sceneMusic);
    }
}