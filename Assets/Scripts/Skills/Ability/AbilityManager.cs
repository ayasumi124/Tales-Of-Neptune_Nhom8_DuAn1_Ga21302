using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    public AbilityState clone = new AbilityState();

    public AbilityState dash = new AbilityState();

    public AbilityState fireball = new AbilityState();

    public AbilityState attack = new AbilityState();
    [System.Serializable]
    public class AbilityState
    {
        public bool unlocked;
        public bool equipped;

        public float cooldown;

        public float maxCooldown;

        public float duration;
    }
    void InitializeDefaultAbilities()
    {
        attack.unlocked = true;
        dash.unlocked = true;

        clone.unlocked = false;
        fireball.unlocked = false;
    }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDefaultAbilities();
    }

    void Update()
    {
        UpdateAbility(attack);
        UpdateAbility(clone);
        UpdateAbility(dash);
        UpdateAbility(fireball);
    }

    public AbilityState GetState(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Attack:
                return attack;
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
        {
            state.cooldown -= Time.deltaTime;

            if (state.cooldown < 0)
                state.cooldown = 0;
        }

        if (state.duration > 0)
        {
            state.duration -= Time.deltaTime;

            if (state.duration < 0)
                state.duration = 0;
        }
    }

    public void UnlockAbility(AbilityType type)
    {
        switch (type)
        {
            case AbilityType.Attack:
                attack.unlocked = true;
                break;

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
            case AbilityType.Attack:
                return attack.unlocked;
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