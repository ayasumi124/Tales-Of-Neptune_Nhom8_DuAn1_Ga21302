using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private string spawnID;

    public string SpawnID => spawnID;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(spawnID))
        {
            spawnID = gameObject.name;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            0.3f
        );

        Gizmos.DrawLine(
            transform.position,
            transform.position + transform.up * 0.7f
        );
    }
}