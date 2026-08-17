using UnityEngine;

public class Fairy : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 5f;
    public float followDistance = 0.7f;
    public float floatHeight = 0.2f;
    public float floatSpeed = 3f;

    private Players playerScript;
    private SpriteRenderer sprite;

    void Start()
    {
        playerScript = player.GetComponent<Players>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Vector3 target = player.position - (Vector3)(playerScript.LastDirection * followDistance);

        target.y += 0.25f; // Đặt vị trí y của fairy cố định ở 0.25f để tránh bị lệch khi player nhảy
        target.y += Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            followSpeed * Time.deltaTime);
        //lật mặt fairy theo hướng player mà vẫn giữ nguyên tranformsScale của x để không làm thay đổi kích thước của fairy
        sprite.flipX = playerScript.FacingDirection > 0;
    }
}