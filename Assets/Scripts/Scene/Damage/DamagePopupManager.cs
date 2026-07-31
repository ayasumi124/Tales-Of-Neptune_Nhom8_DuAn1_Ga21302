using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance { get; private set; }

    [Header("Popup")]
    [SerializeField] private DamagePopup popupPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "Phát hiện DamagePopupManager bị trùng. Xóa object: "
                + gameObject.name
            );

            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void ShowDamage(
        int damage,
        Vector3 position,
        bool critical = false)
    {
        if (popupPrefab == null)
        {
            Debug.LogError(
                "DamagePopupManager chưa được gán Popup Prefab."
            );
            return;
        }

        DamagePopup popup = Instantiate(
            popupPrefab,
            position,
            Quaternion.identity
        );

        if (popup == null)
        {
            Debug.LogError("Không thể tạo DamagePopup.");
            return;
        }

        popup.SetDamage(damage, critical);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}