using Mobs;
using UnityEngine;

namespace Weapons
{
    public class Shotgun : MainWeapon
    {
        private const WeaponType CurrentWeaponType = WeaponType.Shotgun;
        public override WeaponType WeaponType => CurrentWeaponType;

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void RunWeaponLogic()
        {
            for (var i = 0; i < 4; i++)
            {
                var currentRay = UI.AimInstance.GetRay();
                
                if (Physics.Raycast(currentRay, out var hitInfo) && 
                    hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone))
                {
                    var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
                    
                    hitZone.ApplyDamage(damage, hitInfo.point);
                }
            }
            
            UI.AimInstance.AimAnimation();
        }
        
        protected override bool LogicIsRunning() =>
            shootAnimation.isPlaying;

        private void OnShellAnimation() =>
            shellsParticle.Play();
        
        protected override void VisualizeFiring()
        {
            shootAnimation.Play();
            flashParticle.Play(true);
            
            if (AudioSource is null) return;
            
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
