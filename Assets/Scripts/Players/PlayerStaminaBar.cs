using UnityEngine;
using UnityEngine.UI;

public class PlayerStaminaBar : MonoBehaviour
{
    public PlayerStamina stamina;

    public Image fill;

    public Vector3 offset = new Vector3(0,1.5f,0);

    Camera cam;

    void Start()
    {
        cam = Camera.main;

        PlayerStamina.OnStaminaChanged += UpdateBar;

        UpdateBar();

        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        PlayerStamina.OnStaminaChanged -= UpdateBar;
    }

    void LateUpdate()
    {
        transform.position =
            stamina.transform.position + offset;

        transform.rotation =
            cam.transform.rotation;
    }

    void UpdateBar()
    {
        fill.fillAmount =
            stamina.currentStamina / stamina.maxStamina;

        bool show =
            stamina.currentStamina < stamina.maxStamina;

        gameObject.SetActive(show);
    }
}