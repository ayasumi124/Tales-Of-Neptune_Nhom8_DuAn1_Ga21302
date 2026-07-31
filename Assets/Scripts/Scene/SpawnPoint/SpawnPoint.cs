using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn ID")]
    [SerializeField] private string spawnID;

    [Header("Portal Exit")]
    [Tooltip(
        "Điểm Player xuất hiện ban đầu, thường đặt ở tâm portal."
    )]
    [SerializeField] private Transform exitStartPoint;

    [Tooltip("Tốc độ Player bước từ portal tới SpawnPoint.")]
    [SerializeField] private float exitSpeed = 2f;

    public string SpawnID => spawnID;

    public Transform ExitStartPoint => exitStartPoint;

    public float ExitSpeed =>
        Mathf.Max(0.01f, exitSpeed);

    public bool HasPortalExit =>
        exitStartPoint != null;

    public Vector3 ExitStartPosition
    {
        get
        {
            if (exitStartPoint != null)
                return exitStartPoint.position;

            return transform.position;
        }
    }

    public Vector3 FinalPosition =>
        transform.position;

    public Vector2 ExitDirection
    {
        get
        {
            Vector2 direction =
                FinalPosition -
                ExitStartPosition;

            if (direction.sqrMagnitude <= 0.001f)
                return Vector2.zero;

            return direction.normalized;
        }
    }

    public float ExitDistance
    {
        get
        {
            return Vector2.Distance(
                ExitStartPosition,
                FinalPosition
            );
        }
    }

    public float ExitDuration
    {
        get
        {
            if (ExitDistance <= 0.001f)
                return 0f;

            return ExitDistance / ExitSpeed;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (exitStartPoint == null)
            return;

        Gizmos.DrawWireSphere(
            exitStartPoint.position,
            0.15f
        );

        Gizmos.DrawWireSphere(
            transform.position,
            0.15f
        );

        Gizmos.DrawLine(
            exitStartPoint.position,
            transform.position
        );
    }
}