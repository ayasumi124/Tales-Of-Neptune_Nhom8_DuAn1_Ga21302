using UnityEngine;

public class TemporaryEffect : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 0.8f;

    private void Start()
    {
        Destroy(
            gameObject,
            lifeTime
        );
    }
}