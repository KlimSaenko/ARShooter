using System.Collections;
using System.Collections.Generic;
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
        if (!UI.isPaused && playerStatusScript.IsAlive)
        {
            Vector3 moveDir = targetPos - mobTransform.position;

            MobMove(moveDir, 0.55f);
        }
    }

    private void FixedUpdate()
    {
        hpImage.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, HP / (float)mobHP * 558);
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
        else mobSpeed = 0.32f;
    }

    public override IEnumerator Death(int deathType)
    {
        mobAnimator.SetTrigger("Death Trigger");

        yield return new WaitForSeconds(3.4f);

        mobSpeed = 0.32f;
        HP = mobHP;

        foreach (Collider collider in colliders) collider.enabled = true;
        gameObject.SetActive(false);
    }
}
