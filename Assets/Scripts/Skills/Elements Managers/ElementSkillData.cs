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

    [Header("Skill Settings")]
    public float manaCost;
    public float cooldown;

    [HideInInspector]
    public bool unlocked;
}