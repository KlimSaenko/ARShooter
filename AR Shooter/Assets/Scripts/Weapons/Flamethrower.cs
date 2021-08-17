using System.Collections;
using Mobs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class Flamethrower : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.Flamethrower;
        public override WeaponType WeaponType => CurrentWeaponType;

        private Transform MainCam => Camera.main.transform;

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
        
        private readonly RaycastHit[] _hits = new RaycastHit[16];

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            var forward = MainCam.forward;
            var camPosition = MainCam.position;
            
            var hitsCount = Physics.CapsuleCastNonAlloc(camPosition + forward * 0.5f, camPosition + forward * 3.5f,
                UI.AimInstance.CurrentAimSpreadDiameter / 2200f, forward, _hits, 0);

            for (var i = 0; i < hitsCount; i++)
            {
                if (!_hits[i].collider.TryGetComponent(out HitZone enemy)) continue;

                StartCoroutine(ApplyDamage(Vector3.Distance(transform.position, _hits[i].collider.transform.position) * 0.1f, enemy));
            }
            
            UI.AimInstance.AimAnimation();
        }

        private IEnumerator ApplyDamage(float delay, HitZone enemy)
        {
            yield return new WaitForSeconds(delay);
            
            var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
            
            enemy.ApplyDamage(damage);
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
