using System;
using UnityEngine;

public class ElementMasteryManager : MonoBehaviour
{
    public static ElementMasteryManager Instance
    {
        get;
        private set;
    }

    [Serializable]
    public class ElementMasteryState
    {
        public ElementType elementType;
        public int currentMastery;
    }

    [Header("Mastery")]
    [SerializeField]
    private ElementMasteryState fire =
        new ElementMasteryState
        {
            elementType = ElementType.Fire
        };

    [SerializeField]
    private ElementMasteryState ice =
        new ElementMasteryState
        {
            elementType = ElementType.Ice
        };

    [SerializeField]
    private ElementMasteryState thunder =
        new ElementMasteryState
        {
            elementType = ElementType.Thunder
        };

    public static event Action<ElementType, int>
        OnMasteryChanged;

    public static event Action<ElementSkillData>
        OnElementSkillUnlocked;

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public int GetMastery(
        ElementType elementType)
    {
        ElementMasteryState state =
            GetState(elementType);

        return state != null
            ? state.currentMastery
            : 0;
    }

    public void AddMastery(
        ElementType elementType,
        int amount)
    {
        if (amount <= 0)
            return;

        ElementMasteryState state =
            GetState(elementType);

        if (state == null)
            return;

        state.currentMastery += amount;

        OnMasteryChanged?.Invoke(
            elementType,
            state.currentMastery
        );

        Debug.Log(
            $"{elementType} Mastery: " +
            $"{state.currentMastery}"
        );
    }

    public bool CanUnlock(
        ElementSkillData skill)
    {
        if (skill == null)
            return false;

        return GetMastery(skill.elementType) >=
               skill.requiredMastery;
    }

    public bool UnlockSkill(
        ElementSkillData skill)
    {
        if (skill == null)
            return false;

        if (skill.unlocked)
            return true;

        if (!CanUnlock(skill))
        {
            Debug.Log(
                $"Chưa đủ Mastery để mở " +
                $"{skill.skillName}. Cần " +
                $"{skill.requiredMastery}."
            );

            return false;
        }

        skill.unlocked = true;

        OnElementSkillUnlocked?.Invoke(skill);

        Debug.Log(
            $"Đã mở khóa Element Skill: " +
            $"{skill.skillName}"
        );

        return true;
    }

    private ElementMasteryState GetState(
        ElementType elementType)
    {
        switch (elementType)
        {
            case ElementType.Fire:
                return fire;

            case ElementType.Ice:
                return ice;

            case ElementType.Thunder:
                return thunder;

            default:
                return null;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}