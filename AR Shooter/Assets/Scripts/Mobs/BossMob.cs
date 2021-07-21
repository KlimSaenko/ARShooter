using System.Collections;
using System.Collections.Generic;
using Mobs;
using UnityEngine;

public class BossMob : MainMob
{
    [SerializeField] private RectTransform hpImage;

    private void Awake()
    {
        MobAppeared();
    }

    private void Update()
    {
        if (!UI.IsPaused && PlayerStatusScript.IsAlive)
        {
            Vector3 moveDir = TargetPos - MobTransform.position;

            MobMove(moveDir, 0.55f);
        }
    }

    private void FixedUpdate()
    {
        hpImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HP / (float)mobHP * 558);
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
        else MobSpeed = 0.32f;
    }

    public override IEnumerator Death(int deathType)
    {
        mobAnimator.SetTrigger("Death Trigger");

        yield return new WaitForSeconds(3.4f);

        MobSpeed = 0.32f;
        HP = mobHP;

        foreach (Collider collider in colliders) collider.enabled = true;
        gameObject.SetActive(false);
    }
}
