using System.Collections;
using UnityEngine;
using Game.Managers;

namespace Game.Mobs
{
    public class TestMob : MainMob
    {
        [Space, Header("Test Mob")]
        [SerializeField] private ParticleSystem activationParticles;
        [SerializeField] private MobMarker mobMarker;

        private protected override Transform _mobTransform => transform.GetChild(0);
        private protected override Quaternion CurrentLookRotation => Quaternion.Euler(0, mobMarker.transform.eulerAngles.y, 0);

        private protected override void OnMobActivated()
        {
            StartCoroutine(MobLoad());
        }

        private IEnumerator MobLoad()
        {
            while (mobMarker.DistanceToPlayer < 0.8f)
                yield return new WaitForEndOfFrame();

            NotificationManager.SkipNotification();

            activationParticles.Play();
            _loaded = true;
        }

        private bool _loaded;
        internal override bool Loaded() => _loaded;

        public override void ApplyDamage(int damage)
        {
            if (!IsAlive) return;

            HP -= damage;
            mobMarker.ChangeHP(PercentHP);

            if (IsAlive)
            {
                mobAnimator.SetFloat(blendId, Random.value);
                mobAnimator.SetTrigger(getDamageId);
            }
            else
            {
                mobAnimator.SetTrigger(deathId);
            }
        }

        private void Update()
        {
            var delta = MobLookRotate(0.18f);
            mobAnimator.SetFloat(moveBlendId, Mathf.Pow(delta, 0.6f) * 0.9f);
        }
    }
}
