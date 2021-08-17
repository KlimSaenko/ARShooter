using Mobs;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Weapons
{
    public class M4_Carbine : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.M4_Carabine;
        public override WeaponType WeaponType => CurrentWeaponType;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            var currentRay = UI.AimInstance.GetRay();
            
            if (Physics.Raycast(currentRay, out var hitInfo) && 
                hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
            {
                var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
                
                hitZone.ApplyDamage(damage, hitInfo.point);
            }
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;
        
        protected override void VisualizeFiring()
        {
            shellsParticle.Play();
            shootAnimation.Play();
            flashParticle.Play(true);
            
            if (AudioSource is null) return;
            
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
