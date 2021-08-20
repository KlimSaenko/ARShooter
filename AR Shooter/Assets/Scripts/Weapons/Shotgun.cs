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
            var currentRays = new Ray[4];
            for (var i = 0; i < currentRays.Length; i++)
            {
                currentRays[i] = UI.AimInstance.GetRay();
            }
            
            if (Config.CurrentGameplayMode == Config.GameplayMode.Virtual)
            {
                foreach (var currentRay in currentRays)
                {
                    if (!Physics.Raycast(currentRay, out var hitInfo) ||
                        !hitInfo.transform.gameObject.TryGetComponent(out HitZone hitZone)) continue;
                    
                    var damage = Random.Range(weaponStats.damageMin, weaponStats.damageMax + 1);
                        
                    hitZone.ApplyDamage(damage, hitInfo.point);
                }
            }
            else
            {
                RealTargetHit(currentRays);
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
            Vibration.VibratePeek();
            
            if (AudioSource is null) return;
            
            AudioSource.loop = false;
            AudioSource.volume = 1f;
            AudioSource.pitch = Random.Range(0.94f, 1.06f);
            AudioSource.PlayOneShot(shootAudio);
        }
    }
}
