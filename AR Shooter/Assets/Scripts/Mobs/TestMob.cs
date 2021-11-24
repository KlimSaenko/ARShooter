using System.Collections;
using UnityEngine;
using Game.Managers;

namespace Game.Mobs
{
    public class TestMob : MainMob
    {
        [Space, Header("Test Mob")]
        [SerializeField] private GameObject mobGO;
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
            yield return new WaitForSeconds(2);

            while (mobMarker.DistanceToPlayer < 1.2f)
                yield return new WaitForEndOfFrame();

            NotificationManager.SkipNotification();

            mobGO.SetActive(true);
            activationParticles.Play();
            _loaded = true;
        }

        private static bool _loaded;
        internal static bool Loaded() => _loaded;
        internal static float PercentHP = 1;

        public override void ApplyDamage(int damage)
        {
            if (!IsAlive) return;

            HP -= damage;
            PercentHP = HP / (float)mobHP;
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
            var delta = MobRotate(0.18f);
            mobAnimator.SetFloat(moveBlendId, Mathf.Pow(delta, 0.5f));
        }
    }
}
