using System.Collections;
using UnityEngine;

public class PlayerItemUser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health health;
    [SerializeField] private PlayerMana mana;
    [SerializeField] private Animator animator;

    [Header("Animation")]
    [SerializeField] private string useTrigger = "UseItem";

    private bool isUsingItem;

    public bool IsUsingItem => isUsingItem;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();

        if (mana == null)
            mana = GetComponent<PlayerMana>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public bool TryUse(ItemData item)
    {
        if (item == null)
            return false;

        if (!item.Usable)
            return false;

        if (isUsingItem)
            return false;

        if (!CanUseItem(item))
            return false;

        StartCoroutine(
            UseRoutine(item)
        );

        return true;
    }

    private bool CanUseItem(ItemData item)
    {
        switch (item.EffectType)
        {
            case ItemEffectType.RestoreHealth:
                if (health == null)
                {
                    Debug.LogError(
                        "PlayerItemUser không tìm thấy Health."
                    );

                    return false;
                }

                if (health.IsHealthFull())
                {
                    Debug.Log("Máu đã đầy.");
                    return false;
                }

                return true;

            case ItemEffectType.RestoreMana:
                if (mana == null)
                {
                    Debug.LogError(
                        "PlayerItemUser không tìm thấy PlayerMana."
                    );

                    return false;
                }

                if (mana.IsManaFull())
                {
                    Debug.Log("Mana đã đầy.");
                    return false;
                }

                return true;

            case ItemEffectType.IncreaseMaxHealth:
                return health != null;
        }

        return false;
    }

    private IEnumerator UseRoutine(
        ItemData item)
    {
        isUsingItem = true;

        if (animator != null)
        {
            animator.ResetTrigger(
                useTrigger
            );

            animator.SetTrigger(
                useTrigger
            );
        }

        float effectDelay =
            Mathf.Clamp(
                item.EffectDelay,
                0f,
                item.UseDuration
            );

        if (effectDelay > 0f)
        {
            yield return new WaitForSeconds(
                effectDelay
            );
        }

        bool success =
            ApplyEffect(item);

        Debug.Log(
            $"Dùng {item.ItemName}. " +
            $"Effect = {item.EffectType}. " +
            $"Thành công = {success}"
        );

        if (success &&
            item.UseSound != null &&
            AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                item.UseSound
            );
        }

        float remainingTime =
            Mathf.Max(
                0f,
                item.UseDuration -
                effectDelay
            );

        if (remainingTime > 0f)
        {
            yield return new WaitForSeconds(
                remainingTime
            );
        }

        isUsingItem = false;
    }

    private bool ApplyEffect(
        ItemData item)
    {
        switch (item.EffectType)
        {
            case ItemEffectType.RestoreHealth:
                return health != null &&
                       health.Heal(
                           item.EffectValue
                       );

            case ItemEffectType.RestoreMana:
                return mana != null &&
                       mana.RestoreMana(
                           item.EffectValue
                       );

            case ItemEffectType.IncreaseMaxHealth:
                return health != null &&
                       health.IncreaseMaxHealth(
                           Mathf.RoundToInt(
                               item.EffectValue
                           ),
                           true
                       );
        }

        return false;
    }
}