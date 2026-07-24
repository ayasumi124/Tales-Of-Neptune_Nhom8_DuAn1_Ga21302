using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    public DamagePopup popupPrefab;

    void Awake()
    {
        Instance = this;
    }

    public void ShowDamage(
    int damage,
    Vector3 position,
    bool critical = false)
    {
        Debug.Log("Spawn Damage Popup");

        DamagePopup popup = Instantiate(
            popupPrefab,
            position,
            Quaternion.identity);

        popup.SetDamage(damage, critical);
    }
}