using UnityEngine;

[CreateAssetMenu(
    fileName = "New Element Skill",
    menuName = "Element/Element Skill Data"
)]
public class ElementSkillData : ScriptableObject
{
    [Header("Identity")]
    public string skillName;

    [TextArea]
    public string description;

    public Sprite icon;

    [Header("Element")]
    public ElementType elementType;

    [Header("Skill Index")]
    [Range(1, 4)]
    public int skillIndex = 1;

    [Header("Unlock")]
    [Min(0)]
    public int requiredMastery;

    [Header("Cost")]
    [Min(0f)]
    public float manaCost = 10f;

    [Header("Time")]
    [Tooltip(
        "Thời gian hiệu ứng chính tồn tại."
    )]
    [Min(0f)]
    public float duration = 1f;

    [Min(0f)]
    public float cooldown = 5f;

    [HideInInspector]
    public bool unlocked;

    private void OnValidate()
    {
        skillIndex =
            Mathf.Clamp(
                skillIndex,
                1,
                4
            );

        manaCost =
            Mathf.Max(
                0f,
                manaCost
            );

        duration =
            Mathf.Max(
                0f,
                duration
            );

        cooldown =
            Mathf.Max(
                0f,
                cooldown
            );

        requiredMastery =
            Mathf.Max(
                0,
                requiredMastery
            );
    }
}