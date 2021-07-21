using Mobs;
using UnityEngine;

public class SimpleMob : MainMob
{
    private void Awake()
    {
        MobAppeared();
    }

    private void Update()
    {
        if (UI.IsPaused || !PlayerStatusScript.IsAlive) return;
        
        var moveDir = TargetPos - MobTransform.position;

        MobMove(moveDir, 0.33f);
    }

    protected override void Attack(bool start)
    {
        IsAttacking = start;
        mobAnimator.SetBool("Attack", start);

        if (start)
        {
            PlayerStatusScript.ApplyDamage(mobDamage);
            DamageTimeDelay = 0.7f;
        }
        else MobSpeed = 0.35f;
    }
}
