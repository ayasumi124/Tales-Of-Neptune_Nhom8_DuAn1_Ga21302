using UnityEngine;

public class BossAnimationEventRelay : MonoBehaviour
{
    private EnermyAudio enemyAudio;
    private MinotaurosBossAttack bossAttack;

    private void Awake()
    {
        enemyAudio =
            GetComponentInParent<EnermyAudio>();

        bossAttack =
            GetComponentInParent<
                MinotaurosBossAttack
            >();

        if (enemyAudio == null)
        {
            Debug.LogWarning(
                $"{name}: không tìm thấy EnermyAudio ở parent."
            );
        }

        if (bossAttack == null)
        {
            Debug.LogWarning(
                $"{name}: không tìm thấy " +
                "MinotaurosBossAttack ở parent."
            );
        }
    }

    // =====================================================
    // AUDIO EVENTS
    // =====================================================

    public void PlayAttackVoice()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayAttackVoice();
        }
    }

    public void PlayAttackSwing()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayAttackSwing();
        }
    }

    public void PlayAttackImpact()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayAttackImpact();
        }
    }

    public void PlayHurtVoice()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayHurtVoice();
        }
    }

    public void PlayHurtImpact()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayHurtImpact();
        }
    }

    public void PlayDeathVoice()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayDeathVoice();
        }
    }

    public void PlayDeathImpact()
    {
        if (enemyAudio != null)
        {
            enemyAudio.PlayDeathImpact();
        }
    }

    // =====================================================
    // NORMAL ATTACK EVENTS
    // =====================================================

    public void DealNormalDamage()
    {
        if (bossAttack != null)
        {
            bossAttack.DealNormalDamage();
        }
    }

    // =====================================================
    // GROUND SLAM EVENTS
    // =====================================================

    public void GroundSlamImpact()
    {
        if (bossAttack != null)
        {
            bossAttack.GroundSlamImpact();
        }
    }

    // =====================================================
    // END ACTION
    // =====================================================

    public void EndAction()
    {
        if (bossAttack != null)
        {
            bossAttack.EndAction();
        }
    }
}