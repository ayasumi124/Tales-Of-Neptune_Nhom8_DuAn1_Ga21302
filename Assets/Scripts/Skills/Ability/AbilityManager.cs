using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    public AbilityState clone = new AbilityState();

    public AbilityState dash = new AbilityState();

    public AbilityState fireball = new AbilityState();
    [System.Serializable]
    public class AbilityState
    {
        public bool unlocked;

        public float cooldown;

        public float duration;
    }
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateAbility(clone);
        UpdateAbility(dash);
        UpdateAbility(fireball);
    }

    public AbilityState GetState(AbilityType type)
{
    switch (type)
    {
        case AbilityType.Clone:
            return clone;

        case AbilityType.Dash:
            return dash;

        case AbilityType.Fireball:
            return fireball;
    }

    return null;
}
    void UpdateAbility(AbilityState state)
    {
        if (state.cooldown > 0)
            state.cooldown -= Time.deltaTime;

        if (state.duration > 0)
            state.duration -= Time.deltaTime;
    }

    public void UnlockAbility(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Clone:
                clone.unlocked = true;
                break;

            case AbilityType.Dash:
                dash.unlocked = true;
                break;

            case AbilityType.Fireball:
                fireball.unlocked = true;
                break;
        }
            Debug.Log("Clone unlocked = " + clone.unlocked);
    }
    public bool HasAbility(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Clone:
                return clone.unlocked;

            case AbilityType.Dash:
                return dash.unlocked;

            case AbilityType.Fireball:
                return fireball.unlocked;
        }

        return false;
    }
}