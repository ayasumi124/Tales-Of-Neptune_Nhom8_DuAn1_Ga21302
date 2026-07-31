using UnityEngine;

public class ElementTestUnlock : MonoBehaviour
{
    [SerializeField] private ElementData fireData;

    private void Start()
    {
        if (ElementEquipmentManager.Instance == null ||
            ElementMasteryManager.Instance == null)
        {
            Debug.LogError(
                "Thiếu ElementEquipmentManager hoặc ElementMasteryManager."
            );
            return;
        }

        if (fireData == null)
            return;

        // Trang bị Fire vào Slot4.
        ElementEquipmentManager.Instance
            .EquipElement(fireData);

        // Tạm cho đủ Mastery để test cả 4 skill.
        ElementMasteryManager.Instance.AddMastery(
            ElementType.Fire,
            100
        );
    }
}