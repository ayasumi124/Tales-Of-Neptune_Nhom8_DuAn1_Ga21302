using UnityEngine;

public class RoomClearChestSpawner :
    MonoBehaviour
{
    [Header("Room Enemies")]
    [SerializeField]
    private Transform enemyContainer;

    [Header("Reward")]
    [SerializeField]
    private GameObject keyChest;

    private EnermyHealth[] enemies;

    private bool roomCleared;

    private void Start()
    {
        if (enemyContainer != null)
        {
            enemies =
                enemyContainer
                    .GetComponentsInChildren<
                        EnermyHealth
                    >(true);
        }

        if (keyChest != null)
        {
            keyChest.SetActive(false);
        }
    }

    private void Update()
    {
        if (roomCleared)
            return;

        if (!AreAllEnemiesDead())
            return;

        RoomCleared();
    }

    private bool AreAllEnemiesDead()
    {
        if (enemies == null ||
            enemies.Length == 0)
        {
            return false;
        }

        foreach (
            EnermyHealth enemy
            in enemies)
        {
            /*
             * Enemy đã Destroy cũng xem như chết.
             */
            if (enemy != null &&
                !enemy.IsDead)
            {
                return false;
            }
        }

        return true;
    }

    private void RoomCleared()
    {
        roomCleared = true;

        Debug.Log(
            $"{name}: Room Cleared."
        );

        if (keyChest != null)
        {
            keyChest.SetActive(true);
        }
    }
}