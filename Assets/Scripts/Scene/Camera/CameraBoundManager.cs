using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class CameraBoundsManager : MonoBehaviour
{
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        confiner = GetComponent<CinemachineConfiner2D>();

        if (confiner == null)
        {
            Debug.LogError("Không tìm thấy CinemachineConfiner2D.");
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        AssignBounds();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignBounds();
    }

    private void AssignBounds()
    {
        if (confiner == null)
            return;

        GameObject boundsObject = GameObject.FindGameObjectWithTag("CameraBounds");

        if (boundsObject == null)
        {
            Debug.LogWarning(
                $"Scene {SceneManager.GetActiveScene().name} không có object tag CameraBounds."
            );

            confiner.BoundingShape2D = null;
            return;
        }

        Collider2D boundsCollider = boundsObject.GetComponent<Collider2D>();

        if (boundsCollider == null)
        {
            Debug.LogError(
                $"{boundsObject.name} không có Collider2D."
            );
            return;
        }

        confiner.BoundingShape2D = boundsCollider;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log(
            $"Đã gán Camera Bounds: {boundsObject.name}"
        );
    }
}