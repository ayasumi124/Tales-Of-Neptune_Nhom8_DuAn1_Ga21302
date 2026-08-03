using System;
using UnityEngine;

public class EnermyItemDrop : MonoBehaviour
{
    [Serializable]
    public class LootEntry
    {
        [Header("Item")]
        public ItemData itemData;

        [Header("Drop Chance")]
        [Range(0f, 100f)]
        public float dropChance = 30f;

        [Header("Quantity")]
        [Min(1)]
        public int minQuantity = 1;

        [Min(1)]
        public int maxQuantity = 1;
    }

    [Header("World Pickup")]
    [SerializeField]
    private WorldItemPickup worldItemPickupPrefab;

    [Header("Loot Table")]
    [SerializeField]
    private LootEntry[] lootEntries;

    [Header("Drop Position")]
    [SerializeField]
    private Transform dropPoint;

    [SerializeField]
    private float scatterRadius = 0.4f;

    private bool hasDropped;

    public void DropItems()
    {
        if (hasDropped)
            return;

        hasDropped = true;

        if (worldItemPickupPrefab == null)
        {
            Debug.LogError(
                $"{name}: chưa gán WorldItemPickup Prefab."
            );

            return;
        }

        if (lootEntries == null ||
            lootEntries.Length == 0)
        {
            return;
        }

        Vector3 center =
            dropPoint != null
                ? dropPoint.position
                : transform.position;

        foreach (LootEntry entry in lootEntries)
        {
            if (entry == null ||
                entry.itemData == null)
            {
                continue;
            }

            float chance =
                Mathf.Clamp(
                    entry.dropChance,
                    0f,
                    100f
                );

            float randomValue =
                UnityEngine.Random.Range(
                    0f,
                    100f
                );

            if (randomValue > chance)
                continue;

            int min =
                Mathf.Max(
                    1,
                    entry.minQuantity
                );

            int max =
                Mathf.Max(
                    min,
                    entry.maxQuantity
                );

            int quantity =
                UnityEngine.Random.Range(
                    min,
                    max + 1
                );

            Vector2 offset =
                UnityEngine.Random
                    .insideUnitCircle *
                Mathf.Max(
                    0f,
                    scatterRadius
                );

            Vector3 spawnPosition =
                center +
                new Vector3(
                    offset.x,
                    offset.y,
                    0f
                );

            WorldItemPickup pickup =
    Instantiate(
        worldItemPickupPrefab,
        spawnPosition,
        Quaternion.identity);

            pickup.Setup(
                entry.itemData,
                quantity);
        }
    }

    public void ResetDropState()
    {
        hasDropped = false;
    }
}