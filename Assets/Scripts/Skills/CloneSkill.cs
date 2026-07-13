using UnityEngine;
using TMPro;

public class CloneSkill : MonoBehaviour
{


    public float skillCooldown = 60f;
    public float cloneLifeTime = 30f;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI durationText;

    private float cooldownTimer = 0f;
    private float durationTimer = 0f;
    public GameObject clonePrefab;


    void Update()
    {
        // Cooldown
        if (cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;

        // Clone tồn tại
        if (durationTimer > 0)
            durationTimer -= Time.deltaTime;

        // UI Cooldown
        if (cooldownTimer > 0)
            cooldownText.text = "Cooldown: " + cooldownTimer.ToString("F1") + "s";
        else
            cooldownText.text = "Cooldown: Ready";

        // UI Duration
        if (durationTimer > 0)
            durationText.text = "Clone: " + durationTimer.ToString("F1") + "s";
        else
            durationText.text = "Clone: --";

        // Dùng skill
        if (Input.GetKeyDown(KeyCode.K) && cooldownTimer <= 0)
        {
            GameObject clone = Instantiate(
                clonePrefab,
                transform.position,
                Quaternion.identity);

            clone.GetComponent<CloneFollow>().player = transform;

            Destroy(clone, cloneLifeTime);

            cooldownTimer = skillCooldown;
            durationTimer = cloneLifeTime;
        }
    }
}