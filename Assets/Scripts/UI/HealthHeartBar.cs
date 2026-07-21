using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HealthHeartBar : MonoBehaviour
{
    public GameObject heartPrefab;
    public Health playerHealth;
    List<HeartHeart> hearts = new List<HeartHeart>();

    void OnEnable()
    {
        Health.onPlayerDamaged += DrawHeart;
        Health.onPlayerDeath += DrawHeart;
    }

    void OnDisable()
    {
        Health.onPlayerDamaged -= DrawHeart;
        Health.onPlayerDeath -= DrawHeart;
    }
    public void DrawHeart()
    {
        ClearHearts();
       float maxHeartRemainder = playerHealth.maxHealth % 2;
        int heartToMake = (int) ((playerHealth.maxHealth / 2) + (maxHeartRemainder));
        for (int i = 0; i < heartToMake; i++)
        {
            CreateEmptyHearts();
        }

        for (int i = 0; i < hearts.Count; i++)
        {
            int heartStatusRemainder = (int)Mathf.Clamp(playerHealth.currentHealth - (i * 2), 0, 2);
            hearts[i].SetHeartStatus((HeartStatus)heartStatusRemainder);
        }
    }

    public void CreateEmptyHearts()
    {
 GameObject newHeart = Instantiate(heartPrefab, transform);

        HeartHeart heartComponent = newHeart.GetComponent<HeartHeart>();
        heartComponent.SetHeartStatus(HeartStatus.Empty);
        hearts.Add(heartComponent);
    }
    public void ClearHearts()
    {
        foreach (Transform t in transform)
        {
            Destroy(t.gameObject);
        }
        hearts = new List<HeartHeart>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DrawHeart();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
