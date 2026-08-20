using System.Collections.Generic;
using UnityEngine;

public class EnemyFreezeEffect : MonoBehaviour
{
    // =====================================================
    // FREEZE
    // =====================================================

    [Header("Freeze")]
    [Min(0.1f)]
    [SerializeField]
    private float defaultFreezeDuration = 2f;

    // =====================================================
    // VFX
    // =====================================================

    [Header("Freeze VFX")]
    [SerializeField]
    private GameObject defaultFreezeVFXPrefab;

    [SerializeField]
    private Vector3 freezeVFXOffset =
        Vector3.zero;

    private Rigidbody2D rb;

    private RigidbodyConstraints2D
        originalConstraints;

    private bool constraintsStored;

    // =====================================================
    // AUDIO
    // =====================================================

    [Header("Freeze Audio")]
    [SerializeField]
    private AudioClip freezeSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float freezeVolume = 0.8f;

    [SerializeField]
    private AudioClip unfreezeSound;

    [Range(0f, 1f)]
    [SerializeField]
    private float unfreezeVolume = 0.6f;

    // =====================================================
    // MOVEMENT
    // =====================================================

    private EnermyMovement enemyMovement;
    private BossMovement bossMovement;

    // =====================================================
    // ATTACK
    // =====================================================

    private EnermyAttack normalAttack;

    /*
     * Các attack kế thừa EnermyAttackBase:
     *
     * MushroomLeapAttack
     * FlyingSlamAttack
     * ...
     */
    private EnermyAttackBase specialAttack;

    /*
     * Boss có attack controller riêng.
     */
    private MinotaurosBossAttack bossAttack;

    // =====================================================
    // STATE
    // =====================================================

    private GameObject activeFreezeVFX;

    private float freezeTimer;

    private bool frozen;

    /*
     * Ghi lại component nào vốn được bật
     * trước khi Freeze.
     */
    private bool normalAttackWasEnabled;
    private bool specialAttackWasEnabled;
    private bool bossAttackWasEnabled;

    public bool IsFrozen =>
        frozen;

    public float RemainingFreezeTime =>
        Mathf.Max(
            0f,
            freezeTimer
        );

    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (!frozen)
            return;

        freezeTimer -=
            Time.deltaTime;

        if (freezeTimer <= 0f)
        {
            EndFreeze();
        }
    }
    private void FixedUpdate()
    {
        if (!frozen)
            return;

        /*
         * Khóa cứng Rigidbody trong toàn bộ
         * thời gian Freeze.
         */
        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;
        }

        /*
         * Một số Movement script có thể
         * tiếp tục tính velocity.
         * StopImmediately mỗi physics frame
         * để không cho AI kéo Enemy đi.
         */
        if (enemyMovement != null)
        {
            enemyMovement.StopImmediately();
        }

        if (bossMovement != null)
        {
            bossMovement.StopImmediately();
        }
    }

    // =====================================================
    // CACHE
    // =====================================================

    private void CacheComponents()
    {
        if (enemyMovement == null)
        {
            enemyMovement =
                GetComponent<EnermyMovement>();
        }
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (bossMovement == null)
        {
            bossMovement =
                GetComponent<BossMovement>();
        }

        if (normalAttack == null)
        {
            normalAttack =
                GetComponent<EnermyAttack>();
        }

        if (specialAttack == null)
        {
            specialAttack =
                GetComponent<EnermyAttackBase>();
        }

        if (bossAttack == null)
        {
            bossAttack =
                GetComponent<
                    MinotaurosBossAttack
                >();
        }
    }

    // =====================================================
    // APPLY FREEZE
    // =====================================================

    public void ApplyFreeze(
        float duration,
        GameObject freezeVFXPrefab = null)
    {
        CacheComponents();

        float finalDuration =
            duration > 0f
                ? duration
                : defaultFreezeDuration;

        finalDuration =
            Mathf.Max(
                0.1f,
                finalDuration
            );

        /*
         * Đang Freeze rồi thì không tạo
         * thêm cục băng.
         *
         * Chỉ refresh thời gian.
         */
        if (frozen)
        {
            freezeTimer =
                Mathf.Max(
                    freezeTimer,
                    finalDuration
                );

            return;
        }

        frozen = true;

        freezeTimer =
            finalDuration;

        // =================================================
        // CANCEL ATTACK TRƯỚC
        // =================================================

        CancelCurrentAttack();

        // =================================================
        // LOCK MOVEMENT
        // =================================================

        LockMovement();

        // =================================================
        // DISABLE ATTACK
        // =================================================

        DisableAttackScripts();

        // =================================================
        // VFX
        // =================================================

        SpawnFreezeVFX(
            freezeVFXPrefab
        );

        // =================================================
        // AUDIO
        // =================================================

        PlayFreezeSound();

        Debug.Log(
            $"{name} bị FREEZE " +
            $"{finalDuration:F1}s."
        );
    }

    // =====================================================
    // CANCEL ATTACK
    // =====================================================

    private void CancelCurrentAttack()
    {
        /*
         * Enemy thường.
         */
        if (normalAttack != null)
        {
            normalAttack.CancelAttack();
        }

        /*
         * Các enemy dùng EnermyAttackBase
         * như Mushroom/Flying.
         */
        if (specialAttack != null)
        {
            specialAttack.CancelAttack();
        }

        /*
         * Boss.
         *
         * Hủy cả Normal / Ground Slam /
         * Jump Slam đang chạy.
         */
        if (bossAttack != null)
        {
            bossAttack.CancelAction();
        }
    }

    // =====================================================
    // LOCK MOVEMENT
    // =====================================================

    private void LockMovement()
    {
        if (enemyMovement != null)
        {
            enemyMovement.StopImmediately();
        }

        if (bossMovement != null)
        {
            bossMovement.StopImmediately();
        }

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            /*
             * Lưu Constraints ban đầu để sau
             * Freeze trả lại chính xác.
             */
            if (!constraintsStored)
            {
                originalConstraints =
                    rb.constraints;

                constraintsStored = true;
            }

            /*
             * Đóng băng tuyệt đối vị trí.
             */
            rb.constraints =
                originalConstraints |
                RigidbodyConstraints2D
                    .FreezePositionX |
                RigidbodyConstraints2D
                    .FreezePositionY;
        }
    }

    // =====================================================
    // ATTACK DISABLE
    // =====================================================

    private void DisableAttackScripts()
    {
        if (normalAttack != null)
        {
            normalAttackWasEnabled =
                normalAttack.enabled;

            normalAttack.enabled =
                false;
        }

        if (specialAttack != null)
        {
            specialAttackWasEnabled =
                specialAttack.enabled;

            specialAttack.enabled =
                false;
        }

        if (bossAttack != null)
        {
            bossAttackWasEnabled =
                bossAttack.enabled;

            bossAttack.enabled =
                false;
        }
    }

    // =====================================================
    // FREEZE VFX
    // =====================================================

    private void SpawnFreezeVFX(
        GameObject overrideVFX)
    {
        GameObject prefab =
            overrideVFX != null
                ? overrideVFX
                : defaultFreezeVFXPrefab;

        if (prefab == null)
            return;

        if (activeFreezeVFX != null)
        {
            Destroy(
                activeFreezeVFX
            );
        }

        activeFreezeVFX =
            Instantiate(
                prefab,
                transform
            );

        activeFreezeVFX.transform
            .localPosition =
            freezeVFXOffset;

        activeFreezeVFX.transform
            .localRotation =
            Quaternion.identity;
    }

    // =====================================================
    // AUDIO
    // =====================================================

    private void PlayFreezeSound()
    {
        if (freezeSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    freezeSound,
                    freezeVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            freezeSound,
            transform.position,
            freezeVolume
        );
    }

    private void PlayUnfreezeSound()
    {
        if (unfreezeSound == null)
            return;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance
                .PlayElementSkillSFX(
                    unfreezeSound,
                    unfreezeVolume
                );

            return;
        }

        AudioSource.PlayClipAtPoint(
            unfreezeSound,
            transform.position,
            unfreezeVolume
        );
    }

    // =====================================================
    // END FREEZE
    // =====================================================

    public void EndFreeze()
    {
        if (!frozen)
            return;

        frozen = false;
        freezeTimer = 0f;

        // =================================================
        // REMOVE VFX
        // =================================================

        if (activeFreezeVFX != null)
        {
            Destroy(
                activeFreezeVFX
            );

            activeFreezeVFX = null;
        }

        // =================================================
        // ENABLE ATTACK
        // =================================================

        RestoreAttackScripts();

        // =================================================
        // RESUME AI
        // =================================================

        ResumeMovement();

        PlayUnfreezeSound();

        Debug.Log(
            $"{name} hết FREEZE."
        );
    }

    // =====================================================
    // RESTORE ATTACK
    // =====================================================

    private void RestoreAttackScripts()
    {
        if (normalAttack != null &&
            normalAttackWasEnabled)
        {
            normalAttack.enabled =
                true;
        }

        if (specialAttack != null &&
            specialAttackWasEnabled)
        {
            specialAttack.enabled =
                true;
        }

        if (bossAttack != null &&
            bossAttackWasEnabled)
        {
            bossAttack.enabled =
                true;
        }
    }

    // =====================================================
    // RESUME MOVEMENT
    // =====================================================

    private void ResumeMovement()
    {
        /*
         * Mở khóa Rigidbody TRƯỚC.
         */
        if (rb != null &&
            constraintsStored)
        {
            rb.constraints =
                originalConstraints;

            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            constraintsStored = false;
        }

        /*
         * Sau đó mới cho AI chạy lại.
         */
        if (enemyMovement != null &&
            enemyMovement.enabled &&
            gameObject.activeInHierarchy)
        {
            enemyMovement.ResumeAI();
        }

        if (bossMovement != null &&
            bossMovement.enabled &&
            gameObject.activeInHierarchy)
        {
            bossMovement.ResumeAI();
        }
    }

    // =====================================================
    // FORCE REMOVE
    // =====================================================

    public void RemoveFreezeImmediately()
    {
        if (!frozen)
            return;

        EndFreeze();
    }

    // =====================================================
    // DISABLE
    // =====================================================

    private void OnDisable()
    {
        frozen = false;
        freezeTimer = 0f;

        if (rb != null &&
            constraintsStored)
        {
            rb.constraints =
                originalConstraints;

            rb.linearVelocity =
                Vector2.zero;

            rb.angularVelocity =
                0f;

            constraintsStored = false;
        }

        if (activeFreezeVFX != null)
        {
            Destroy(
                activeFreezeVFX
            );

            activeFreezeVFX = null;
        }
    }

    // =====================================================
    // VALIDATION
    // =====================================================

    private void OnValidate()
    {
        defaultFreezeDuration =
            Mathf.Max(
                0.1f,
                defaultFreezeDuration
            );

        freezeVolume =
            Mathf.Clamp01(
                freezeVolume
            );

        unfreezeVolume =
            Mathf.Clamp01(
                unfreezeVolume
            );
    }
}