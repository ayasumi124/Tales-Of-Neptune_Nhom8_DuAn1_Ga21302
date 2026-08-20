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
    [Min(0f)]
    public float manaCost;

    [Tooltip("Thời gian hiệu ứng chính tồn tại.")]
    [Min(0f)]
    public float duration;

    [Min(0f)]
    public float cooldown;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Audio")]
    [Tooltip("Âm thanh phát khi cast skill.")]
    public AudioClip castSound;

    [Range(0f, 2f)]
    public float castVolume = 1f;

    [HideInInspector]
    public bool unlocked;
}