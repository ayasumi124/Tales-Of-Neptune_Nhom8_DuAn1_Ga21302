using UnityEngine;

public class CloneSkill : MonoBehaviour
{
    [Header("Clone")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private AbilityData skillData;

    [Header("Equipped Slot Keys")]
    [SerializeField] private KeyCode slot3Key = KeyCode.K;
    [SerializeField] private KeyCode slot4Key = KeyCode.F;

    private PlayerMana mana;

    private void Awake()
    {
        mana = GetComponent<PlayerMana>();
    }

    private void Update()
    {
        if (Time.timeScale <= 0f)
            return;

        if (AbilityManager.Instance == null)
            return;

        if (EquipmentManager.Instance == null)
            return;

        if (skillData == null ||
            clonePrefab == null)
        {
            return;
        }

        // Chưa mở khóa Clone thì không cho dùng.
        if (!AbilityManager.Instance.HasAbility(
                AbilityType.Clone))
        {
            return;
        }

        // Kiểm tra Clone đang được trang bị ở slot nào.
        int equippedSlot =
            EquipmentManager.Instance
                .GetEquippedSlot(skillData);

        KeyCode useKey;

        switch (equippedSlot)
        {
            case 3:
                useKey = slot3Key;
                break;

            case 4:
                useKey = slot4Key;
                break;

            default:
                // Clone chưa được trang bị.
                return;
        }

        if (!Input.GetKeyDown(useKey))
            return;

        TryUseClone();
    }

    private void TryUseClone()
    {
        AbilityManager.AbilityState state =
            AbilityManager.Instance.GetState(
                AbilityType.Clone
            );

        if (state == null)
        {
            Debug.LogError(
                "Không tìm thấy AbilityState của Clone."
            );

            return;
        }

        if (state.cooldown > 0f)
            return;

        if (mana == null)
        {
            Debug.LogError(
                "Player thiếu PlayerMana."
            );

            return;
        }

        if (!mana.UseMana(skillData.manaCost))
        {
            if (ManaUI.Instance != null)
            {
                ManaUI.Instance.ShowNoMana();
            }

            return;
        }

        SpawnClone();

        state.cooldown =
            skillData.cooldown;

        state.maxCooldown =
            skillData.cooldown;

        state.duration =
            skillData.duration;

        state.maxDuration =
            skillData.duration;
    }

    private void SpawnClone()
    {
        GameObject clone =
            Instantiate(
                clonePrefab,
                transform.position,
                Quaternion.identity
            );

        CloneFollow cloneFollow =
            clone.GetComponent<CloneFollow>();

        if (cloneFollow != null)
        {
            cloneFollow.player =
                transform;
        }
        else
        {
            Debug.LogError(
                "Clone Prefab thiếu CloneFollow."
            );
        }

        Destroy(
            clone,
            Mathf.Max(
                0.1f,
                skillData.duration
            )
        );

        Debug.Log("Đã triệu hồi Clone.");
    }
}