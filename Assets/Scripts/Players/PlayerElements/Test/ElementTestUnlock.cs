using UnityEngine;

public class ElementTestUnlock : MonoBehaviour
{
    [Header("Element Data")]
    [SerializeField]
    private ElementData fireData;

    [SerializeField]
    private ElementData iceData;

    [Header("Test")]
    [SerializeField]
    private ElementType testElement =
        ElementType.Ice;

    private void Start()
    {
        if (ElementEquipmentManager.Instance == null ||
            ElementMasteryManager.Instance == null)
        {
            Debug.LogError(
                "Thiếu ElementEquipmentManager hoặc " +
                "ElementMasteryManager."
            );

            return;
        }

        ElementData selectedData =
            GetSelectedElementData();

        if (selectedData == null)
        {
            Debug.LogError(
                $"Chưa gán ElementData cho {testElement}."
            );

            return;
        }

        // Equip element cần test.
        ElementEquipmentManager.Instance
            .EquipElement(
                selectedData
            );

        // Cho đủ Mastery để test.
        ElementMasteryManager.Instance
            .AddMastery(
                testElement,
                100
            );

        // Unlock toàn bộ skill đủ Mastery.
        if (selectedData.skills != null)
        {
            foreach (
                ElementSkillData skill
                in selectedData.skills)
            {
                if (skill == null)
                    continue;

                ElementMasteryManager.Instance
                    .UnlockSkill(
                        skill
                    );
            }
        }

        Debug.Log(
            $"TEST ELEMENT: {testElement} " +
            "đã được Equip + 100 Mastery."
        );
    }

    private ElementData GetSelectedElementData()
    {
        switch (testElement)
        {
            case ElementType.Fire:
                return fireData;

            case ElementType.Ice:
                return iceData;

            default:
                return null;
        }
    }
}