using UnityEngine;
using UnityEngine.UI;

public class EnermyHealthBar : MonoBehaviour
{
    public Image fillImage;

    private EnermyHealth health;

    void Start()
    {
        health = GetComponentInParent<EnermyHealth>();
    }

    void Update()
    {
        fillImage.fillAmount =
            (float)health.currentHealth /
            health.maxHealth;
    }
}