using UnityEngine;
using UnityEngine.UI;

public class HeartHeart : MonoBehaviour
{
    [Header("Heart Sprites")]
    [SerializeField]
    private Sprite fullHeart;

    [SerializeField]
    private Sprite halfHeart;

    [SerializeField]
    private Sprite emptyHeart;

    private Image heartImage;

    private void Awake()
    {
        heartImage =
            GetComponent<Image>();

        if (heartImage == null)
        {
            Debug.LogError(
                $"{gameObject.name} thiếu component Image."
            );
        }
    }

    public void SetHeartStatus(
        HeartStatus status)
    {
        if (heartImage == null)
        {
            heartImage =
                GetComponent<Image>();
        }

        if (heartImage == null)
            return;

        switch (status)
        {
            case HeartStatus.Full:
                heartImage.sprite =
                    fullHeart;
                break;

            case HeartStatus.Half:
                heartImage.sprite =
                    halfHeart;
                break;

            case HeartStatus.Empty:
                heartImage.sprite =
                    emptyHeart;
                break;
        }
    }
}

public enum HeartStatus
{
    Empty = 0,
    Half = 1,
    Full = 2
}