using UnityEngine;
using UnityEngine.UI;

public class CloneHealthBar : MonoBehaviour
{
    public Image fillImage;

    CloneHealth health;

    void Start()
    {
        health = GetComponentInParent<CloneHealth>();
    }

    void Update()
    {
        fillImage.fillAmount =
            (float)health.currentHealth /
            health.maxHealth;
    }
}