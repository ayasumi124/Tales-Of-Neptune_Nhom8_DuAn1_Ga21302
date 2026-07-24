using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability/Ability Data")]
public class AbilityData : ScriptableObject
{
    public AbilityType type;

    [Header("Skill")]
    public float manaCost;
    public float cooldown;

    public string skillName;

    [TextArea]
    public string description;

    public Sprite icon;
}