using UnityEngine;

[CreateAssetMenu(
    fileName = "New Element",
    menuName = "Element/Element Data"
)]
public class ElementData : ScriptableObject
{
    [Header("Element")]
    public ElementType elementType;

    public string elementName;

    [TextArea]
    public string description;
    

    public Sprite elementIcon;

    [Header("Mastery")]
    public int maxMastery = 100;

    [Header("Four Skills")]
    public ElementSkillData[] skills =
        new ElementSkillData[4];

    private void OnValidate()
    {
        if (skills == null ||
            skills.Length != 4)
        {
            System.Array.Resize(
                ref skills,
                4
            );
        }
    }
    
}