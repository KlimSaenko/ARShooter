using System.Collections;
using Game.Managers;
using Game.Mobs;
using Game.UI;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.Weapons
{
    public class Flamethrower : MainWeapon
    {
        private const WeaponName CurrentWeaponName = WeaponName.Flamethrower;
        public override WeaponName WeaponName => CurrentWeaponName;

        private static Transform MainCam => Camera.main.transform;

        private ParticleSystem[] _subParticles;

        //private protected override IEnumerator Shooting()
        //{
        //    _subParticles ??= flashParticle.gameObject.GetComponentsInChildren<ParticleSystem>();

        //    var emission = flashParticle.emission;
        //    emission.enabled = true;

        //    foreach (var subParticle in _subParticles)
        //    {
        //        emission = subParticle.emission;
        //        emission.enabled = true;
        //    }
            
        //    AudioSource.loop = true;
        //    DOTween.Kill(TweenId);
        //    AudioSource.volume = 1f;
        //    AudioSource.pitch = Random.Range(0.94f, 1.06f);
        //    AudioSource.clip = shootAudio;
        //    AudioSource.Play();
            
        //    while (IsFiring && CanShoot)
        //    {
        //        VisualizeFiring();

        //        RunWeaponLogic();

        //        BulletCount--;

        //        yield return new WaitWhile(LogicIsRunning);
        //    }
            
        //    emission = flashParticle.emission;
        //    emission.enabled = false;

        //    foreach (var subParticle in _subParticles)
        //    {
        //        emission = subParticle.emission;
        //        emission.enabled = false;
        //    }
            
        //    AudioSource.DOFade(0, 0.2f).OnComplete(() => AudioSource.Stop()).SetId(TweenId);
        //}

        private const int TweenId = 101;
        
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            if (Config.CurrentGameplayMode == Config.GameplayMode.Virtual)
            {
                // var forward = MainCam.forward;
                // var camPosition = MainCam.position;
                //
                // var hitsCount = Physics.CapsuleCastNonAlloc(camPosition + forward * 0.5f, camPosition + forward * 3.5f,
                //     UI.AimInstance.CurrentAimSpreadDiameter / 2200f, forward, _hits, 0);
                //
                // for (var i = 0; i < hitsCount; i++)
                // {
                //     if (!_hits[i].collider.TryGetComponent(out HitZone enemy)) continue;
                //
                //     StartCoroutine(ApplyDamage(Vector3.Distance(transform.position, _hits[i].collider.transform.position) * 0.1f, enemy));
                // }

                StartCoroutine(ShootingPatterns.ProcessCapsuleRay(CommonUI.Aim.CurrentAimSpreadDiameter / 2200f, 3.5f, 0.1f));
            }
            else
            {
                var screenPoint = new Vector2(Screen.width / 2, Screen.height / 2);
                var currentRay = Camera.main.ScreenPointToRay(screenPoint);
            
                var hitZoneTypes = HumanRecognitionVisualizer.Instance.ProcessRaycast(new []{ screenPoint }, out var distance);
            
                if (distance is < 0.05f or > 3.3f) return;
            
                if ((HitZone.ZoneType)hitZoneTypes[0] == HitZone.ZoneType.None) return;

                StartCoroutine(RealApplyDamage(distance * 0.1f, currentRay.GetPoint(distance), (HitZone.ZoneType)hitZoneTypes[0]));
            }
            
            Aim.AimAnimation();
        }

        private IEnumerator ApplyDamage(float delay, HitZone enemy)
        {
            yield return new WaitForSeconds(delay);
            
            var damage = Random.Range(Stats.damageMin, Stats.damageMax + 1);
            
            enemy.ApplyDamage(damage);
        }
        
        private IEnumerator RealApplyDamage(float delay, Vector3 pos, HitZone.ZoneType hitZoneType)
        {
            yield return new WaitForSeconds(delay);
            
            var damage = Random.Range(Stats.damageMin, Stats.damageMax + 1);
                                                            
            Pool.Decals.ActivateHitMarker(pos, damage, hitZoneType, false);
        }
        
        //protected override bool LogicIsRunning() =>
        //    shootAnimator.isPlaying;
        
        protected override void VisualizeFiring()
        {
            //shootAnimator.Play();
        }
    }
}
