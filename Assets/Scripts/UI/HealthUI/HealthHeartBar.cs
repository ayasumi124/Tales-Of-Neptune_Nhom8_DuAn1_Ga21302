using System.Collections.Generic;
using UnityEngine;

public class HealthHeartBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject heartPrefab;

    [SerializeField]
    private Health playerHealth;

    private readonly List<HeartHeart> hearts =
        new List<HeartHeart>();

    private void Awake()
    {
        FindPlayerHealth();
    }

    private void OnEnable()
    {
        Health.onPlayerDamaged +=
            DrawHeart;

        Health.onPlayerHealed +=
            DrawHeart;

        Health.onMaxHealthChanged +=
            DrawHeart;

        Health.onPlayerDeath +=
            DrawHeart;
    }

    private void Start()
    {
        FindPlayerHealth();
        DrawHeart();
    }

    private void OnDisable()
    {
        Health.onPlayerDamaged -=
            DrawHeart;

        Health.onPlayerHealed -=
            DrawHeart;

        Health.onMaxHealthChanged -=
            DrawHeart;

        Health.onPlayerDeath -=
            DrawHeart;
    }

    private void FindPlayerHealth()
    {
        if (playerHealth != null)
            return;

        if (GameManager.Instance != null &&
            GameManager.Instance.Player != null)
        {
            playerHealth =
                GameManager.Instance.Player
                    .GetComponent<Health>();
        }

        if (playerHealth == null)
        {
            playerHealth =
                FindFirstObjectByType<Health>();
        }

        if (playerHealth == null)
        {
            Debug.LogError(
                "HealthHeartBar không tìm thấy Health của Player."
            );
        }
    }

    public void DrawHeart()
    {
        if (playerHealth == null)
            FindPlayerHealth();

        if (playerHealth == null ||
            heartPrefab == null)
        {
            return;
        }

        ClearHearts();

        /*
         * 2 HP = 1 trái tim.
         * Ví dụ Max HP = 7 thì cần 4 tim.
         */
        int heartsToCreate =
            Mathf.CeilToInt(
                playerHealth.maxHealth / 2f
            );

        for (int i = 0;
             i < heartsToCreate;
             i++)
        {
            CreateEmptyHeart();
        }

        for (int i = 0;
             i < hearts.Count;
             i++)
        {
            float remainingHealth =
                playerHealth.currentHealth -
                i * 2f;

            HeartStatus status;

            if (remainingHealth >= 2f)
            {
                status =
                    HeartStatus.Full;
            }
            else if (remainingHealth >= 1f)
            {
                status =
                    HeartStatus.Half;
            }
            else
            {
                status =
                    HeartStatus.Empty;
            }

            hearts[i].SetHeartStatus(
                status
            );
        }
    }

    private void CreateEmptyHeart()
    {
        GameObject newHeart =
            Instantiate(
                heartPrefab,
                transform
            );

        HeartHeart heartComponent =
            newHeart.GetComponent<
                HeartHeart
            >();

        if (heartComponent == null)
        {
            Debug.LogError(
                "Heart Prefab thiếu HeartHeart."
            );

            Destroy(newHeart);
            return;
        }

        heartComponent.SetHeartStatus(
            HeartStatus.Empty
        );

        hearts.Add(
            heartComponent
        );
    }

    private void ClearHearts()
    {
        for (int i =
                 transform.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                transform
                    .GetChild(i)
                    .gameObject
            );
        }

        hearts.Clear();
    }
}