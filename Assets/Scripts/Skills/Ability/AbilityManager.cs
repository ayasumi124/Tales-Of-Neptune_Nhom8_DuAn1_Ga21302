using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    public bool hasClone;
    public bool hasDash;
    public bool hasFireball;

    void Awake()
    {
        Instance = this;
    }

    public void UnlockAbility(AbilityData data)
{
    switch (data.type)
    {
        case AbilityType.Clone:
            hasClone = true;
            break;

        case AbilityType.Dash:
            hasDash = true;
            break;

        case AbilityType.Fireball:
            hasFireball = true;
            break;
    }
}
    public bool HasAbility(AbilityType type)
{
    switch (type)
    {
        case AbilityType.Clone:
            return hasClone;

        case AbilityType.Dash:
            return hasDash;

        case AbilityType.Fireball:
            return hasFireball;
    }

    return false;
}
}