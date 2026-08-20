using UnityEngine;

public class Fairy : MonoBehaviour
{
    [Header("Follow")]
    public Transform player;

    public float followSpeed = 5f;
    public float followDistance = 0.7f;

    [Header("Floating")]
    public float floatHeight = 0.2f;
    public float floatSpeed = 3f;

    private Players playerScript;
    private SpriteRenderer sprite;

    private bool followEnabled = true;

    public bool FollowEnabled =>
        followEnabled;

    private void Start()
    {
        FindPlayer();

        sprite =
            GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!followEnabled)
            return;

        if (player == null ||
            playerScript == null)
        {
            FindPlayer();

            if (player == null ||
                playerScript == null)
            {
                return;
            }
        }

        Vector3 target =
            player.position -
            (Vector3)(
                playerScript.LastDirection *
                followDistance
            );

        target.y += 0.25f;

        target.y +=
            Mathf.Sin(
                Time.time *
                floatSpeed
            ) *
            floatHeight;

        transform.position =
            Vector3.Lerp(
                transform.position,
                target,
                followSpeed *
                Time.deltaTime
            );

        if (sprite != null)
        {
            sprite.flipX =
                playerScript.FacingDirection > 0;
        }
    }

    public void SetFollowEnabled(
        bool value)
    {
        followEnabled = value;
    }

    private void FindPlayer()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            player =
                GameManager.Instance.Player
                    .transform;

            playerScript =
                GameManager.Instance.Player
                    .GetComponent<Players>();
        }
    }
}