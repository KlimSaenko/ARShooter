using UnityEngine;

public class SimpleMob : MainMob
{
    private void Awake()
    {
        MobAppeared();
    }

    private void Update()
    {
        if (!UI.isPaused && playerStatusScript.IsAlive)
        {
            Vector3 moveDir = targetPos - mobTransform.position;

            MobMove(moveDir, 0.33f);
        }
    }

    protected override void Attack(bool start)
    {
        isAttacking = start;
        mobAnimator.SetBool("Attack", start);

        if (start)
        {
            playerStatusScript.ApplyDamage(mobDamage);
            damageTimeDelay = 0.7f;
        }
        else mobSpeed = 0.35f;
    }
}
