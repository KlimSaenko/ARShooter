using System.Collections;
using Mobs;
using UnityEngine;

namespace Weapons
{
    public class Flamethrower : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.Flamethrower;
        public override WeaponType WeaponType => CurrentWeaponType;

        private ParticleSystem[] _subParticles;

        private protected override IEnumerator Shooting()
        {
            _subParticles ??= flashParticle.gameObject.GetComponentsInChildren<ParticleSystem>();

            var emission = flashParticle.emission;
            emission.enabled = true;

            foreach (var subParticle in _subParticles)
            {
                emission = subParticle.emission;
                emission.enabled = true;
            }
            
            while (IsFiring && CanShoot)
            {
                VisualizeFiring();

                RunWeaponLogic();

                BulletCount--;

                yield return new WaitWhile(LogicIsRunning);
            }
            
            emission = flashParticle.emission;
            emission.enabled = false;

            foreach (var subParticle in _subParticles)
            {
                emission = subParticle.emission;
                emission.enabled = false;
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            foreach (var triggeredEnemy in FlamethrowerTrigger.TriggeredEnemies)
            {
                var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
                
                triggeredEnemy.ApplyDamage(damage);
            }
            
            // var currentRay = UI.AimInstance.GetRay();
            //
            // if (Physics.Raycast(currentRay, out var hitInfo) && 
            //     hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
            // {
            //     var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
            //     
            //     hitZone.ApplyDamage(damage, hitInfo.point);
            // }
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
        
        protected override void VisualizeFiring()
        {
            shootAnimation.Play();
            
            if (AudioSource is null) return;
            
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
